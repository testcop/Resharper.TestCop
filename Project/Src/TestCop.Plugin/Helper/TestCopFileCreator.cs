// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2017
// --

namespace TestCop.Plugin.Helper
{
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;
    using JetBrains.Application;
    using JetBrains.Application.DataContext;
    using JetBrains.Application.Settings;
    using JetBrains.Application.Threading;
    using JetBrains.DocumentManagers.impl;
    using JetBrains.Lifetimes;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Feature.Services.LiveTemplates.Context;
    using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
    using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
    using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
    using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
    using JetBrains.Util;

    using TestCop.Plugin.Extensions;

    [ShellComponent]
    public class TestCopFileCreator
    {
        private readonly StoredTemplatesProvider _storedTemplatesProvider;
        private readonly ISettingsStore _settingsStore;
        private readonly IProjectFileExtensions _fileExtensions;
        private readonly DataContexts _dataContexts;
        private readonly Lifetime _lifetime;
        private readonly TemplateScopeManager _scopeManager;

        public TestCopFileCreator(StoredTemplatesProvider storedTemplatesProvider
            , ISettingsStore settingsStore
            , IProjectFileExtensions fileExtensions
            , DataContexts dataContexts, Lifetime lifetime,
            TemplateScopeManager scopeManager)
        {
            this._storedTemplatesProvider = storedTemplatesProvider;
            this._settingsStore = settingsStore;
            this._dataContexts = dataContexts;
            this._lifetime = lifetime;
            this._fileExtensions = fileExtensions;
            this._scopeManager = scopeManager;
        }

        public void CreateFileWithinProject([NotNull] TestCopProjectItem projectItem, [NotNull] string targetFile)
        {
            IShellLocks shellLocks = projectItem.Project.Locks;
            string desiredTemplateName = LookupTemplateName(projectItem.Project);
            IContextBoundSettingsStore boundSettingsStore =
                this._settingsStore.BindToContextTransient(ContextRange.ApplicationWide);

            IDataContext context = this._dataContexts.CreateOnActiveControl(this._lifetime);

            IEnumerable<ITemplateScopePoint> applicableFileTemplateScopes =
                this._scopeManager.EnumerateRealScopePoints(
                    new TemplateAcceptanceContext(new ProjectFolderWithLocation(projectItem.Project)));
            applicableFileTemplateScopes =
                applicableFileTemplateScopes.Distinct().Where(s => s is InLanguageSpecificProject).ToList();

            Template classTemplate = this._storedTemplatesProvider
                .EnumerateTemplates(boundSettingsStore, TemplateApplicability.File)
                .Where(x => x.Description == desiredTemplateName
                            && TemplateScopeManager.TemplateIsAvailable(x, applicableFileTemplateScopes))
                .Select(x => x)
                .FirstOrDefault();

            if (classTemplate == null)
            {
                ResharperHelper.AppendLineToOutputWindow(shellLocks,
                    $"File Template for '{desiredTemplateName}' not found with default to 'Class'");
                classTemplate = LoadTemplateFromQuickList(context, "Class");
            }

            IProjectFolder folder =
                (IProjectFolder)projectItem.Project.FindProjectItemByLocation(projectItem.SubNamespaceFolder
                    .ToVirtualFileSystemPath())
                ?? GetOrCreateProjectFolder(projectItem);

            if (folder == null)
            {
                ResharperHelper.AppendLineToOutputWindow(shellLocks,
                    $"Error failed to create/location project folder {projectItem.SubNamespaceFolder}");
                return;
            }

            string extension = this._fileExtensions
                .GetExtensions(projectItem.Project.ProjectProperties.DefaultLanguage.DefaultProjectFileType).First();

            FileTemplatesManager.Instance.CreateFileFromTemplateAsync(targetFile + extension,
                new ProjectFolderWithLocation(folder), classTemplate);
        }

        private static IProjectFolder GetOrCreateProjectFolder(TestCopProjectItem projectItem)
        {
            // TODO: Need to create folders honouring the namespace provider setting defined with projectItem
            return projectItem.Project.GetOrCreateProjectFolder(projectItem.SubNamespaceFolder.ToVirtualFileSystemPath());
        }

        private static Template LoadTemplateFromQuickList(IDataContext context, string templateDescription)
        {
            return FileTemplatesManager.Instance.GetFileTemplatesForActions(context)
                .FirstOrDefault(c => c.Description == templateDescription);
        }

        private static string LookupTemplateName(IProject associatedProject)
        {
            string desiredTemplateName = associatedProject.IsTestProject()
                ? TestCopSettingsManager.Instance.Settings.UnitTestFileTemplateName
                : TestCopSettingsManager.Instance.Settings.CodeFileTemplateName;
            return desiredTemplateName;
        }
    }
}