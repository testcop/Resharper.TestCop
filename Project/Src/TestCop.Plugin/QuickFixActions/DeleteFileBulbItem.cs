// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2016
// --

using System.Collections.Generic;
using System.IO;
using JetBrains;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.QuickFixActions
{
    [QuickFix]
    public class DeleteFileBulbItem : IQuickFix
    {
        private readonly FilesNotPartOfProjectWarning _highlight;

        public DeleteFileBulbItem(FilesNotPartOfProjectWarning highlight)
        {
            _highlight = highlight;
        }
             
        public IEnumerable<IntentionAction> CreateBulbItems()
        {            
            var list = new List<IntentionAction>();
                   
            var anchor = _highlight.FileOnDisk.Count == 1 ? BulbMenuAnchors.FirstClassContextItems
                : (IAnchor)new SubmenuAnchor(BulbMenuAnchors.FirstClassContextItems, SubmenuBehavior.ExecutableDuplicateFirst);            
                                
            if (_highlight.FileOnDisk.Count > 1)
            {                
                list.Add(BulbActionExtensions.ToQuickFixIntention(new RemoveFileBulb(_highlight.CurrentProject, _highlight.FileOnDisk.ToArray()), anchor, UnnamedThemedIcons.Agent16x16.Id));
            }

            _highlight.FileOnDisk.ForEach(f=>
                list.Add(BulbActionExtensions.ToQuickFixIntention(new RemoveFileBulb(_highlight.CurrentProject, new[] { f }), anchor, UnnamedThemedIcons.Agent16x16.Id))
            );
       
            return list;    
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }
    }

    class RemoveFileBulb : IBulbAction
    {
        private readonly FileInfo[] _files;

        public RemoveFileBulb(IProject currentProject, FileInfo[] files)
        {
            _files = files;

            if (_files.Length > 1)
            {
                Text = "Delete orphaned files from project [{0} files]".FormatEx(_files.Length);
            }
            else
            {
                Text = "Delete orphaned file [{0}]".FormatEx(_files[0].FullName.RemoveLeading(currentProject.Location.FullPath));
            }
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            foreach (var file in _files)
            {
                file.Delete();   
            }            
            DTEHelper.RefreshSolutionExplorerWindow();
        }

        public string Text { get; private set; }
    }
}
