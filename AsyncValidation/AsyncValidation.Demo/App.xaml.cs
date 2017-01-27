using System.Windows;
using AsyncValidation.Tasks;

namespace AsyncValidation.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.DataContext = new DemoViewModel(new TaskFactory(), new ProgramDispatcher.ProgramDispatcher());
        }
    }
}
