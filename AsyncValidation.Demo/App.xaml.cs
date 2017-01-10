using System.Windows;
using AsyncValidation.ProgramDispatcher;
using AsyncValidation.Tasks;
using Microsoft.Practices.Unity;

namespace AsyncValidation.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var container = new UnityContainer();
            container.RegisterInstance<IUnityContainer>(container);
            container.RegisterInstance<IProgramDispatcher>(new ProgramDispatcher.ProgramDispatcher());
            container.RegisterInstance<ITaskFactory>(new TaskFactory());

            var mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.DataContext = container.Resolve<DemoViewModel>();
        }
    }
}
