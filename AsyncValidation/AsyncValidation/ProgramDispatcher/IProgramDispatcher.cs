using System;

namespace AsyncValidation.ProgramDispatcher
{
    /// <summary>
    /// Invokes action in UI thread
    /// </summary>
    public interface IProgramDispatcher
    {
        void InvokeOnUI(Action action);
    }
}