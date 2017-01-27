using System;
using System.Windows.Threading;

namespace AsyncValidation.ProgramDispatcher
{
    /// <summary>
    /// Invokes action in UI thread
    /// </summary>
    public class ProgramDispatcher : IProgramDispatcher
    {
        private static Dispatcher _uiDispatcher;

        public ProgramDispatcher()
        {
            if (_uiDispatcher != null && _uiDispatcher.Thread.IsAlive)
            {
                return;
            }

            _uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        public void InvokeOnUI(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (_uiDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _uiDispatcher.BeginInvoke(action);
            }
        }
    }
}
