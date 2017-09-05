using DataLightViewer.ViewModels;
using System.Security;
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
            InitializeComponent();
            DataContext = new ExplorerViewModel(this);
        }
    }
}
