using System;
using System.Windows.Threading;
using AsyncValidation.ProgramDispatcher;
using AsyncValidation.Tasks;
using Microsoft.Practices.Unity;
using NSubstitute;
using NUnit.Framework;

namespace AsyncValidation.Tests
{
    public class TestsBase
    {
        protected IUnityContainer UnityContainerMock { get; set; }
        protected ITaskFactory TaskFactoryMock { get; set; }
        protected IProgramDispatcher DispatcherMock { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            DispatcherMock = Substitute.For<IProgramDispatcher>();
            DispatcherMock.When(d => d.InvokeOnUI(Arg.Any<Action>())).Do(info => TestsSetUp.Dispatcher.Invoke(info.Arg<Action>(), DispatcherPriority.Normal));

            UnityContainerMock = Substitute.For<IUnityContainer>();
            TaskFactoryMock = Substitute.For<ITaskFactory>();

            UnityContainerMock.Resolve<ITaskFactory>().Returns(TaskFactoryMock);
            UnityContainerMock.Resolve<IProgramDispatcher>().Returns(DispatcherMock);
        }
    }
}