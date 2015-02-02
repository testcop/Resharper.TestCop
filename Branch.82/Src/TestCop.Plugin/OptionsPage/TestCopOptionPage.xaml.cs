// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using JetBrains;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Support;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Features.Common.Options;
using JetBrains.ReSharper.LiveTemplates.Templates;
using JetBrains.ReSharper.LiveTemplates.UI;
using JetBrains.UI.Application;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Icons;
using JetBrains.UI.Options;
using JetBrains.Util;
using JetBrains.Util.Logging;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using Binding = System.Windows.Data.Binding;
using Control = System.Windows.Controls.Control;
using ListBox = System.Windows.Controls.ListBox;
using TextBox = System.Windows.Controls.TextBox;

namespace TestCop.Plugin.OptionsPage
{
  [OptionsPage(PID, "TestCop ", typeof(UnnamedThemedIcons.Agent16x16), ParentId = ToolsPage.PID)]  
  public partial class TestCopOptionPage : IOptionsPage
  {      
      private readonly Lifetime _lifetime;
      private readonly OptionsSettingsSmartContext _settings;
      private readonly StoredTemplatesProvider _storedTemplatesProvider;
      private readonly UIApplication _application;
      private readonly ISolution _solution;
      private const string PID = "TestCopPageId";
      private readonly FileTemplatesManager _fileTemplatesManager;
      
      public TestCopOptionPage(Lifetime lifetime, OptionsSettingsSmartContext settings
          , IThemedIconManager iconManager, UIApplication application
        , StoredTemplatesProvider storedTemplatesProvider, FileTemplatesManager fileTemplatesManager, ISolution solution = null)
      {
          _lifetime = lifetime;
          _settings = settings;
          _application = application;
          _solution = solution;
          _fileTemplatesManager = fileTemplatesManager;          
          _storedTemplatesProvider = storedTemplatesProvider;

          InitializeComponent();
          var testFileAnalysisSettings = settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.DoMeSlowly);
          
          InitializeComponent();
          BuildTestStrategyCombo(testFileAnalysisSettings);

          //Do this first as it is reference by other display fields
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testClassSuffixTextBox, P(x => x.TestClassSuffix), "^[_a-zA-Z,]*$");

          //Regex Config for Multiple Test Assemply Logic via project naming
          BindWithValidationMustBeARegex(testFileAnalysisSettings, testProjectNameRegExTextBox, P(x => x.TestProjectNameToCodeProjectNameRegEx));
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testProjectNameRegExReplaceTextBox, P(x => x.TestProjectNameToCodeProjectNameRegExReplace), "^[\\$\\.a-zA-Z1-9]*$");
          
          //Regex Config for Multiple Test Assemply Logic via namespace naming
          BindWithValidationMustBeARegex(testFileAnalysisSettings, testNamespaceRegExTextBox, P(x=>x.TestProjectToCodeProjectNameSpaceRegEx));          
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testNamespaceRegExReplaceTextBox, P(x=>x.TestProjectToCodeProjectNameSpaceRegExReplace), "^[\\$\\.a-zA-Z1-9]*$");
          //
          //Regex Config for Single Test Assemply Logic
          BindWithValidationMustBeARegex(testFileAnalysisSettings, SingleTestNamespaceRegExTextBox,                              P(x => x.SingleTestRegexTestToAssembly));
          BindWithRegexMatchesValidation(testFileAnalysisSettings, SingleTestNamespaceToAssemblyRegExReplaceTextBox,             P(x => x.SingleTestRegexTestToAssemblyProjectReplace), "^[\\$\\.a-zA-Z1-9]*$");
          BindWithRegexMatchesValidation(testFileAnalysisSettings, SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox, P(x => x.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace), "^[\\$\\.a-zA-Z1-9]*$");
          BindWithValidationMustBeARegex(testFileAnalysisSettings, SingleTestCodeNamespaceRegExTextBox,                          P(x => x.SingleTestRegexCodeToTestAssembly));
          BindWithRegexMatchesValidation(testFileAnalysisSettings, SingleTestCodeNamespaceToTestRegExReplaceTextBox,             P(x => x.SingleTestRegexCodeToTestReplace), "^[\\$\\.a-zA-Z1-9]*$");
          //          
          testFileAnalysisSettings.TestingAttributes().ForEach(p => testingAttributesListBox.Items.Add(p));
          testFileAnalysisSettings.BddPrefixes().ForEach(p => contextPrefixesListBox.Items.Add(p));

          SwitchBetweenFilesShortcutTextBox.Text = testFileAnalysisSettings.ShortcutToSwitchBetweenFiles;
          OrphanedFilesPatternsTextBox.Text = testFileAnalysisSettings.OrphanedFilesPatterns;

          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, codeTemplateTextBox, P(x=>x.CodeFileTemplateName));
          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, unitTestTemplateTextBox, P(x => x.UnitTestFileTemplateName));

          ShowAllTestsWithUsageCheckBox.IsChecked = testFileAnalysisSettings.FindAnyUsageInTestAssembly;
          CheckTestNamespaces.IsChecked = testFileAnalysisSettings.CheckTestNamespaces;
          CheckSearchForOrphanedCodeFiles.IsChecked = testFileAnalysisSettings.FindOrphanedProjectFiles;
          SupportRenameRefactor.IsChecked = testFileAnalysisSettings.SupportRenameRefactor;
          
          OutputPanelOpenOnKeyboardMapping.IsChecked = testFileAnalysisSettings.OutputPanelOpenOnKeyboardMapping;

          TestCopLogoImage.Source =
          (ImageSource) new BitmapToImageSourceConverter().Convert(
              iconManager.Icons[UnnamedThemedIcons.Agent64x64.Id].CurrentGdipBitmap96, null, null, null);          


      }

      private void BuildTestStrategyCombo(TestFileAnalysisSettings testFileAnalysisSettings)
      {
          MultiTestRegex.Tag = MultiTestRegexHelp.Tag = TestProjectStrategy.TestProjectPerCodeProject;
          SingleTestRegex.Tag = SingleTestRegexHelp.Tag = TestProjectStrategy.SingleTestProjectPerSolution;
          MultiTestSameNamespaceRegex.Tag = TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject;
          
          TestCopStrategyCombo.Items.Clear();

          foreach (var value in Enum.GetValues(typeof (TestProjectStrategy)).Cast<TestProjectStrategy>())
          {
              var item = new ListBoxItem() {Content = value.GetDescription(), Tag = value};
              TestCopStrategyCombo.Items.Add(item);

              if (value == testFileAnalysisSettings.TestCopProjectStrategy)
              {
                  TestCopStrategyCombo.SelectedItem = item;
              }
          }
      }

      private void BindWithValidationMustBeAFileTemplate(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property)
      {                    
          var boundSettingsStore = _application.Settings.BindToContextTransient(ContextRange.ApplicationWide);

          var binding = new Binding { Path = new PropertyPath(property) };
          var rule = new IsAFileTemplateValidationRule(_lifetime, _storedTemplatesProvider, boundSettingsStore);

          binding.ValidationRules.Add(rule);
          binding.NotifyOnValidationError = true;
          binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
          tb.DataContext = testFileAnalysisSettings;
          tb.SetBinding(TextBox.TextProperty, binding);
      }

      private static string P<T>(Expression<Func<TestFileAnalysisSettings, T>> expression)
      {
          var member = expression.Body as MemberExpression;

          if (member != null)
              return member.Member.Name;

          throw new ArgumentException("Expression is not a member access", "expression");
      }

      private static void BindWithRegexMatchesValidation(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property, string regexString)
      {         
          var binding = new Binding { Path = new PropertyPath(property) };
          var namespaceRule = new RegexValidationRule
          {
              RegexText = regexString,
              ErrorMessage = "Invalid suffix.",
              RegexOptions = RegexOptions.IgnoreCase,
              ValidatesOnTargetUpdated = true
          };

          binding.ValidationRules.Add(namespaceRule);
                    
          binding.NotifyOnValidationError = true;
          binding.ValidatesOnDataErrors = true;
          binding.ValidatesOnExceptions = true;

          binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
          tb.DataContext = testFileAnalysisSettings;
          tb.SetBinding(TextBox.TextProperty, binding);          
      }

      private static void BindWithValidationMustBeARegex(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property)
      {
          var binding = new Binding { Path = new PropertyPath(property)};
          var namespaceRule = new IsARegexValidationRule
          {              
              ErrorMessage = "Invalid Regex",
              RegexOptions = RegexOptions.IgnoreCase,
              MinimumGroupsInRegex = 2,
              ValidatesOnTargetUpdated = true              
          };

          binding.ValidationRules.Add(namespaceRule);
          binding.NotifyOnValidationError = true;
          binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
          tb.DataContext = testFileAnalysisSettings;
          tb.SetBinding(TextBox.TextProperty, binding);
      }

      public EitherControl Control
      {
          get { return this; }
      }

      public string Id
      {
          get { return PID; }
      }

      public bool OnOk()
      {
          if (Validation.GetHasError(testNamespaceRegExTextBox))return false;
          if (Validation.GetHasError(testClassSuffixTextBox))return false;
          if (Validation.GetHasError(unitTestTemplateTextBox)) return false;
          if (Validation.GetHasError(codeTemplateTextBox)) return false;

          if (Validation.GetHasError(SingleTestNamespaceRegExTextBox)) return false;
          if (Validation.GetHasError(SingleTestNamespaceToAssemblyRegExReplaceTextBox))return false;
          if (Validation.GetHasError(SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox))return false;
          if (Validation.GetHasError(SingleTestCodeNamespaceRegExTextBox))return false;
          if (Validation.GetHasError(SingleTestCodeNamespaceToTestRegExReplaceTextBox))return false;
       
          var attributes = testingAttributesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestingAttributeText, attributes);

          attributes = contextPrefixesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.BddPrefix, attributes);

          _settings.SetValue((TestFileAnalysisSettings s) => s.FindAnyUsageInTestAssembly,ShowAllTestsWithUsageCheckBox.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.CheckTestNamespaces, CheckTestNamespaces.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.FindOrphanedProjectFiles, CheckSearchForOrphanedCodeFiles.IsChecked);
          
          _settings.SetValue((TestFileAnalysisSettings s) => s.OutputPanelOpenOnKeyboardMapping, OutputPanelOpenOnKeyboardMapping.IsChecked);

          var selectedItem = TestCopStrategyCombo.SelectedItem as ListBoxItem ?? new ListBoxItem() { Tag = TestProjectStrategy.TestProjectPerCodeProject };
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestCopProjectStrategy, selectedItem.Tag);

          
          //RegEx Config for Multi Test via project naming
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectNameToCodeProjectNameRegEx,
                testProjectNameRegExTextBox.Text.Replace(" ", "")); 
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectNameToCodeProjectNameRegExReplace,
                testProjectNameRegExReplaceTextBox.Text.Replace(" ", ""));                    
          //Regex Config for Multi Test Assemply Logic via project namespace
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestClassSuffix,
                             testClassSuffixTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegEx,
                             testNamespaceRegExTextBox.Text.Replace(" ", "")); 
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegExReplace,
                             testNamespaceRegExReplaceTextBox.Text.Replace(" ", ""));          
          //Regex Config for Single Test Assemply Logic          
          _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssembly,
                   SingleTestNamespaceRegExTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssemblyProjectReplace,
                             SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace,
                   SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexCodeToTestAssembly,
                   SingleTestCodeNamespaceRegExTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.SingleTestRegexCodeToTestReplace,
                   SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text.Replace(" ", ""));
          //                    
          _settings.SetValue((TestFileAnalysisSettings s) => s.CodeFileTemplateName, codeTemplateTextBox.Text);
          _settings.SetValue((TestFileAnalysisSettings s) => s.UnitTestFileTemplateName, unitTestTemplateTextBox.Text);

          _settings.SetValue((TestFileAnalysisSettings s) => s.SupportRenameRefactor, SupportRenameRefactor.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.OrphanedFilesPatterns, OrphanedFilesPatternsTextBox.Text);
          
          DTEHelper.AssignKeyboardShortcutIfMissing(
              true, 
              ResharperHelper.MacroNameSwitchBetweenFiles, SwitchBetweenFilesShortcutTextBox.Text);
          _settings.SetValue((TestFileAnalysisSettings s) => s.ShortcutToSwitchBetweenFiles, SwitchBetweenFilesShortcutTextBox.Text);
          
          return true;
      }

      public bool ValidatePage()
      {
          return true;
      }

      private void BtnAddClick(object sender, RoutedEventArgs e)
      {
          AddItemFromTextbox(attributeTextBox, testingAttributesListBox);
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
        AddItemFromTextbox(contextTextBox, contextPrefixesListBox);
    }
    
    private void BtnRemoveContextClick(object sender, RoutedEventArgs e)
    {
        RemoveAndClearItem(contextTextBox, contextPrefixesListBox);
    }      

    private void ContextPrefixesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        contextTextBox.Text = contextPrefixesListBox.SelectedItem != null ? contextPrefixesListBox.SelectedItem.ToString() : "";
    }

      private void AddItemFromTextbox(TextBox tb, ListBox lb)
      {
          lb.Items.Add(tb.Text);
          tb.Clear();
      }

      private void RemoveAndClearItem(TextBox tb, ListBox lb)
      {
          if (lb.Items.Contains(tb.Text))
          {
              lb.Items.Remove(tb.Text);
              tb.Clear();
          }
      }

      private void ProjectNameRegexTextChanged(object sender, TextChangedEventArgs e)
      {
          var outcomeTexBox = regExProjectOutcome;

          Regex regEx;

          tbProjectSuffixGuidance.Text = string.Format("The configuration below define that all UnitTest Classes " +
                                              "must end in {0} (e.g. {1} ) and the project name of all " +
                                              "test assemblies must match the RegEx '{2}'. Use brackets to extract the associated code project name. "+
                                              "The namespace of the project and associated test project must be the same. "
                                      , testClassSuffixTextBox.Text.Replace(",", " or "), GetSampleClassNames()
                                      , testProjectNameRegExTextBox.Text);
          try
          {
              outcomeTexBox.Text = "";
              regEx = new Regex(testProjectNameRegExTextBox.Text);
          }
          catch (Exception) { return; }

          if (regEx.GetGroupNames().Count() < 2)
          {
              outcomeTexBox.Text = "RegEx must contain at least one regex group ().";
              return;
          }

          if (_solution != null)
          {
              ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestcopOptionsPage", () =>
              {
                  var testProjects = _solution.GetAllCodeProjects().Select(p => p).Where(p => regEx.IsMatch(p.Name ?? "")).ToList();
                  outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the NAME of any loaded projects.";

              }).Invoke();
          }  
      }

      private void MultiTestClassAndNamespaceTextChanged(object sender, TextChangedEventArgs e)
      {          
          var outcomeTexBox = regExOutcome;
          
            Regex regEx;
            
            tbSuffixGuidance.Text=string.Format("The test class and test namespace configuration below define that all UnitTest Classes " +
                                                "must end in {0} (e.g. {1} ) and the namespace of all " +
                                                "test assemblies must match the RegEx '{2}'. Use brackets to extract the associated code project namespace."
                                        ,testClassSuffixTextBox.Text.Replace(","," or "), GetSampleClassNames()
                                        ,testNamespaceRegExTextBox.Text);
            try
            {
                outcomeTexBox.Text = "";                
                regEx = new Regex(testNamespaceRegExTextBox.Text);               
            }
            catch (Exception){return;}

            if (regEx.GetGroupNames().Count() < 2)
            {
                regExOutcome.Text = "RegEx must contain at least one regex group ().";
                return;
            }
                             
            if (_solution != null)
            {              
                ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestcopOptionsPage", () =>
                { 
                    var testProjects = _solution.GetAllCodeProjects().Select(p=>p).Where(p=>regEx.IsMatch(p.GetDefaultNamespace()??"")).ToList();
                    outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the NAMESPACE of any loaded projects.";

                }).Invoke();
            }        
      }

      private string GetSampleClassNames()
      {
          string sampleFileNames = "";
          testClassSuffixTextBox.Text.Split(',').ForEach(s =>
              sampleFileNames=sampleFileNames.AppendIfNotNull(" ,", "ClassA{0}, ClassA.Security{0}".FormatEx(s)));
          return sampleFileNames;
      }

      private void SingleTestClassAndNamespaceTextChanged(object sender, TextChangedEventArgs e)
      {
          var outcomeTexBox = SingleTestRegExOutcome;

          Regex regEx;

          tbSingleTestSuffixGuidanceOne.Text = string.Format("The configuration below define that the namespace of test classes " +
                                              " must match the RegEx '{0}'. Use brackets to extract the associated code project namespace. "+
                                              " The replace string '{1}' will be used to identify the code project namespace and the replace string '{2}' to build the sub namespace "+
                                              " within the code project. \n \n"
                                      , SingleTestNamespaceRegExTextBox.Text, SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text
                                      , SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text);

          tbSingleTestSuffixGuidanceTwo.Text = string.Format("The configuration below defines the sections(s) of the code class namespace that will map to sub-namespace within the test project by " +
                                              "extracting '{0}' from the RegEx '{1}' when it is applied to code files namespace.\n \n"                                                                                    
                                              ,SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text, SingleTestCodeNamespaceRegExTextBox.Text);

          try
          {
              outcomeTexBox.Text = "";
              regEx = new Regex(SingleTestNamespaceRegExTextBox.Text);
          }
          catch (Exception) { return; }

          if (regEx.GetGroupNames().Count() < 2)
          {
              regExOutcome.Text = "RegEx must contain at least one regex group ().";
              return;
          }

          if (_solution != null)
          {
              ResharperHelper.ProtectActionFromReEntry(_lifetime, "TestcopOptionsPage", () =>
              {
                  var testProjects = _solution.GetAllCodeProjects().Select(p => p).Where(p => regEx.IsMatch((p.GetDefaultNamespace() ?? "NS2")+".NS1")).ToList();
                  outcomeTexBox.Text = testProjects.Any() ? "" : "Warning: the regex does not match the namespace of any loaded projects.";

              }).Invoke();
          }
      }

      private void DisplayLoadProjectTip()
      {
          LoadProjectToSelectFileTemplate.Text = "Note: A project/solution needs to be loaded.";
          LoadProjectToSelectFileTemplate.Visibility=Visibility.Visible;
          LoadProjectToSelectFileTemplate.Foreground = new SolidColorBrush(Colors.Red);
      }
      
      private void FileTemplateSelectFromList(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {                    
          Template template=null;
                                  
            if (_solution == null)
            {
                ResharperHelper.AppendLineToOutputWindow("Unable to identify current solution.");
                DisplayLoadProjectTip();
                return; 
            }

            var project = _solution.GetAllCodeProjects().FirstOrDefault();
            if (project == null)
            {
                ResharperHelper.AppendLineToOutputWindow("Unable to identify a code project.");
                DisplayLoadProjectTip();
                return;
            }
                    
            IEnumerable<IFileTemplatesSupport> applicableFileTemplates = _fileTemplatesManager.FileTemplatesSupports.Where(s => s.Accepts(project));                              
            var scope = applicableFileTemplates.SelectMany(s =>s.ScopePoints)
                .Distinct()
                .Where(s=>s.GetDefaultUID()!= new InAnyProject().GetDefaultUID())
                .ToList();
                                       
            using (       
                var templateDialog =
                    new TemplateChooserDialog(
                    #if !R7
                    _lifetime,
                    #endif
                        FileTemplatesManager.Instance.QuickListSupports,
                        scope, project.ToDataContext(),
                        TemplateApplicability.File))

            {
                if (templateDialog.ShowDialog(_application.MainWindow) !=
                    DialogResult.OK)
                {
                    return;
                }
                template = templateDialog.Template;
            }
                          
                                                                                              
          if (template != null)
          {
              ((TextBox) sender).Text = template.Description;
          }          
      }
      
      private void ResetButton_OnClick(object sender, RoutedEventArgs e)
      {
          SingleTestNamespaceRegExTextBox.Text=
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssembly, Logger.Interface);
          SingleTestNamespaceToAssemblyRegExReplaceTextBox.Text =
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssemblyProjectReplace, Logger.Interface);
          SingleTestNamespaceToAssemblySubNameSpaceRegExReplaceTextBox.Text =
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, Logger.Interface);
          SingleTestCodeNamespaceRegExTextBox.Text =
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexCodeToTestAssembly, Logger.Interface);
          SingleTestCodeNamespaceToTestRegExReplaceTextBox.Text =
            SettingsEntryAttribute.ReflectionHelpers.GetDefaultValueFromRuntimeType<TestFileAnalysisSettings, string>(l => l.SingleTestRegexCodeToTestReplace, Logger.Interface);
      }

      private void TestCopStrategyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
          var selectedItem = TestCopStrategyCombo.SelectedItem as ListBoxItem ?? new ListBoxItem(){Tag = TestProjectStrategy.TestProjectPerCodeProject};

          foreach (var item in tabControl.Items.Cast<TabItem>().Where(i=>i.Tag!=null).Where(i=>(int)i.Tag>0))
          {
              item.Visibility = ((int)item.Tag) == ((int)selectedItem.Tag)? Visibility.Visible: Visibility.Collapsed;
          }

          switch ((TestProjectStrategy)selectedItem.Tag)
          {
              case TestProjectStrategy.SingleTestProjectPerSolution:
                  tbStrategyOverview.Text 
                    = "Each Visual Studio solution has only one test project for all code projects within it. "+
                    "You will need to define regular expressions (RegEx) to desribe how the namespace of your code namespace " +
                    "maps to the namespace of the test within the single test project.";
                  break;

              case TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject:
                  tbStrategyOverview.Text
                    = "Each test project maps to a single code project through its project name." +
                    "To use this option the namespace of the code and test assembly must be the same. " +
                    "You will need to define regular expressions (RegEx) to desribe how the project name of each Test project " +
                    "maps to the name of the code project.  For example : DalTests => Dal";
                  break;

              case TestProjectStrategy.TestProjectPerCodeProject:
                  tbStrategyOverview.Text
                    = "Each test project maps to a single code project through its namespace." +
                    "You will need to define regular expressions (RegEx) to desribe how the namespace of each Test namespace " +
                    "maps to the namespace of the code project. For example : mycorp.myapp.tests.dal => mycorp.myapp.dal";
                  break;

              default:
                  tbStrategyOverview.Text = ".";
                  break;
          }
          
      }

    
  }
}
