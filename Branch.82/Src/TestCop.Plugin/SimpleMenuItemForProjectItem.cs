// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --
using System;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.UI.Icons;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.RichText;

namespace TestCop.Plugin
{
    public class SimpleMenuItemForProjectItem : SimpleMenuItem
    {
        public IProjectItem AssociatedProjectItem { get; set; }

        public SimpleMenuItemForProjectItem([NotNull] RichText text, [CanBeNull] IconId icon,
            [CanBeNull] Action FOnExecute, IProjectItem associatedProjectItem)
            : base(text, icon, FOnExecute)
        {
            AssociatedProjectItem = associatedProjectItem;
        }
    }
}
