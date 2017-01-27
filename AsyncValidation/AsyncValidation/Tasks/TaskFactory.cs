using System;
using System.Threading.Tasks;

namespace AsyncValidation.Tasks
{
    /// <summary>
    /// Starts new task
    /// </summary>
    public class TaskFactory: ITaskFactory
    {
        public Task StartNew(Action action)
        {
            return Task.Factory.StartNew(action);
        }
    }
}
