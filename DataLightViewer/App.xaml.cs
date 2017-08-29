using DataLightViewer.Views.Main;
using System.Windows;

namespace DataLightViewer
{
    public partial class App : Application
    {
        public static string CurrentProjectPath;
        public static string ServerConnectionString;
        public static readonly string ServerConnectionStringLiteral = "connectionString";

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
        }
    }
}
