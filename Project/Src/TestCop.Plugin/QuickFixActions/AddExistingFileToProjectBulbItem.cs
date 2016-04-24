// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2016
// --
 
using System;
using System.Collections.Generic;
using System.IO;
using JetBrains;
using JetBrains.Application.Progress;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.I18n.Services;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.QuickFixActions
{
    [QuickFix]
    public class AddExistingFileToProjectBulbItem : IQuickFix
    {
        private readonly FilesNotPartOfProjectWarning _highlight;

        public AddExistingFileToProjectBulbItem(FilesNotPartOfProjectWarning highlight)
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
                list.Add(BulbActionExtensions.ToQuickFixIntention(new AddFileBulb(_highlight.CurrentProject, _highlight.FileOnDisk.ToArray()), anchor, UnnamedThemedIcons.Agent16x16.Id ));
            }

            foreach (var fileInfo in _highlight.FileOnDisk)
            {
                list.Add(BulbActionExtensions.ToQuickFixIntention(new AddFileBulb(_highlight.CurrentProject, new[] { fileInfo }), anchor, UnnamedThemedIcons.Agent16x16.Id));
            }

            return list;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }
    }
    
    class AddFileBulb : QuickFixBase
    {
        private readonly IProject _currentProject;
        private readonly FileInfo[] _files;
        private readonly string _text;

        public AddFileBulb(IProject currentProject, FileInfo[] files)
        {
            _currentProject = currentProject;
            _files = files;

            if (_files.Length > 1)
            {
                _text = "Add orphaned files to project [{0} files]".FormatEx(_files.Length);
            }
            else
            {
                _text = "Add orphaned file to project [{0}]".FormatEx(_files[0].FullName.RemoveLeading(currentProject.Location.FullPath));
            }
        }
      
        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {            
            using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.GetType().Name, progress))
            {
                foreach (var file in _files)
                {
                    _currentProject.AddExistenFile(FileSystemPath.Parse(file.FullName), cookie);
                }
                cookie.Commit(progress);
            }

            return null;
        }

        public override string Text { get { return _text; } }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return true;
        }
    }    
}
