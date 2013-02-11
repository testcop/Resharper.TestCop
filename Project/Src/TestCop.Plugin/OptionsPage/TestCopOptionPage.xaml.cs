using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Features.Common.Options;
using JetBrains.UI.CrossFramework;
using JetBrains.UI.Icons;
using JetBrains.UI.Options;
using JetBrains.Util;

namespace TestCop.Plugin.OptionsPage
{
  [OptionsPage(PID, "TestCop ", typeof(UnnamedThemedIcons.Agent16x16), ParentId = ToolsPage.PID)]  
  public partial class TestCopOptionPage : IOptionsPage
  {
      private readonly OptionsSettingsSmartContext _settings;
      private const string PID = "TestCopPageId";
      
      public TestCopOptionPage(Lifetime lifetime, OptionsSettingsSmartContext settings, IThemedIconManager iconManager)
      {
          _settings = settings;
          InitializeComponent();
          
          var testFileAnalysisSettings = settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.DoMeSlowly);

          InitializeComponent();

          BindWithRegexValidation(testFileAnalysisSettings, testNamespaceSuffixTextBox, "TestNameSpaceSuffix","^[.]?[A-Z][a-zA-Z]*$");
          BindWithRegexValidation(testFileAnalysisSettings, testClassSuffixTextBox, "TestClassSuffix", "^[a-zA-Z]*$");
                                            
          testFileAnalysisSettings.TestingAttributes.ForEach(p => testingAttributesListBox.Items.Add(p));
          testFileAnalysisSettings.BddPrefixes.ForEach(p => contextPrefixesListBox.Items.Add(p));

          showAllTestsWithUsageCheckBox.IsChecked = testFileAnalysisSettings.FindAnyUsageInTestAssembly;
          checkTestNamespaces.IsChecked = testFileAnalysisSettings.CheckTestNamespaces;

          TestCopLogoImage.Source =
          (ImageSource) new BitmapToImageSourceConverter().Convert(
              iconManager.Icons[UnnamedThemedIcons.Agent64x64.Id].CurrentGdipBitmap96, null, null, null);

      }

      private void BindWithRegexValidation(TestFileAnalysisSettings testFileAnalysisSettings,TextBox tb, string property, string regexString)
      {
          var namespaceSuffixBinding = new Binding { Path = new PropertyPath(property) };
          var namespaceRule = new RegexValidationRule
          {
              RegexText = regexString,
              ErrorMessage = "Invalid suffix.",
              RegexOptions = RegexOptions.IgnoreCase
          };

          namespaceSuffixBinding.ValidationRules.Add(namespaceRule);
          namespaceSuffixBinding.NotifyOnValidationError = true;
          tb.DataContext = testFileAnalysisSettings;
          tb.SetBinding(TextBox.TextProperty, namespaceSuffixBinding);
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
          if (Validation.GetHasError(testNamespaceSuffixTextBox))return false;
          if (Validation.GetHasError(testClassSuffixTextBox))return false;         

          var attributes = testingAttributesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestingAttributeText, attributes);

          attributes = contextPrefixesListBox.Items.Cast<string>().ToList().Join(",");
          _settings.SetValue((TestFileAnalysisSettings s) => s.BddPrefix, attributes);

          _settings.SetValue((TestFileAnalysisSettings s) => s.FindAnyUsageInTestAssembly,
                             showAllTestsWithUsageCheckBox.IsChecked);
          _settings.SetValue((TestFileAnalysisSettings s) => s.CheckTestNamespaces, checkTestNamespaces.IsChecked);

          _settings.SetValue((TestFileAnalysisSettings s) => s.TestClassSuffix,
                             testClassSuffixTextBox.Text.Replace(" ", ""));
          _settings.SetValue((TestFileAnalysisSettings s) => s.TestNameSpaceSuffix,
                             testNamespaceSuffixTextBox.Text.Replace(" ", ""));

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
          tbSuffixGuidance.Text=string.Format("The test class and test namespace configuration below define that all UnitTest Classes " +
                                              "must end in '{0}' (e.g. ClassA{0}, ClassA.Security{0}, ClassB{0} ) and the namespace of all " +
                                              "test assemblies must end in '{1}' (e.g. MyCompany.MyApplication{1})."
                                        ,testClassSuffixTextBox.Text,testNamespaceSuffixTextBox.Text);                       
      }
  }

  [ValueConversion(typeof(System.Drawing.Bitmap), typeof(ImageSource))]
  public class BitmapToImageSourceConverter : IValueConverter
  {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
          var bmp = value as System.Drawing.Bitmap;
          if (bmp == null)return null;
          return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                      bmp.GetHbitmap(),
                      IntPtr.Zero,
                      Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
          throw new NotSupportedException();
      }
  }
}
