// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin
{
    public class ProjectAnalysisElementProcessor : IRecursiveElementProcessor
    {
        private readonly IDaemonProcess _process;
        private readonly IContextBoundSettingsStore _settings;        
        private readonly List<HighlightingInfo> _myHighlightings = new List<HighlightingInfo>();

        public List<HighlightingInfo> Highlightings
        {
            get { return _myHighlightings; }
        }

        public ProjectAnalysisElementProcessor(IDaemonProcess process, IContextBoundSettingsStore settings)
        {
            _process = process;
            _settings = settings;            
        }
        
        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return false;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            if (String.Compare(_process.SourceFile.Name, "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)==0) return;

            var usingBlock = element as IUsingList;
            if (usingBlock != null)
            {
                CheckForProjectFilesNotInProjectAndWarn(element);                
            }                    
        }
   
        public bool ProcessingIsFinished
        {
            get {  return _process.InterruptFlag; }
        }

        private void CheckForProjectFilesNotInProjectAndWarn(ITreeNode declaration)
        {
               //TODO:
        }       
    }
}