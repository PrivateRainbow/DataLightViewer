using DataLightViewer.Helpers;
using DataLightViewer.ViewModels;
using System;
using System.Windows;
using System.ComponentModel;

namespace DataLightViewer.Views
{
    /// <summary>
    /// Interaction logic for ObjectExplorerWindow.xaml
    /// </summary>
    public partial class ObjectExplorerWindow : Window
    {
        private readonly ExplorerViewModel _explorerVm;

        public ObjectExplorerWindow(ExplorerViewModel explorer)
        {
            _explorerVm = explorer;
            _explorerVm.ValidationCheckMessage += ValidationCheckHandler;
            _explorerVm.UpdatePasswordMessage += UpdatePasswordHandler;
            _explorerVm.ConnectionEstablished += ConnectionEstablishedHandler;

            DataContext = _explorerVm;            
            InitializeComponent();
        }

        private void ConnectionEstablishedHandler(object sender, EventArgs e) => this.Close();
        private void ValidationCheckHandler(object sender, System.EventArgs e) => passwordField.GetBindingExpression(PasswordBoxAssistant.BoundPassword).UpdateSource();
        private void UpdatePasswordHandler(object sender, System.EventArgs e)
        {
            passwordField.Password = string.Empty;
            passwordField.GetBindingExpression(PasswordBoxAssistant.BoundPassword).UpdateTarget();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _explorerVm.ValidationCheckMessage -= ValidationCheckHandler;
            _explorerVm.UpdatePasswordMessage -= UpdatePasswordHandler;
            _explorerVm.ConnectionEstablished -= ConnectionEstablishedHandler;
        }
    }
}
