using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Scope;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.LiveTemplates.Templates;
using JetBrains.Util;
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

        public TestCopFileCreater(StoredTemplatesProvider storedTemplatesProvider
            , ISettingsStore settingsStore
            , IProjectFileExtensions fileExtensions
            , DataContexts dataContexts, Lifetime lifetime, FileTemplatesManager fileTemplatesManager)
        {
            _storedTemplatesProvider = storedTemplatesProvider;
            _settingsStore = settingsStore;
            _dataContexts = dataContexts;
            _lifetime = lifetime;
            _fileTemplatesManager = fileTemplatesManager;
            _fileExtensions = fileExtensions;
        }

        public  void CreateFileWithinProject(IProject associatedProject,FileSystemPath fileSystemPath, string targetFile)
        {                        
            var desiredTemplateName = LookupTemplateName(associatedProject);
            var boundSettingsStore = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide);

            var context = _dataContexts.CreateOnActiveControl(_lifetime);
            
            var applicableFileTemplates = _fileTemplatesManager.FileTemplatesSupports.Where(s => s.Accepts(associatedProject));
            var applicableFileTemplateScopes = applicableFileTemplates.SelectMany(s => s.ScopePoints).Distinct().ToList();
            
            var classTemplate = _storedTemplatesProvider.EnumerateTemplates(boundSettingsStore, TemplateApplicability.File)
               .Where(x => x.Description == desiredTemplateName
                   && TemplateScopeManager.TemplateIsAvailable(x, applicableFileTemplateScopes))
               .Select(x => x)
               .FirstOrDefault();

            if (classTemplate == null)
            {
                ResharperHelper.AppendLineToOutputWindow(string.Format("File Template for '{0}' not found will default to 'Class'", desiredTemplateName));
                classTemplate = LoadTemplateFromQuickList(context, "Class");
            }
            IProjectFolder folder = (IProjectFolder)associatedProject.FindProjectItemByLocation(fileSystemPath)
                                    ?? AddNewItemUtil.GetOrCreateProjectFolder(associatedProject, fileSystemPath);

            if (folder == null)
            {
                ResharperHelper.AppendLineToOutputWindow("Error failed to create/location project folder" + fileSystemPath);
                return;
            }

            string extension = Enumerable.First(_fileExtensions.GetExtensions(associatedProject.ProjectProperties.DefaultLanguage.DefaultProjectFileType));
            FileTemplatesManager.Instance.CreateFileFromTemplate(targetFile + extension, folder, classTemplate);
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
