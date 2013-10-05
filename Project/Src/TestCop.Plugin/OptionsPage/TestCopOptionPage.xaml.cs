// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
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
using EnvDTE;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.Interop.WinApi;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Context;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Features.Common.Options;
using JetBrains.ReSharper.LiveTemplates.CSharp.Context;
using JetBrains.ReSharper.LiveTemplates.CSharp.Scope;
using JetBrains.ReSharper.LiveTemplates.Templates;
using JetBrains.ReSharper.LiveTemplates.UI;
using JetBrains.ReSharper.LiveTemplates.VB.Scope;
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
      
      public TestCopOptionPage(Lifetime lifetime, OptionsSettingsSmartContext settings
          , IThemedIconManager iconManager, UIApplication application
        , StoredTemplatesProvider storedTemplatesProvider, ISolution solution = null)
      {
          _lifetime = lifetime;
          _settings = settings;
          _application = application;
          _solution = solution;
          _storedTemplatesProvider = storedTemplatesProvider;

          InitializeComponent();
         
          var testFileAnalysisSettings = settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.DoMeSlowly);

          InitializeComponent();

          BindWithValidationMustBeARegex(testFileAnalysisSettings, testNamespaceRegExTextBox, "TestProjectToCodeProjectNameSpaceRegEx");
          BindWithRegexMatchesValidation(testFileAnalysisSettings, testClassSuffixTextBox, "TestClassSuffix", "^[a-zA-Z]*$");
                                            
          testFileAnalysisSettings.TestingAttributes.ForEach(p => testingAttributesListBox.Items.Add(p));
          testFileAnalysisSettings.BddPrefixes.ForEach(p => contextPrefixesListBox.Items.Add(p));

          SwitchBetweenFilesShortcutTextBox.Text = testFileAnalysisSettings.ShortcutToSwitchBetweenFiles;

          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, codeTemplateTextBox, "CodeFileTemplateName");
          BindWithValidationMustBeAFileTemplate(testFileAnalysisSettings, unitTestTemplateTextBox, "UnitTestFileTemplateName");

          showAllTestsWithUsageCheckBox.IsChecked = testFileAnalysisSettings.FindAnyUsageInTestAssembly;
          checkTestNamespaces.IsChecked = testFileAnalysisSettings.CheckTestNamespaces;

          TestCopLogoImage.Source =
          (ImageSource) new BitmapToImageSourceConverter().Convert(
              iconManager.Icons[UnnamedThemedIcons.Agent64x64.Id].CurrentGdipBitmap96, null, null, null);         
      }

      private void BindWithValidationMustBeAFileTemplate(TestFileAnalysisSettings testFileAnalysisSettings, TextBox tb, string property)
      {
          var binding = new Binding { Path = new PropertyPath(property) };
          var rule = new IsAFileTemplateValidationRule(_lifetime,_storedTemplatesProvider ,_settings);

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

          _settings.SetValue((TestFileAnalysisSettings s) => s.FindAnyUsageInTestAssembly,showAllTestsWithUsageCheckBox.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.CheckTestNamespaces, checkTestNamespaces.IsChecked);

          _settings.SetValue((TestFileAnalysisSettings s) => s.TestClassSuffix,
                             testClassSuffixTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestProjectToCodeProjectNameSpaceRegEx,
                             testNamespaceRegExTextBox.Text.Replace(" ", ""));

          _settings.SetValue((TestFileAnalysisSettings s) => s.CodeFileTemplateName, codeTemplateTextBox.Text);
          _settings.SetValue((TestFileAnalysisSettings s) => s.UnitTestFileTemplateName, unitTestTemplateTextBox.Text);

          DTEHelper.AssignKeyboardShortcutIfMissing(ResharperHelper.MacroNameSwitchBetweenFiles, SwitchBetweenFilesShortcutTextBox.Text);
          _settings.SetValue((TestFileAnalysisSettings s) => s.ShortcutToSwitchBetweenFiles, SwitchBetweenFilesShortcutTextBox.Text);
          
          return true;
      }

      public bool ValidatePage()
      {
          return true;
      }

      private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
      {
          AddItemFromTextbox(attributeTextBox, testingAttributesListBox);
      }

      private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
      {
          RemoveAndClearItem(attributeTextBox, testingAttributesListBox);
      }

      private void testingAttributesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
          attributeTextBox.Text = testingAttributesListBox.SelectedItem != null
                                      ? testingAttributesListBox.SelectedItem.ToString()
                                      : "";
      }

      private void btnAddContext_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        AddItemFromTextbox(contextTextBox, contextPrefixesListBox);
    }
    
    private void btnRemoveContext_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        RemoveAndClearItem(contextTextBox, contextPrefixesListBox);
    }      

    private void contextPrefixesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

      private void classAndNamespace_TextChanged(object sender, TextChangedEventArgs e)
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

     
      private void FileTemplateSelectFromList(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {          
          var dataContexts = Shell.Instance.GetComponent<DataContexts>();
                              
          Template template=null;
          
          Lifetimes.Using((System.Action<Lifetime>)(l =>
                                                    {
                                                        var context = dataContexts.CreateOnActiveControl(l);

                                                        IProjectFolder projectFolder = FileTemplateUtil.GetProjectFolderFromContext(context);
                                                        if (projectFolder == null) return;
                                                        var project = projectFolder.GetProject();
                                                        if (project == null) return;

                                                        var scope = new List<ITemplateScopePoint>();
                                                                                                                                                                        
                                                        if (project.ProjectProperties.DefaultLanguage == ProjectLanguage.VBASIC)
                                                        {
                                                            scope.Add(new InVBProjectFile());   
                                                        }
                                                        else
                                                        {
                                                            scope.Add(new InCSharpProjectFile());   
                                                        }
                                                        
                                                        using (                                                                                                                
                                                            var templateDialog =
                                                                new TemplateChooserDialog(l,
                                                                    FileTemplatesManager.Instance.QuickListSupports,
                                                                    scope, projectFolder.ToDataContext(),
                                                                    TemplateApplicability.File))
                                                        {
                                                            if (templateDialog.ShowDialog(_application.MainWindow) !=
                                                                DialogResult.OK)
                                                            {
                                                                return;
                                                            }
                                                            template = templateDialog.Template;
                                                        }
                                                    }));
                                                                                              
          if (template != null)
          {
              ((TextBox) sender).Text = template.Description;
          }
          
      }     
  }
}
