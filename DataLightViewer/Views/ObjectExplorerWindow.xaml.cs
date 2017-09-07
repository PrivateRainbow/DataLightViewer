using DataLightViewer.Helpers;
using DataLightViewer.ViewModels;
using System.Windows;

namespace DataLightViewer.Views
{
    /// <summary>
    /// Interaction logic for ObjectExplorerWindow.xaml
    /// </summary>
    public partial class ObjectExplorerWindow : Window
    {
        public ObjectExplorerWindow()
        {
            var vm = new ExplorerViewModel(this);
            vm.ValidationCheckMessage += ValidationCheckHandler;

            DataContext = vm;

            InitializeComponent();
        }

        private void ValidationCheckHandler(object sender, System.EventArgs e)
        {
            passwordField.GetBindingExpression(PasswordBoxAssistant.BoundPassword).UpdateSource();
        }
    }
}
