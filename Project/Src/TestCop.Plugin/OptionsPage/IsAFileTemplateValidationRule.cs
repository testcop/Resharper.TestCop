// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.LiveTemplates.CSharp.Scope;
using JetBrains.ReSharper.LiveTemplates.VB.Scope;
using TestCop.Plugin.Helper;


namespace TestCop.Plugin.OptionsPage
{    
    public class IsAFileTemplateValidationRule : ValidationRule
    {
        private readonly Lifetime _lifetime;
        readonly StoredTemplatesProvider _storedTemplatesProvider;
        private readonly IContextBoundSettingsStore _settingsStore;
        

        public IsAFileTemplateValidationRule(Lifetime lifetime
            , StoredTemplatesProvider storedTemplatesProvider
            , IContextBoundSettingsStore settingsStore)
        {
            _lifetime = lifetime;
            _storedTemplatesProvider = storedTemplatesProvider;
            _settingsStore = settingsStore;            
        }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;
            var action = ResharperHelper.ProtectActionFromReEntry(_lifetime, "IsAFileTemplateValidationRule", () => result = CheckTemplateExists(value));
            action.Invoke();
            return result;
        }

        private ValidationResult CheckTemplateExists(object value)
        {
            ValidationResult result = ValidationResult.ValidResult;
            string text = value as string ?? String.Empty;
                                            
            var template = GetTemplatesForName(text);

            if (!template.Any())
            {
                result = new ValidationResult(false, "Unknown template name");
            }

            return result;
        }

        private IEnumerable<Template> GetTemplatesForName(string templateDescription)
        {            
            IList<ITemplateScopePoint> applicableFileTeamplateScope = new List<ITemplateScopePoint>();
            applicableFileTeamplateScope.Add(new InAnyProject());
            applicableFileTeamplateScope.Add(new InCSharpProjectFile());
            applicableFileTeamplateScope.Add(new InVBProjectFile());

            var template = _storedTemplatesProvider.EnumerateTemplates(_settingsStore, TemplateApplicability.File)
                .Where(x => x.Description == templateDescription
                            && TemplateScopeManager.TemplateIsAvailable(x, applicableFileTeamplateScope))
                .Select(x=>x);
            return template;
        }
    }
}