using System;
using System.Threading.Tasks;

namespace AsyncValidation.Tasks
{
    public class TaskFactory: ITaskFactory
    {
        public Task StartNew(Action action)
        {
            return Task.Factory.StartNew(action);
        }

        public Task<T> StartNew<T>(Func<T> func)
        {
            return Task.Factory.StartNew(func);
        }
    }
}
