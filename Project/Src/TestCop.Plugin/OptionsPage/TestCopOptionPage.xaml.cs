// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

using JetBrains;
using JetBrains.Annotations;
using JetBrains.Application.Icons;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.Components;
using JetBrains.Application.UI.Options;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.Application.UI.UIAutomation;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.LiveTemplates.UI;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.UI.Extensions;
using JetBrains.Util;
using JetBrains.Util.Logging;

using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;

namespace TestCop.Plugin.OptionsPage
{
    using JetBrains.ReSharper.Resources.Shell;

    using System.Collections.Generic;

    [OptionsPage(PID, "TestCop ", typeof(UnnamedThemedIcons.Agent16x16), ParentId = ToolsPage.PID)]
    public partial class TestCopOptionPage : IOptionsPage
    {
        private readonly Lifetime _lifetime;
        private readonly OptionsSettingsSmartContext _settings;
        private readonly TemplateScopeManager _scopeManager;
        private readonly StoredTemplatesProvider _storedTemplatesProvider;
        private readonly ILiveTemplatesUIHelper _templatesUiHelper;
        private readonly IUIApplication _application;
        private readonly ISolution _solution;
        private const string PID = "TestCopPageId";
        private const string TestClassSuffixValidationRegEx = "^[_a-zA-Z,]*$";
        private const string ValidationRegEx = "^[\\$\\.a-zA-Z1-9]*$";
        private readonly FileTemplatesManager _fileTemplatesManager;
        private readonly ILogger _logger;

        public TestCopOptionPage(Lifetime lifetime, OptionsSettingsSmartContext settings, TemplateScopeManager scopeManager
            , IThemedIconManager iconManager, IUIApplication application
          , StoredTemplatesProvider storedTemplatesProvider, ILiveTemplatesUIHelper templatesUiHelper, FileTemplatesManager fileTemplatesManager, ISolution solution = null)
        {
            _lifetime = lifetime;
            _settings = settings;
            _scopeManager = scopeManager;
            _application = application;
            _solution = solution;
            _fileTemplatesManager = fileTemplatesManager;
            _storedTemplatesProvider = storedTemplatesProvider;
            _templatesUiHelper = templatesUiHelper;
            _logger = Logger.GetLogger<TestCopOptionPage>();

            InitializeComponent();
            TestFileAnalysisSettings testFileAnalysisSettings = settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.DoMeSlowly);

            InitializeComponent();
            BuildTestStrategyCombo(testFileAnalysisSettings);

            //Do this first as it is reference by other display fields
            testClassSuffixTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.TestClassSuffix), TestClassSuffixValidationRegEx);

            //Regex Config for Multiple Test Assembly Logic via project naming
            testProjectNameRegExTextBox.BindWithValidationMustBeARegex(testFileAnalysisSettings, P(x => x.TestProjectNameToCodeProjectNameRegEx));
            testProjectNameRegExReplaceTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.TestProjectNameToCodeProjectNameRegExReplace), ValidationRegEx);

            //Regex Config for Multiple Test Assembly Logic via namespace naming
            testNamespaceRegExTextBox.BindWithValidationMustBeARegex(testFileAnalysisSettings, P(x => x.TestProjectToCodeProjectNameSpaceRegEx));
            testNamespaceRegExReplaceTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.TestProjectToCodeProjectNameSpaceRegExReplace), ValidationRegEx);

            //Regex Config for Single Test Assembly Logic
            SingleTestNamespaceRegExTextBox.BindWithValidationMustBeARegex(testFileAnalysisSettings, P(x => x.SingleTestRegexTestToAssembly));
            SingleTestNamespaceToAssemblyRegExReplaceTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.SingleTestRegexTestToAssemblyProjectReplace), ValidationRegEx);
            SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace), ValidationRegEx);
            SingleTestCodeNamespaceRegExTextBox.BindWithValidationMustBeARegex(testFileAnalysisSettings, P(x => x.SingleTestRegexCodeToTestAssembly));
            SingleTestCodeNamespaceToTestRegExReplaceTextBox.BindWithRegexMatchesValidation(testFileAnalysisSettings, P(x => x.SingleTestRegexCodeToTestReplace), ValidationRegEx);
            //

            foreach (string testingAttribute in testFileAnalysisSettings.TestingAttributes())
            {
                this.testingAttributesListBox.Items.Add(testingAttribute);
            }
            foreach (string bddPrefix in testFileAnalysisSettings.BddPrefixes())
            {
                this.contextPrefixesListBox.Items.Add(bddPrefix);
            }

            OrphanedFilesPatternsTextBox.Text = testFileAnalysisSettings.OrphanedFilesPatterns;

            BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, codeTemplateTextBox, P(x => x.CodeFileTemplateName));
            BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, unitTestTemplateTextBox, P(x => x.UnitTestFileTemplateName));

            ShowAllTestsWithUsageCheckBox.IsChecked = testFileAnalysisSettings.FindAnyUsageInTestAssembly;
            CheckTestNamespaces.IsChecked = testFileAnalysisSettings.CheckTestNamespaces;
            CheckSearchForOrphanedCodeFiles.IsChecked = testFileAnalysisSettings.FindOrphanedProjectFiles;
            SupportRenameRefactor.IsChecked = testFileAnalysisSettings.SupportRenameRefactor;

            OutputPanelOpenOnKeyboardMapping.IsChecked = testFileAnalysisSettings.OutputPanelOpenOnKeyboardMapping;

            TestCopLogoImage.Source =
            (ImageSource)new BitmapToImageSourceConverter().Convert(
                iconManager.Icons[UnnamedThemedIcons.Agent64x64.Id].CurrentGdipBitmap96, null, null, null);
        }

        private void BuildTestStrategyCombo(TestFileAnalysisSettings testFileAnalysisSettings)
        {
            MultiTestRegex.Tag = MultiTestRegexHelp.Tag = TestProjectStrategy.TestProjectPerCodeProject;
            SingleTestRegex.Tag = SingleTestRegexHelp.Tag = TestProjectStrategy.SingleTestProjectPerSolution;
            MultiTestSameNamespaceRegex.Tag = TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject;

            TestCopStrategyCombo.Items.Clear();

            foreach (TestProjectStrategy value in Enum.GetValues(typeof(TestProjectStrategy)).Cast<TestProjectStrategy>())
            {
                ListBoxItem item = new ListBoxItem() { Content = value.GetDescription(), Tag = value };
                TestCopStrategyCombo.Items.Add(item);

                if (value == testFileAnalysisSettings.TestCopProjectStrategy)
                {
                    TestCopStrategyCombo.SelectedItem = item;
                }
            }
        }

        private void BindWithValidationMustBeAFileTemplate(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property)
        {
            IContextBoundSettingsStore boundSettingsStore = _application.Settings.BindToContextTransient(ContextRange.ApplicationWide);

            Binding binding = new Binding { Path = new PropertyPath(property) };
            IsAFileTemplateValidationRule rule = new IsAFileTemplateValidationRule(_lifetime, _storedTemplatesProvider, boundSettingsStore);

            binding.ValidationRules.Add(rule);
            binding.NotifyOnValidationError = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            tb.DataContext = testFileAnalysisSettings;
            tb.SetBinding(TextBox.TextProperty, binding);
        }

        private static string P<T>(Expression<Func<TestFileAnalysisSettings, T>> expression)
        {
            if (expression.Body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("Expression is not a member access", "expression");
        }

        public EitherControl Control => this;

        public string Id => PID;

        public bool OnOk()
        {
            if (Validation.GetHasError(testNamespaceRegExTextBox)) return false;
            if (Validation.GetHasError(testClassSuffixTextBox)) return false;
            if (Validation.GetHasError(unitTestTemplateTextBox)) return false;
            if (Validation.GetHasError(codeTemplateTextBox)) return false;

            if (Validation.GetHasError(SingleTestNamespaceRegExTextBox)) return false;
            if (Validation.GetHasError(SingleTestNamespaceToAssemblyRegExReplaceTextBox)) return false;
            if (Validation.GetHasError(SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox)) return false;
            if (Validation.GetHasError(SingleTestCodeNamespaceRegExTextBox)) return false;
            if (Validation.GetHasError(SingleTestCodeNamespaceToTestRegExReplaceTextBox)) return false;

            string attributes = testingAttributesListBox.Items.Cast<string>().ToList().Join(",");
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestingAttributeText, attributes);

            attributes = contextPrefixesListBox.Items.Cast<string>().ToList().Join(",");
            _settings.SetValue((TestFileAnalysisSettings s) => s.BddPrefix, attributes);

            _settings.SetValue((TestFileAnalysisSettings s) => s.FindAnyUsageInTestAssembly, ShowAllTestsWithUsageCheckBox.IsChecked ?? false);
            _settings.SetValue((TestFileAnalysisSettings s) => s.CheckTestNamespaces, CheckTestNamespaces.IsChecked ?? false);
            _settings.SetValue((TestFileAnalysisSettings s) => s.FindOrphanedProjectFiles, CheckSearchForOrphanedCodeFiles.IsChecked ?? false);

            _settings.SetValue((TestFileAnalysisSettings s) => s.OutputPanelOpenOnKeyboardMapping, OutputPanelOpenOnKeyboardMapping.IsChecked ?? false);

            ListBoxItem selectedItem = TestCopStrategyCombo.SelectedItem as ListBoxItem ?? new ListBoxItem() { Tag = TestProjectStrategy.TestProjectPerCodeProject };
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestCopProjectStrategy, selectedItem.Tag);


            // RegEx Config for Multi Test via project naming
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectNameToCodeProjectNameRegEx, testProjectNameRegExTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectNameToCodeProjectNameRegExReplace, testProjectNameRegExReplaceTextBox.Text.Replace(" ", ""));
            // Regex Config for Multi Test Assembly Logic via project namespace
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestClassSuffix, testClassSuffixTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegEx, testNamespaceRegExTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegExReplace, testNamespaceRegExReplaceTextBox.Text.Replace(" ", ""));
            // Regex Config for Single Test Assembly Logic
            _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssembly, SingleTestNamespaceRegExTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssemblyProjectReplace, SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexCodeToTestAssembly, SingleTestCodeNamespaceRegExTextBox.Text.Replace(" ", ""));
            _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexCodeToTestReplace, SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text.Replace(" ", ""));
            //
            _settings.SetValue((TestFileAnalysisSettings s) => s.CodeFileTemplateName, codeTemplateTextBox.Text);
            _settings.SetValue((TestFileAnalysisSettings s) => s.UnitTestFileTemplateName, unitTestTemplateTextBox.Text);

            _settings.SetValue((TestFileAnalysisSettings s) => s.SupportRenameRefactor, SupportRenameRefactor.IsChecked ?? false);
            _settings.SetValue((TestFileAnalysisSettings s) => s.OrphanedFilesPatterns, OrphanedFilesPatternsTextBox.Text);

            return true;
        }

        public bool ValidatePage()
        {
            return true;
        }

        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            AddItemFromTextBox(attributeTextBox, testingAttributesListBox);
        }

        private void BtnRemoveClick(object sender, RoutedEventArgs e)
        {
            RemoveAndClearItem(attributeTextBox, testingAttributesListBox);
        }

        private void TestingAttributesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            attributeTextBox.Text = testingAttributesListBox.SelectedItem != null
                                        ? testingAttributesListBox.SelectedItem.ToString()
                                        : "";
        }

        private void BtnAddContextClick(object sender, RoutedEventArgs e)
        {
            AddItemFromTextBox(contextTextBox, contextPrefixesListBox);
        }

        private void BtnRemoveContextClick(object sender, RoutedEventArgs e)
        {
            RemoveAndClearItem(contextTextBox, contextPrefixesListBox);
        }

        private void ContextPrefixesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            contextTextBox.Text = contextPrefixesListBox.SelectedItem != null ? contextPrefixesListBox.SelectedItem.ToString() : "";
        }

        private static void AddItemFromTextBox(TextBox tb, ItemsControl lb)
        {
            lb.Items.Add(tb.Text);
            tb.Clear();
        }

        private static void RemoveAndClearItem(TextBox tb, ItemsControl lb)
        {
            if (lb.Items.Contains(tb.Text))
            {
                lb.Items.Remove(tb.Text);
                tb.Clear();
            }
        }

        private void ProjectNameRegexTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock outcomeTexBox = regExProjectOutcome;

            Regex regEx;

            tbProjectSuffixGuidance.Text = 
                $"The configuration below defines that the project name of all test assemblies must match the RegEx '{this.testProjectNameRegExTextBox.Text}'. " +
                "Use brackets to extract the associated code project name. The namespace of the project and associated test project must be the same. ";
            try
            {
                outcomeTexBox.Text = "";
                regEx = new Regex(testProjectNameRegExTextBox.Text);
            }
            catch (Exception) { return; }

            if (regEx.GetGroupNames().Length < 2)
            {
                outcomeTexBox.Text = "RegEx must contain at least one regex group ().";
                return;
            }

            if (_solution != null)
            {
                ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestCopOptionsPage", () =>
                {
                    List<IProject> testProjects = _solution.GetAllCodeProjects().Select(p => p).Where(p => regEx.IsMatch(p.Name ?? "")).ToList();
                    outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the NAME of any loaded projects.";

                }).Invoke();
            }
        }

        private void TestClassSuffixTextChanged(object sender, TextChangedEventArgs e)
        {
            testClassSuffixTextBox.ToolTip = "Valid class names for your config : {0}".FormatEx(GetSampleClassNames());
        }

        private void MultiTestClassAndNamespaceTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock outcomeTexBox = regExOutcome;

            Regex regEx;

            tbSuffixGuidance.Text = "The configuration below defines that the namespace of all test assemblies must match " +
                                    $"the RegEx '{this.testNamespaceRegExTextBox.Text}'. Use brackets to extract the associated code project namespace.";
            try
            {
                outcomeTexBox.Text = "";
                regEx = new Regex(testNamespaceRegExTextBox.Text);
            }
            catch (Exception) { return; }

            if (regEx.GetGroupNames().Length < 2)
            {
                regExOutcome.Text = "RegEx must contain at least one regex group ().";
                return;
            }

            if (_solution != null)
            {
                ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestCopOptionsPage", () =>
                {
                    List<IProject> testProjects = _solution.GetAllCodeProjects().Select(p => p).Where(p => regEx.IsMatch(p.GetDefaultNamespace() ?? "")).ToList();
                    outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the NAMESPACE of any loaded projects.";

                }).Invoke();
            }
        }

        private string GetSampleClassNames()
        {
            string sampleFileNames = "";
            foreach (string s in this.testClassSuffixTextBox.Text.Split(','))
            {
                sampleFileNames = sampleFileNames.AppendIfNotNull(" ,", "ClassA{0}, ClassA.Security{0}".FormatEx(s));
            }

            return sampleFileNames;
        }

        private void SingleTestClassAndNamespaceTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock outcomeTexBox = SingleTestRegExOutcome;

            Regex regEx;

            tbSingleTestSuffixGuidanceOne.Text = 
                $"The configuration below defines that the namespace of test classes must match the RegEx '{this.SingleTestNamespaceRegExTextBox.Text}'. " +
                $"Use brackets to extract the associated code project namespace. The replace string '{this.SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text}' " +
                $"will be used to identify the code project namespace and the replace string '{this.SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text}' " +
                "to build the sub namespace within the code project. \n \n";

            tbSingleTestSuffixGuidanceTwo.Text =
                "The configuration below defines the sections(s) of the code class namespace that will map to sub-namespace within the test project by extracting " +
                $"'{this.SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text}' from the RegEx '{this.SingleTestCodeNamespaceRegExTextBox.Text}' when it is applied " +
                "to code files namespace.\n \n";

            try
            {
                outcomeTexBox.Text = "";
                regEx = new Regex(SingleTestNamespaceRegExTextBox.Text);
            }
            catch (Exception) { return; }

            if (regEx.GetGroupNames().Length < 2)
            {
                regExOutcome.Text = "RegEx must contain at least one regex group ().";
                return;
            }

            if (_solution != null)
            {
                ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestCopOptionsPage", () =>
                {
                    List<IProject> testProjects = _solution.GetAllCodeProjects()
                        .Select(p => p).Where(p => regEx.IsMatch((p.GetDefaultNamespace() ?? "NS2") + ".NS1")).ToList();
                    outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the namespace of any loaded projects.";

                }).Invoke();
            }
        }

        private void DisplayLoadProjectTip()
        {
            LoadProjectToSelectFileTemplate.Text = "Note: A project/solution needs to be loaded.";
            LoadProjectToSelectFileTemplate.Visibility = Visibility.Visible;
            LoadProjectToSelectFileTemplate.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void FileTemplateSelectFromList(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_solution == null)
            {
                ResharperHelper.AppendLineToOutputWindow(_solution?.Locks, "Unable to identify current solution.");
                DisplayLoadProjectTip();
                return;
            }

            IProject project = _solution.GetAllCodeProjects().FirstOrDefault();

            if (project == null)
            {
                ResharperHelper.AppendLineToOutputWindow(_solution.Locks, "Unable to identify a code project.");
                DisplayLoadProjectTip();
                return;
            }

            this._templatesUiHelper.ChooseTemplate(this._settings.ToImplementation(),
                this._fileTemplatesManager.QuickListSupports, TemplateApplicability.File,
                template => { ((TextBox)sender).Text = template.Description; });
        
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            SingleTestNamespaceRegExTextBox.Text =
              SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssembly, _logger) ?? string.Empty;
            SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text =
              SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssemblyProjectReplace, _logger) ?? string.Empty;
            SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text =
              SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, _logger) ?? string.Empty;
            SingleTestCodeNamespaceRegExTextBox.Text =
              SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexCodeToTestAssembly, _logger) ?? string.Empty;
            SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text =
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexCodeToTestReplace, _logger) ?? string.Empty;
        }

        private void TestCopStrategyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem selectedItem = TestCopStrategyCombo.SelectedItem as ListBoxItem ?? new ListBoxItem() { Tag = TestProjectStrategy.TestProjectPerCodeProject };

            foreach (TabItem item in tabControl.Items.Cast<TabItem>().Where(i => i.Tag != null).Where(i => (int)i.Tag > 0))
            {
                item.Visibility = ((int)item.Tag) == ((int)selectedItem.Tag) ? Visibility.Visible : Visibility.Collapsed;
            }

            switch ((TestProjectStrategy)selectedItem.Tag)
            {
                case TestProjectStrategy.SingleTestProjectPerSolution:
                    tbStrategyOverview.Text
                      = "Each Visual Studio solution has only one test project for all code projects within it. " +
                      "You will need to define regular expressions (RegEx) to describe how the namespace of your code namespace " +
                      "maps to the namespace of the test within the single test project.";

                    AppendMoreInfoHyperLink(tbStrategyOverview
                        , "https://github.com/testcop/docs/blob/master/wiki/Single_Test_Project_Within_Solution.md");

                    break;

                case TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject:
                    tbStrategyOverview.Text
                      = "Each test project maps to a single code project through its project name." +
                      "To use this option the namespace of the code and test assembly must be the same. " +
                      "You will need to define regular expressions (RegEx) to describe how the project name of each Test project " +
                      "maps to the name of the code project.  For example : DalTests => Dal";

                    AppendMoreInfoHyperLink(tbStrategyOverview
                      , "https://github.com/testcop/docs/blob/master/wiki/Each_test_project_maps_to_a_code_project_via_project_name.md");

                    break;

                case TestProjectStrategy.TestProjectPerCodeProject:
                    tbStrategyOverview.Text
                      = "Each test project maps to a single code project through its namespace." +
                      "You will need to define regular expressions (RegEx) to describe how the namespace of each Test namespace " +
                      "maps to the namespace of the code project. For example : mycorp.myapp.tests.dal => mycorp.myapp.dal";

                    AppendMoreInfoHyperLink(tbStrategyOverview
                            , "https://github.com/testcop/docs/blob/master/wiki/Each_test_project_maps_to_a_code_project_via_namespace.md");

                    break;

                default:
                    tbStrategyOverview.Text = ".";
                    break;
            }

        }

        private static void AppendMoreInfoHyperLink(TextBlock tb, string uriString)
        {
            tb.Append(new System.Windows.Documents.LineBreak());

            System.Windows.Documents.Hyperlink hyperlink =
                new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run("More information..."))
                {
                    NavigateUri = new Uri(uriString)
                };

            tb.Append(hyperlink);
            hyperlink.RequestNavigate += (sender, args) => System.Diagnostics.Process.Start(args.Uri.AbsoluteUri);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
