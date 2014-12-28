using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ExpressionSelection;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using DataConstants = JetBrains.ReSharper.Psi.Services.DataConstants;
using JetBrains.ReSharper.Resources.Shell;

namespace TestCop.Plugin.Tests
{
    public static class DataContextOfTestTextControl
    {
        public static IDataContext Create(Lifetime lifetime, ITextControl textControl, ISolution solution)
        {
            var provider = new List<IDataRule>();
            provider.AddRule("Test", DataConstants.DECLARED_ELEMENTS, ctx => TextControlToPsi.GetDeclaredElements(solution, textControl).ToDeclaredElementsDataConstant());
            provider.AddRule("Test", JetBrains.TextControl.DataContext.DataConstants.TEXT_CONTROL, textControl);
            provider.AddRule("Test", JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION, solution);
            provider.AddRule("Test", DataConstants.REFERENCE, ctx => TextControlToPsi.GetReferencesAtCaret(solution, textControl).FirstOrDefault());
            provider.AddRule("Test", DataConstants.SELECTED_EXPRESSION, ctx => ExpressionSelectionUtil.GetSelectedExpression<ITreeNode>(solution, textControl, false));
            return Shell.Instance.Components.ActionManager().DataContexts.CreateWithDataRules(lifetime, provider);
        }
    } 

}
