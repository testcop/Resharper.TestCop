using JetBrains;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestExplorer.Common;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.New2;
using JetBrains.UI.ActionsRevised;

namespace TestCop.Plugin
{
    //UnitTestSessionRunAllActionBase
    public abstract class TestCopUnitTestRunContextActionBase : IExecutableAction, IAction
    {       

        public virtual bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            IUnitTestSession session = this.GetSession(context);
            IHostProviderDescriptor providerDescriptor = this.GetHostProviderDescriptor();
            HostProviderAvailability availability = providerDescriptor.Provider.GetAvailability();
            presentation.Text = StringEx.FormatEx(providerDescriptor.Format, (object)"Current Session");
            presentation.Visible = availability != HostProviderAvailability.Nonexistent;
            if (session != null && session.IsIdle.Value && session.Elements.Count > 0)
                return availability == HostProviderAvailability.Available;
            return false;
        }

        public virtual void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            IUnitTestSession session = this.GetSession(context);
            if (session == null || !session.IsIdle.Value)
                return;
            IHostProvider provider = this.GetHostProviderDescriptor().Provider;
            UnitTestElements elementsToRun = this.GetElementsToRun(session, context);
            if (elementsToRun == null)
                return;
            UnitTestingFacadeFluentExtensions.Run(DataConstantsExtensions.GetComponent<IUnitTestingFacade>(context), elementsToRun).Using(provider, new BuildPolicy?(), new PlatformType?(), new PlatformVersion?()).In.CurrentOrNewSession((string)null);
        }

        [NotNull]
        protected abstract IHostProviderDescriptor GetHostProviderDescriptor();

        [CanBeNull]
        protected virtual IUnitTestSession GetSession([NotNull] IDataContext context)
        {
            return context.GetData<IUnitTestSession>(UnitTestDataConstants.UNIT_TEST_SESSION);
        }

        [CanBeNull]
        protected virtual UnitTestElements GetElementsToRun([NotNull] IUnitTestSession session, [NotNull] IDataContext context)
        {
            return context.GetData<UnitTestElements>(UnitTestDataConstants.VISIBLE_UNIT_TEST_ELEMENTS);
        }
    }
}
