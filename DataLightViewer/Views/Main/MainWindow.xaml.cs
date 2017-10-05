using System.Windows;
using DataLightViewer.ViewModels;

namespace DataLightViewer.Views.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainVm;
        public MainWindow(MainWindowViewModel mainVm)
        {
            _mainVm = mainVm;

            Loaded += MainWindow_Loaded;
            Closing += _mainVm.AppShutDownHandler;

            DataContext = _mainVm;
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AvalongTextEditor.SyntaxHighlighting = SyntaxHighlightResolver.ResolveHiglightingDefinition(Loader.Types.SqlNodeBuilderType.TransactSql);
        }
    }
}
