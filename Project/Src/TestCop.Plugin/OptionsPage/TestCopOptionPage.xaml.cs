// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
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
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using Binding = System.Windows.Data.Binding;
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

          BindWithValidationMustBeARegex(testFileAnalysisSettings, testNamespaceRegExTextBox, "TestProjectToCodeProjectNameSpaceRegEx");
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testClassSuffixTextBox, "TestClassSuffix", "^[a-zA-Z]*$");
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testNamespaceRegExReplaceTextBox, "TestProjectToCodeProjectNameSpaceRegExReplace", "^[\\$\\.a-zA-Z1-9]*$");
                                            
          testFileAnalysisSettings.TestingAttributes.ForEach(p => testingAttributesListBox.Items.Add(p));
          testFileAnalysisSettings.BddPrefixes.ForEach(p => contextPrefixesListBox.Items.Add(p));

          SwitchBetweenFilesShortcutTextBox.Text = testFileAnalysisSettings.ShortcutToSwitchBetweenFiles;

          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, codeTemplateTextBox, "CodeFileTemplateName");
          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, unitTestTemplateTextBox, "UnitTestFileTemplateName");

          ShowAllTestsWithUsageCheckBox.IsChecked = testFileAnalysisSettings.FindAnyUsageInTestAssembly;
          CheckTestNamespaces.IsChecked = testFileAnalysisSettings.CheckTestNamespaces;
          OutputPanelOpenOnKeyboardMapping.IsChecked = testFileAnalysisSettings.OutputPanelOpenOnKeyboardMapping;

          TestCopLogoImage.Source =
          (ImageSource) new BitmapToImageSourceConverter().Convert(
              iconManager.Icons[UnnamedThemedIcons.Agent64x64.Id].CurrentGdipBitmap96, null, null, null);         
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

      private void BindWithRegexMatchesValidation(TestFileAnalysisSettings testFileAnalysisSettings,TextBox tb, string property, string regexString)
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
          binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
          tb.DataContext = testFileAnalysisSettings;
          tb.SetBinding(TextBox.TextProperty, binding);
      }

      private void BindWithValidationMustBeARegex(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property)
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

          var attributes = testingAttributesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestingAttributeText, attributes);

          attributes = contextPrefixesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.BddPrefix, attributes);

          _settings.SetValue((TestFileAnalysisSettings s) => s.FindAnyUsageInTestAssembly,ShowAllTestsWithUsageCheckBox.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.CheckTestNamespaces, CheckTestNamespaces.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.OutputPanelOpenOnKeyboardMapping, OutputPanelOpenOnKeyboardMapping.IsChecked);
          
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestClassSuffix,
                             testClassSuffixTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegEx,
                             testNamespaceRegExTextBox.Text.Replace(" ", "")); 
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegExReplace,
                             testNamespaceRegExReplaceTextBox.Text.Replace(" ", ""));

          _settings.SetValue((TestFileAnalysisSettings s) => s.CodeFileTemplateName, codeTemplateTextBox.Text);
          _settings.SetValue((TestFileAnalysisSettings s) => s.UnitTestFileTemplateName, unitTestTemplateTextBox.Text);

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

      private void ClassAndNamespaceTextChanged(object sender, TextChangedEventArgs e)
      {
            Regex regEx;
          
            tbSuffixGuidance.Text=string.Format("The test class and test namespace configuration below define that all UnitTest Classes " +
                                                "must end in '{0}' (e.g. ClassA{0}, ClassA.Security{0}, ClassB{0} ) and the namespace of all " +
                                                "test assemblies must match the RegEx '{1}'. Use brackets to extract the associated project namespace."
                                        ,testClassSuffixTextBox.Text,testNamespaceRegExTextBox.Text);
            try
            {
                regExOutcome.Text = "";
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
                    regExOutcome.Text = testProjects.Any() ? "" : "Warning: the regex does not match the namespace of any loaded projects.";
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
  }
}
