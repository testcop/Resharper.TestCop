// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2017
// --

using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers.impl;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Context;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper
{
    [ShellComponent]
    public class TestCopFileCreater
    {
        private readonly StoredTemplatesProvider _storedTemplatesProvider;
        private readonly ISettingsStore _settingsStore;
        private readonly IProjectFileExtensions _fileExtensions;
        private readonly DataContexts _dataContexts;
        private readonly Lifetime _lifetime;
        private readonly FileTemplatesManager _fileTemplatesManager;
        private readonly TemplateScopeManager _scopeManager;

        public TestCopFileCreater(StoredTemplatesProvider storedTemplatesProvider
            , ISettingsStore settingsStore
            , IProjectFileExtensions fileExtensions
            , DataContexts dataContexts, Lifetime lifetime, FileTemplatesManager fileTemplatesManager, TemplateScopeManager scopeManager)
        {
            _storedTemplatesProvider = storedTemplatesProvider;
            _settingsStore = settingsStore;
            _dataContexts = dataContexts;
            _lifetime = lifetime;
            _fileTemplatesManager = fileTemplatesManager;
            _fileExtensions = fileExtensions;
            _scopeManager = scopeManager;
        }

        public  void CreateFileWithinProject([NotNull] TestCopProjectItem projectItem, [NotNull] string targetFile)
        {
            var shellLocks = projectItem.Project.Locks;
            var desiredTemplateName = LookupTemplateName(projectItem.Project);
            var boundSettingsStore = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide);

            var context = _dataContexts.CreateOnActiveControl(_lifetime);
                        
            var applicableFileTemplateScopes = _scopeManager.EnumerateRealScopePoints(new TemplateAcceptanceContext(new ProjectFolderWithLocation(projectItem.Project)));
            applicableFileTemplateScopes = applicableFileTemplateScopes.Distinct().Where(s => s is InLanguageSpecificProject).ToList();
            
            var classTemplate = _storedTemplatesProvider.EnumerateTemplates(boundSettingsStore, TemplateApplicability.File)
               .Where(x => x.Description == desiredTemplateName
                   && TemplateScopeManager.TemplateIsAvailable(x, applicableFileTemplateScopes))
               .Select(x => x)
               .FirstOrDefault();

            if (classTemplate == null)
            {                
                ResharperHelper.AppendLineToOutputWindow(shellLocks, string.Format("File Template for '{0}' not found with default to 'Class'", desiredTemplateName));
                classTemplate = LoadTemplateFromQuickList(context, "Class");
            }
            IProjectFolder folder = (IProjectFolder)projectItem.Project.FindProjectItemByLocation(projectItem.SubNamespaceFolder)
                                    ?? GetOrCreateProjectFolder(projectItem);
            
            if (folder == null)
            {
                ResharperHelper.AppendLineToOutputWindow(shellLocks, "Error failed to create/location project folder" + projectItem.SubNamespaceFolder);
                return;
            }

            string extension = Enumerable.First(_fileExtensions.GetExtensions(projectItem.Project.ProjectProperties.DefaultLanguage.DefaultProjectFileType));

            FileTemplatesManager.Instance.CreateFileFromTemplateAsync(targetFile + extension, new ProjectFolderWithLocation(folder), classTemplate);
        }

        private static IProjectFolder GetOrCreateProjectFolder(TestCopProjectItem projectItem)
        {
            ///TODO: Need to create folders honouring the namespace provider setting defined with projectItem
            return projectItem.Project.GetOrCreateProjectFolder(projectItem.SubNamespaceFolder);
        }

        private static Template LoadTemplateFromQuickList(IDataContext context, string templateDescription)
        {
            return FileTemplatesManager.Instance.GetFileTemplatesForActions(context).FirstOrDefault(c => c.Description == templateDescription);
        }

        private static string LookupTemplateName(IProject associatedProject)
        {
            var desiredTemplateName = associatedProject.IsTestProject()
                ? TestCopSettingsManager.Instance.Settings.UnitTestFileTemplateName
                : TestCopSettingsManager.Instance.Settings.CodeFileTemplateName;
            return desiredTemplateName;
        }     
    }
}
