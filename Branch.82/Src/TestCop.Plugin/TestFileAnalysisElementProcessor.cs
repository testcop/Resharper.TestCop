// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.VB.Util;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Highlighting;
using IAttribute = JetBrains.ReSharper.Psi.CSharp.Tree.IAttribute;
using IAttributesOwnerDeclaration = JetBrains.ReSharper.Psi.CSharp.Tree.IAttributesOwnerDeclaration;

namespace TestCop.Plugin
{
    public class TestFileAnalysisElementProcessor : IRecursiveElementProcessor
    {
        private readonly IDaemonProcess _process;
        private readonly IContextBoundSettingsStore _settings;        
        private readonly List<HighlightingInfo> _myHighlightings = new List<HighlightingInfo>();

        public List<HighlightingInfo> Highlightings
        {
            get { return _myHighlightings; }
        }

        public TestFileAnalysisElementProcessor(IDaemonProcess process, IContextBoundSettingsStore settings)
        {
            _process = process;
            _settings = settings;            
        }

        private ISolution Solution { get { return _process.Solution; } }
        private IPsiSourceFile CurrentSourceFile { get { return _process.SourceFile; } }

        private IList<string> TestAttributes
        {
            get
            {
                var testFileAnalysisSettings = _settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                var testingAttributes = testFileAnalysisSettings.TestingAttributes();
                if (testingAttributes.Count == 0)
                {
                    testingAttributes.Add("TestFixture");
                    testingAttributes.Add("TestClass");
                    testingAttributes.Add("TestMethod");
                }
                return testingAttributes;
            }
        }
      
        private TestFileAnalysisSettings Settings
        {
            get { return _settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault); }
        }

        private IList<string> BDDPrefixes
        {
            get
            {
                var prefix = Settings.BddPrefixes();               
                return prefix;
            }
        }
              
        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return true;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
        }

        public void ProcessAfterInterior(ITreeNode element)
        {                                     
            var functionDeclaration = element as ICSharpFunctionDeclaration;
            if (functionDeclaration != null)
            {
                ProcessFunctionDeclaration(functionDeclaration);                
            }
            
            var typeDeclaration = element as ICSharpTypeDeclaration;
            if (typeDeclaration != null)
            {
                ProcessTypeDeclaration(typeDeclaration);                           
            }             
        }
        
        private void ProcessTypeDeclaration(ICSharpTypeDeclaration declaration)
        {
            if (declaration.GetContainingNode<ICSharpTypeDeclaration>() != null)
            {
                return;//Dont instpect types already within a type
            }
            
            var testingAttributes = FindTestingAttributes(declaration, TestAttributes);
            if (testingAttributes.Count == 0)
            {
                /* type is missing attributes - lets check the body */
                if (!CheckMethodsForTestingAttributes(declaration, TestAttributes)) return;
            }
            
            //We have a testing attribute so now check some conformance.                       
            CheckElementIsPublicAndCreateWarningIfNot(declaration, testingAttributes);
            
            if (CheckNamingOfTypeEndsWithTestSuffix(declaration))
            {
                if (CheckNamingOfFileAgainstTypeAndCreateWarningIfNot(declaration))
                {
                    CheckClassnameInFileNameActuallyExistsAndCreateWarningIfNot(declaration);
                }
            }

        }

    

        static private bool CheckMethodsForTestingAttributes(ICSharpTypeDeclaration declaration, IList<string> testAttributes )
        {
            var sourceFile = declaration.GetSourceFile();
            if (declaration.DeclaredElement == null) return false;
            foreach (var m in declaration.DeclaredElement.Methods.SelectMany(m => m.GetDeclarationsIn(sourceFile)).OfType<IAttributesOwnerDeclaration>())
            {
                if (Enumerable.Any(FindTestingAttributes(m, testAttributes))) return true;                
            }
            return false;
        }

        static IList<IAttribute> FindTestingAttributes(IAttributesOwnerDeclaration element, IList<string> testAttributes)
        {
            var testingAttributes =
                (from a in element.Attributes where testAttributes.Contains(a.Name.QualifiedName) select a).ToList();
            return testingAttributes;
        }

        private void ProcessFunctionDeclaration(ICSharpFunctionDeclaration declaration)
        {
            // Nothing to calculate
            if (declaration.Body == null) return;

            var testingAttributes = FindTestingAttributes(declaration, TestAttributes);                
            if (testingAttributes.Count==0) return;

            CheckElementIsPublicAndCreateWarningIfNot(declaration, testingAttributes);
            CheckTestMethodHasCodeAndCreateWarningIfNot(declaration);
        }

        public bool ProcessingIsFinished
        {
            get {  return _process.InterruptFlag; }
        }

        private bool CheckNamingOfTypeEndsWithTestSuffix(ICSharpTypeDeclaration declaration)
        {
            if (declaration.IsAbstract) return true;

            var declaredClassName = declaration.DeclaredName;
            if (!declaredClassName.StartsWith(Enumerable.ToArray(BDDPrefixes)))
            {
                if (!declaredClassName.EndsWith(Settings.TestClassSuffixes()))
                {
                    var testingWarning = new TestClassNameSuffixWarning(Settings.TestClassSuffix, declaration);
                    _myHighlightings.Add(new HighlightingInfo(declaration.GetNameDocumentRange(), testingWarning));
                    return false;

                }
            }
            return true;
        }

        private bool CheckNamingOfFileAgainstTypeAndCreateWarningIfNot(ICSharpTypeDeclaration declaration)
        {
            var declaredClassName = declaration.DeclaredName;
            if (declaredClassName.StartsWith(Enumerable.ToArray(BDDPrefixes))) return false;

            var currentFileName = CurrentSourceFile.GetLocation().NameWithoutExtension;
            
            var testClassNameFromFileName = currentFileName.Replace(".", "");
            
            if (testClassNameFromFileName != declaredClassName)
            {                
                var testingWarning = new TestClassNameDoesNotMatchFileNameWarning(declaredClassName, testClassNameFromFileName, declaration);                                
                _myHighlightings.Add(new HighlightingInfo(declaration.GetNameDocumentRange(), testingWarning));
                return false;
            }

            return true;
        }
        
        private void CheckTestMethodHasCodeAndCreateWarningIfNot(ICSharpFunctionDeclaration declaration)
        {
            var statements = declaration.Body.Statements;
            
            if (!statements.Any())
            {
                IHighlighting highlighting = new TestMethodMissingCodeWarning("Test method is empty");
                _myHighlightings.Add(new HighlightingInfo(declaration.GetNameDocumentRange(), highlighting));
            }
            //declaration.Body.Accept(TreeNodeVisitor) -- extend to look at code for at least one IExpressionStatement
        }

        private void CheckElementIsPublicAndCreateWarningIfNot(IAccessRightsOwnerDeclaration declaration, IEnumerable<IAttribute> testingAttributes)
        {
            AccessRights accessRights = declaration.GetAccessRights();
            if (accessRights == AccessRights.PUBLIC) return;
            
            foreach (var attribute in testingAttributes)
            {
                IHighlighting highlighting;                     
                
                if (declaration.DeclaredElement.IsClass())
                {
                    highlighting = new ClassShouldBePublicWarning(attribute.Name.QualifiedName, declaration);
                }
                else
                {
                    highlighting = new MethodShouldBePublicWarning(attribute.Name.QualifiedName, declaration);
                }

                _myHighlightings.Add(new HighlightingInfo(declaration.GetNameDocumentRange(), highlighting));
                return;
            }
        }
  
        private void CheckClassnameInFileNameActuallyExistsAndCreateWarningIfNot(ICSharpTypeDeclaration thisDeclaration)
        {            
            if (thisDeclaration.IsAbstract) return;
            
            var currentFileName = CurrentSourceFile.GetLocation().NameWithoutExtension;

            var appropriateTestClassSuffixes = TestCopSettingsManager.Instance.Settings.GetAppropriateTestClassSuffixes(currentFileName);

            foreach (var testClassSuffix in appropriateTestClassSuffixes)
            {
                var className =
                    currentFileName.Split(new[] {'.'}, 2)[0].RemoveTrailing(testClassSuffix);

                var declaredElements = ResharperHelper.FindClass(Solution, className);

                var currentProject = thisDeclaration.GetProject();
                var currentDeclarationNamespace = thisDeclaration.OwnerNamespaceDeclaration != null
                    ? thisDeclaration.OwnerNamespaceDeclaration.DeclaredName
                    : "";

                var associatedProjects = currentProject.GetAssociatedProjects(currentDeclarationNamespace);
                if (associatedProjects == null || associatedProjects.Count == 0)
                {
                    var highlight =
                        new TestFileNameWarning(
                            "Project for this test assembly was not found - check namespace of projects",
                            thisDeclaration);
                    _myHighlightings.Add(new HighlightingInfo(thisDeclaration.GetNameDocumentRange(), highlight));
                    return;
                }

                var filteredDeclaredElements = new List<IClrDeclaredElement>(declaredElements);
                ResharperHelper.RemoveElementsNotInProjects(filteredDeclaredElements,
                    associatedProjects.Select(p => p.Project).ToList());

                if (filteredDeclaredElements.Count == 0)
                {
                    string message =
                        string.Format(
                            "The file name begins with {0} but no matching class exists in associated project",
                            className);

                    foreach (var declaredElement in declaredElements)
                    {
                        var cls = declaredElement as TypeElement;
                        if (cls != null)
                        {
                            message += string.Format("\nHas it moved to {0}.{1} ?", cls.OwnerNamespaceDeclaration(),
                                cls.GetClrName());
                        }
                    }

                    var highlight = new TestFileNameWarning(message, thisDeclaration);
                    _myHighlightings.Add(new HighlightingInfo(thisDeclaration.GetNameDocumentRange(), highlight));

                    return;
                }

                if (Settings.CheckTestNamespaces)
                {
                    CheckClassNamespaceOfTestMatchesClassUnderTest(thisDeclaration, declaredElements);
                }
            }
        }

        private void CheckClassNamespaceOfTestMatchesClassUnderTest(ICSharpTypeDeclaration thisDeclaration, List<IClrDeclaredElement> declaredElements)
        {            
            var thisProject = thisDeclaration.GetProject();
            if (thisProject == null) return;

            var associatedProject = thisProject.GetAssociatedProjects(thisDeclaration.GetContainingNamespaceDeclaration().DeclaredName).FirstOrDefault();
            if (associatedProject == null) return;
            ResharperHelper.RemoveElementsNotInProjects(declaredElements,new []{associatedProject.Project});   

            var thisProjectsDefaultNamespace = thisProject.GetDefaultNamespace();
            if (string.IsNullOrEmpty(thisProjectsDefaultNamespace)) return;

            var associatedProjectsDefaultNameSpace = associatedProject.Project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(associatedProjectsDefaultNameSpace)) return;

            var nsToBeFoundShouldBe = associatedProject.Project.GetDefaultNamespace()+associatedProject.SubNamespace;
                       
            //Lookup the namespaces of the declaredElements we've found that possibly match this test             
            IList<string> foundNameSpaces = new List<string>();
            foreach (var declaredTestElement in declaredElements)
            {                
                var cls = declaredTestElement as TypeElement;
                if (cls == null) continue;
                var ns = cls.OwnerNamespaceDeclaration();

                if (nsToBeFoundShouldBe == ns)
                {
                    return;//found a match !
                }
                foundNameSpaces.Add(ns);
            }

            foreach (var ns in foundNameSpaces)
            {
                if (ns.StartsWith(associatedProjectsDefaultNameSpace))
                {
                    var targetsubNameSpace = ns.Substring(associatedProjectsDefaultNameSpace.Length).TrimStart(new[] { '.' });
                    string suggestedNameSpace = thisProjectsDefaultNamespace.AppendIfNotNull(".", targetsubNameSpace );

                    var targetFolder = thisProject.Location.Combine(targetsubNameSpace.Replace(".", @"\"));
                                        
                    var highlight = new TestFileNameSpaceWarning(CurrentSourceFile.ToProjectFile(), thisDeclaration, suggestedNameSpace
                        , thisProject, targetFolder);
                                                
                    _myHighlightings.Add(new HighlightingInfo(thisDeclaration.GetNameDocumentRange(), highlight));                   
                }
            }            
        }         
    }
}