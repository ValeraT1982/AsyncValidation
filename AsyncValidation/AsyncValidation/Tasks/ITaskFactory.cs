using System;
using System.Threading.Tasks;

namespace AsyncValidation.Tasks
{
    public interface ITaskFactory
    {
        Task StartNew(Action action);

        Task<T> StartNew<T>(Func<T> func);
    }
}
