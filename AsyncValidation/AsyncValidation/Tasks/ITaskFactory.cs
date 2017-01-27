using System;
using System.Threading.Tasks;

namespace AsyncValidation.Tasks
{
    /// <summary>
    /// Starts new task
    /// </summary>
    public interface ITaskFactory
    {
        Task StartNew(Action action);
    }
}
