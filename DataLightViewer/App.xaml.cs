using DataLightViewer.Views.Main;
using System.Windows;
using DataLightViewer.Models;

namespace DataLightViewer
{
    public partial class App : Application
    {
        #region Data

        /// <summary>
        /// Session can be initialized by creating or opening project
        /// </summary>
        public static bool IsSessionInitialized;
        public static AppConnectionMode ConnectionMode;
        public static AppWorkState WorkState;

        public static string ServerConnectionString;
        public static readonly string ServerConnectionStringLiteral = "connectionString";

        #endregion

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var main = new MainWindow(new ViewModels.MainWindowViewModel());
            main.Show();
        }
    }
}
