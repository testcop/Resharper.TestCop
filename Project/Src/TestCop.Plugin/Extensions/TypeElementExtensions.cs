// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2014
// --

using System.Collections.Generic;
using JetBrains.ReSharper.Psi;

namespace TestCop.Plugin.Extensions
{
    public static class TypeElementExtensions
    {
        public static string OwnerNamespaceDeclaration(this ITypeElement element)
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
