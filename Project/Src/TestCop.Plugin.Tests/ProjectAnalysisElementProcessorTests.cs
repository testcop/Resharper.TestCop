using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Lifetimes;
using JetBrains.Util;
using NUnit.Framework;

namespace TestCop.Plugin.Tests
{
    [TestFixture]
    public class ProjectAnalysisElementProcessorTests
    {
        [Test]
        public void IsNotInDirectoryTest()
        {
            var dirs = new Collection<VirtualFileSystemPath>();
            dirs.Add(FileSystemPath.TryParse(@"c:\temp2\temp").ToVirtualFileSystemPath());
            dirs.Add(FileSystemPath.TryParse(@"c:\temp\temp2\bin").ToVirtualFileSystemPath());

            Assert.IsFalse(
                ProjectAnalysisElementProcessor.InDirectory(dirs, FileSystemPath.TryParse(@"c:\temp\temp2\temp3\file.cs").ToVirtualFileSystemPath()));
        }

        [Test]
        public void IsInDirectoryTest()
        {
            var dirs = new Collection<VirtualFileSystemPath>();
            dirs.Add(FileSystemPath.TryParse(@"c:\temp2\temp").ToVirtualFileSystemPath());
            dirs.Add(FileSystemPath.TryParse(@"c:\temp\temp2\bin").ToVirtualFileSystemPath());
            dirs.Add(FileSystemPath.TryParse(@"c:\temp\temp2\temp3").ToVirtualFileSystemPath());

            Assert.IsTrue(
                ProjectAnalysisElementProcessor.InDirectory(dirs, FileSystemPath.TryParse(@"c:\temp\temp2\temp3\file.cs").ToVirtualFileSystemPath()));
        }
    }
}
