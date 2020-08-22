// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --
using JetBrains.Application.Shell;
using JetBrains.Application.Threading;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    class RegisterKeyboardShortcutsForTestCop : IOneTimeInitializationHandler
    {
        private readonly IShellLocks _locks;

        public RegisterKeyboardShortcutsForTestCop(IShellLocks locks)
        {
            _locks = locks;
        }

        public void PerformOneTimeInitialization()
        {
            ResharperHelper.ForceKeyboardBindings(_locks);
        }
    }
}
