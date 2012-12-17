using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.TextControl;
using JetBrains.Util;
using TestCop.Extensions;
using TestCop.Helper;
using DataConstants = JetBrains.TextControl.DataContext.DataConstants;

namespace TestCop
{
    [ActionHandler("TestCop.JumpToTest")]
    internal class JumpToTestFileAction : IActionHandler
    {        
        public JumpToTestFileAction()
        {        
            ResharperHelper.ForceKeyboardBindings();
        }

        bool IActionHandler.Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            // fetch focused text editor control
            ITextControl textControl = context.GetData(DataConstants.TEXT_CONTROL);
            
            // enable this action if we are in text editor, disable otherwise            
            return textControl != null;
        }
      
        void IActionHandler.Execute(IDataContext context, DelegateExecute nextExecute)
        {            
            ITextControl textControl = context.GetData(DataConstants.TEXT_CONTROL);
            if (textControl == null)
            {
                MessageBox.ShowError("Text control unavailable.");                
                return;
            }
            ISolution solution = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION);
            if (solution == null){return;}

            var currentProject = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.Project);
            var targetProject = ResharperHelper.FindAssociatedProject(currentProject);     
            if(targetProject==null)
            {
                ResharperHelper.AppendLineToOutputWindow("Unable to locate associated assembly");
                return;
            }
            
            var settings = solution.GetPsiServices().SettingsStore
                .BindToContextLive(textControl.Lifetime, ContextRange.ApplicationWide).GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                                                
            IClrTypeName clrTypeClassName = ResharperHelper.GetClassNameAppropriateToLocation(solution, textControl);            
            var classNamesToFind = new List<string>();

            var baseFileName = ResharperHelper.GetBaseFileName(context, solution);
            bool isTestFile = baseFileName.EndsWith(settings.TestClassSuffix);
            var classNameFromFileName = ResharperHelper.UsingFileNameGetClassName(baseFileName).Flip(isTestFile, settings.TestClassSuffix);            

            Func<IClrDeclaredElement, IClrDeclaredElement, bool> declElementMatcher = (element, declaredElement) => element.ToString() == declaredElement.ToString();
            var elementsFoundInTarget = new List<IClrDeclaredElement>();
            var elementsFoundInSolution = new List<IClrDeclaredElement>();
                       
            if(clrTypeClassName!=null)
            {
                string className = clrTypeClassName.ShortName.Flip(isTestFile, settings.TestClassSuffix);                
                elementsFoundInTarget.AddRangeIfMissing(ResharperHelper.FindClass(solution, className, targetProject), declElementMatcher );

                classNamesToFind.Add(className);
                elementsFoundInTarget.AddRangeIfMissing(ResharperHelper.FindClass(solution, className, targetProject), declElementMatcher);
                elementsFoundInSolution.AddRangeIfMissing(ResharperHelper.FindClass(solution, className), declElementMatcher );
            }
            
            classNamesToFind.Add(classNameFromFileName);
            elementsFoundInTarget.AddRangeIfMissing(ResharperHelper.FindClass(solution, classNameFromFileName, targetProject), declElementMatcher);
            elementsFoundInSolution.AddRangeIfMissing(ResharperHelper.FindClass(solution, classNameFromFileName), declElementMatcher); 
            
            if (!isTestFile)
            {                       
                var references = FindReferencesWithinAssociatedAssembly(context, solution, textControl, clrTypeClassName);
                elementsFoundInTarget.AddRangeIfMissing(references, declElementMatcher);                
            }

            JumpToTestMenuHelper.PromptToOpenOrCreateClassFiles(textControl.Lifetime, context, solution
                    ,currentProject, clrTypeClassName,targetProject
                    ,elementsFoundInTarget, elementsFoundInSolution);            
        }
        
       

        private TestFileAnalysisSettings Settings { 
            get
            {
                var lifetimeDefinition = Lifetimes.Define(EternalLifetime.Instance, "TestCop");
                var lifetime = lifetimeDefinition.Lifetime;
                var settingsStore = Shell.Instance.GetComponent<ISettingsStore>();
                var contextBoundSettingsStoreLive = settingsStore.BindToContextLive(lifetime, ContextRange.ApplicationWide);
                var mySettings = contextBoundSettingsStoreLive.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);                
                return mySettings; 
            }

        }

        private IList<IClrDeclaredElement> FindReferencesWithinAssociatedAssembly(IDataContext context
            , ISolution solution, ITextControl textControl, IClrTypeName clrTypeClassName)
        {
            if (clrTypeClassName == null)
            {
                ResharperHelper.AppendLineToOutputWindow("FindReferencesWithinAssociatedAssembly() - clrTypeClassName was null");
                return new List<IClrDeclaredElement>();
            }

            IPsiServices services = solution.GetPsiServices();
            IProject currentProject = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.Project);

            var targetProject = ResharperHelper.FindAssociatedProject(currentProject);
            ISearchDomain searchDomain;

            if (Settings.FindAnyUsageInTestAssembly)
            {                
                searchDomain = SearchDomainFactory.Instance.CreateSearchDomain(
                targetProject.GetAllProjectFiles().Select(p => p.GetPsiModule()));
            }
            else
            {
                //look for similar named files that also have references to this code            
                var items = new List<IProjectFile>();
                var pattern = string.Format("{0}.*{1}", clrTypeClassName.ShortName, Settings.TestClassSuffix);
                var finder = new ProjectFileFinder2(items, new Regex(pattern));
                targetProject.Accept(finder);
                searchDomain = SearchDomainFactory.Instance.CreateSearchDomain(items.Select(p => p.ToSourceFile()));
            }


            var decCache = solution.GetPsiServices().CacheManager.GetDeclarationsCache(DeclarationCacheLibraryScope.REFERENCED, true);
            ITypeElement declaredElement = decCache.GetTypeElementByCLRName(clrTypeClassName);

            var findReferences = services.Finder.FindReferences(
                declaredElement, searchDomain, new ProgressIndicator(textControl.Lifetime));
            /*            
            var containingTypeOrTypeMember = TextControlToPsi.GetContainingTypeOrTypeMember(solution, textControl);
            if(containingTypeOrTypeMember==null)return new List<IClrDeclaredElement>();
                                            
            var findReferences = services.Finder.FindReferences(
                containingTypeOrTypeMember, searchDomain, new ProgressIndicator(textControl.Lifetime));
           */
            List<IClassDeclaration> findReferencesWithinAssociatedAssembly = findReferences.Select(p => ((IClassDeclaration) p.GetTreeNode().GetContainingNode(typeof (IClassDeclaration)))).ToList();
            return findReferencesWithinAssociatedAssembly
                .Select(p => p.DeclaredElement).ToList()                
                .Select(p => p as IClrDeclaredElement).ToList();                                    
        }               
    }
}

