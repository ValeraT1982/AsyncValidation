using System.Threading;
using System.Windows.Threading;
using NUnit.Framework;

namespace AsyncValidation.Tests
{
    [SetUpFixture]
    public class TestsSetUp
    {
        public static Dispatcher Dispatcher { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
    }
}
