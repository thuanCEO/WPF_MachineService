using System.Configuration;
using System.Data;
using System.Windows;

namespace WPF_MachineService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            StartForm startForm = new StartForm();
            startForm.Show();
        }
    }

}
