using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Application.Progress;
using JetBrains.ReSharper.Feature.Services.Cpp.Generate;
using JetBrains.ReSharper.Feature.Services.Cpp.Generate.Builders;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp;
using JetBrains.ReSharper.Psi.Cpp.Lang;
using JetBrains.ReSharper.Psi.Cpp.Symbols;
using JetBrains.ReSharper.Psi.Cpp.Tree;
using JetBrains.ReSharper.Psi.Cpp.Types;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using TestCop.Plugin;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    //CppEqualityOperatorsBuilder
    
    [GeneratorBuilder("Overrides", typeof(CSharpLanguage))]
    [GeneratorBuilder("Implementations", typeof(CSharpLanguage))]
    class GenerateTestStubs : GeneratorBuilderBase<CSharpGeneratorContext>
  {
    public override double Priority
    {
      get
      {
        return 0.0;
      }
    }

    protected override void Process(CSharpGeneratorContext context, IProgressIndicator progress)
    {     
    }
   
    protected override bool HasProcessableElements(CSharpGeneratorContext context, IEnumerable<IGeneratorElement> elements)
    {
        foreach (IGeneratorElement generatorElement in elements)
        {
            GeneratorDeclaredElement<IOverridableMember> generatorDeclaredElement = generatorElement as GeneratorDeclaredElement<IOverridableMember>;
            if (generatorDeclaredElement != null && generatorDeclaredElement.DeclaredElement.GetContainingType() is IClass && ModifiersOwnerExtension.CanBeOverriden((IModifiersOwner)generatorDeclaredElement.DeclaredElement))
                return true;
        }
        return false;
    }

    protected override bool IsAvaliable(CSharpGeneratorContext context)
    {
        ResharperHelper.AppendLineToOutputWindow(context.PsiModule.Name);
        if (context.PsiModule.Name.EndsWith(TestCopSettingsManager.Instance.Settings.TestClassSuffix))
        {
            return true;
        }
        
        return false;              
    }
  }
}
        
 /*       
        : GeneratorBuilderBase<CSharpGeneratorContext>
    {
        public override double Priority
        {
            get
            {
                return 0.0;
            }
        }

        protected override bool HasProcessableElements(CSharpGeneratorContext context, IEnumerable<IGeneratorElement> elements)
        {
            foreach (IGeneratorElement generatorElement in elements)
            {
                GeneratorDeclaredElement<IOverridableMember> generatorDeclaredElement = generatorElement as GeneratorDeclaredElement<IOverridableMember>;
                if (generatorDeclaredElement != null && generatorDeclaredElement.DeclaredElement.GetContainingType() is IClass && ModifiersOwnerExtension.CanBeOverriden((IModifiersOwner)generatorDeclaredElement.DeclaredElement))
                    return true;
            }
            return false;
        }

        protected override void Process(CSharpGeneratorContext context)
        {
            ITypeElement declaredElement = context.ClassDeclaration.DeclaredElement;
            if (declaredElement == null || !(declaredElement is IClass) && !(declaredElement is IStruct))
                return;
            List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>> membersToOverride = GenerateTestStubs.GetMembersToOverride(context);
            GenerateTestStubs.FilterConflictingMembers(membersToOverride);
            foreach (Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember> pair in membersToOverride)
                GenerateTestStubs.GenerateInheritor(context, pair.First, pair.Second);
        }

        private static List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>> GetMembersToOverride(CSharpGeneratorContext context)
        {
            List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>> list = new List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>>();
            foreach (IGeneratorElement generatorElement in (IEnumerable<IGeneratorElement>)context.InputElements)
            {
                GeneratorDeclaredElement<IOverridableMember> first = generatorElement as GeneratorDeclaredElement<IOverridableMember>;
                if (first != null)
                {
                    IOverridableMember declaredElement = first.DeclaredElement;
                    if (!(declaredElement.GetContainingType() is IInterface))
                        list.Add(Pair.Of<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>(first, declaredElement));
                }
            }
            return list;
        }

        private static void FilterConflictingMembers(List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>> abstractMembers)
        {
            LocalList<InvocableSignature> localList = new LocalList<InvocableSignature>();
            List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>> list = new List<Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember>>();
            foreach (Pair<GeneratorDeclaredElement<IOverridableMember>, IOverridableMember> pair in abstractMembers)
            {
                IParametersOwner parametersOwner = pair.Second as IParametersOwner;
                if (parametersOwner != null)
                {
                    localList.Add(parametersOwner.GetSignature(pair.First.Substitution));
                    list.Add(pair);
                }
            }
            for (int index1 = 0; index1 < localList.Count; ++index1)
            {
                for (int index2 = index1 + 1; index2 < localList.Count; ++index2)
                {
                    if (SignatureComparers.Strict.Compare(localList[index1], localList[index2]))
                        abstractMembers.Remove(list[index2]);
                }
            }
        }

        private static void GenerateInheritor(CSharpGeneratorContext context, GeneratorDeclaredElement<IOverridableMember> inputElement, IOverridableMember overridableMember)
        {
            IClassMemberDeclaration implementation = GenerateTestStubs.CreateImplementation(context, inputElement, overridableMember);
            context.PutMemberDeclaration<IClassMemberDeclaration>(implementation, (IGeneratorElement)inputElement, (Func<IClassMemberDeclaration, IGeneratorElement>)(newDeclaration => (IGeneratorElement)new GeneratorOverrideDeclarationElement((IDeclaration)newDeclaration, inputElement)), (string)null);
        }

        private static IClassMemberDeclaration CreateImplementation(CSharpGeneratorContext context, GeneratorDeclaredElement<IOverridableMember> inputElement, IOverridableMember baseMember)
        {
            ISubstitution newSubstitution;
            IClassMemberDeclaration memberDeclaration = (IClassMemberDeclaration)CSharpGenerateUtil.CreateMemberDeclaration(context.ClassDeclaration, inputElement.Substitution, baseMember, !baseMember.IsAbstract, out newSubstitution);
            memberDeclaration.SetOverride(true);
            AccessRights accessRights = baseMember.GetAccessRights();
            if (accessRights == AccessRights.PROTECTED_OR_INTERNAL)
            {
                if (PsiModuleExtensions.AreInternalsVisibleTo(baseMember.Module, memberDeclaration.GetPsiModule()))
                    memberDeclaration.SetAccessRights(AccessRights.PROTECTED_OR_INTERNAL);
                else
                    memberDeclaration.SetAccessRights(AccessRights.PROTECTED);
            }
            else
                memberDeclaration.SetAccessRights(accessRights);
            IAccessorOwnerDeclaration accessorOwnerDeclaration = memberDeclaration as IAccessorOwnerDeclaration;
            
            if (accessorOwnerDeclaration != null)
                GenerateTestStubs.SetAccessorsRights(accessorOwnerDeclaration, accessRights, baseMember);
             
            return memberDeclaration;
        }
        
    }
}
*/