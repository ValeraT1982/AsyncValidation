using System;

namespace AsyncValidation.ProgramDispatcher
{
    public interface IProgramDispatcher
    {
        void InvokeOnUI(Action action);
    }
}