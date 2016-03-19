using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ExpressionSelection;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.DataContext;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;

using JetBrains.ReSharper.Resources.Shell;

namespace TestCop.Plugin.Tests
{
    public static class DataContextOfTestTextControl
    {
        public static IDataContext Create(Lifetime lifetime, ITextControl textControl, ISolution solution)
        {
            var provider = new List<IDataRule>();
            provider.AddRule("Test", PsiDataConstants.DECLARED_ELEMENTS, ctx => TextControlToPsi.GetDeclaredElements(solution, textControl).ToDeclaredElementsDataConstant());
            provider.AddRule("Test", JetBrains.TextControl.DataContext.TextControlDataConstants.TEXT_CONTROL, textControl);
            provider.AddRule("Test", JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION, solution);
            provider.AddRule("Test", PsiDataConstants.REFERENCE, ctx => TextControlToPsi.GetReferencesAtCaret(solution, textControl).FirstOrDefault());
            provider.AddRule("Test", PsiDataConstants.SELECTED_EXPRESSION, ctx => ExpressionSelectionUtil.GetSelectedExpression<ITreeNode>(solution, textControl, false));
            return Shell.Instance.Components.ActionManager().DataContexts.CreateWithDataRules(lifetime, provider);
        }
    } 

}
