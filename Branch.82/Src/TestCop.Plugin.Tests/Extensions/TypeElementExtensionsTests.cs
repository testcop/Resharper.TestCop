// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.ReSharper.Psi;
using NUnit.Framework;
using Rhino.Mocks;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Tests.Extensions
{
    [TestFixture]
    public class TypeElementExtensionsTests
    {
        [Test]
        public void OwnerNamespaceDeclarationWalksHierarchyCorrectlyTest()
        {                        
            var nsRoot = MockRepository.GenerateStub<INamespace>();
            nsRoot.Expect(p => p.IsRootNamespace).Return(true);
            nsRoot.Expect(p => p.ShortName).Return("ROOTNS");

            var ns2 = MockRepository.GenerateStub<INamespace>();
            ns2.Expect(p => p.IsRootNamespace).Return(false);
            ns2.Expect(m => m.GetContainingNamespace()).Return(nsRoot);
            ns2.Expect(p => p.ShortName).Return("NS2");

            var ns1 = MockRepository.GenerateStub<INamespace>();
            ns1.Expect(p => p.IsRootNamespace).Return(false);
            ns1.Expect(m => m.GetContainingNamespace()).Return(ns2);
            ns1.Expect(p => p.ShortName).Return("NS1");

            var elem = MockRepository.GenerateStub<ITypeElement>();
            elem.Expect(m => m.GetContainingNamespace()).Return(ns1);

            string namespacePath = elem.OwnerNamespaceDeclaration();

            Assert.AreEqual("NS2.NS1", namespacePath);
        }        
    }
}