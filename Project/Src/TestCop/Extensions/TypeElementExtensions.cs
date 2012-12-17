using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;

namespace TestCop.Extensions
{
    public static class TypeElementExtensions
    {
        public static string OwnerNamespaceDeclaration(this TypeElement element)
        {
            INamespace containingNamespace = element.GetContainingNamespace();
            var list = new List<string>();
            for (; !containingNamespace.IsRootNamespace; containingNamespace = containingNamespace.GetContainingNamespace())
                list.Add(containingNamespace.ShortName);
            list.Reverse();
            return string.Join(".", list.ToArray());
        }     
    
    }
}
