// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2016
// --

namespace TestCop.Plugin.QuickFixActions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using JetBrains;
    using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.Feature.Services.Bulbs;
    using JetBrains.ReSharper.Feature.Services.Intentions;
    using JetBrains.ReSharper.Feature.Services.QuickFixes;
    using JetBrains.TextControl;
    using JetBrains.Util;

    using TestCop.Plugin.Extensions;
    using TestCop.Plugin.Helper;
    using TestCop.Plugin.Highlighting;

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
            List<IntentionAction> list = new List<IntentionAction>();
                   
            IAnchor anchor = _highlight.FileOnDisk.Count == 1 ? BulbMenuAnchors.FirstClassContextItems
                : (IAnchor)new SubmenuAnchor(BulbMenuAnchors.FirstClassContextItems, SubmenuBehavior.ExecutableDuplicateFirst);            
                                
            if (_highlight.FileOnDisk.Count > 1)
            {                
                list.Add(new RemoveFileBulb(this._highlight.CurrentProject, this._highlight.FileOnDisk.ToArray()).ToQuickFixIntention(anchor, UnnamedThemedIcons.Agent16x16.Id));
            }

            list.AddRange(this._highlight.FileOnDisk.Select(file => new RemoveFileBulb(this._highlight.CurrentProject, new[] { file }).ToQuickFixIntention(anchor, UnnamedThemedIcons.Agent16x16.Id)));

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
            foreach (FileInfo file in _files)
            {
                file.Delete();   
            }            
            DTEHelper.RefreshSolutionExplorerWindow();
        }

        public string Text { get; private set; }
    }
}
