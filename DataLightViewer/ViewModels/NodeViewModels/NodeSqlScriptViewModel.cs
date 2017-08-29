using DataLightViewer.Commands;
using DataLightViewer.Helpers;
using DataLightViewer.Mediator;
using System;
using System.Windows;
using System.Windows.Input;

namespace DataLightViewer.ViewModels
{
    public class NodeSqlScriptViewModel : BaseViewModel
    {
        private string _script;
        public string Script
        {
            get => _script;
            set
            {
                _script = value;
                OnPropertyChanged(nameof(Script));
            }
        }

        public ICommand SaveScriptCommand { get; }
        public ICommand ClearScriptCommand { get; }

        public NodeSqlScriptViewModel()
        {
            Messenger.Instance.Subscribe(MessageType.SqlPreparation, UpdateScript);

            SaveScriptCommand = new RelayCommand(() => WriteScriptAsync());
            ClearScriptCommand = new RelayCommand(() => CleanContent());
        }

        private void UpdateScript(object source)
        {
            var content = source as string;
            Script = content.TrimStart(Environment.NewLine.ToCharArray());
        }

        private void CleanContent() => Script = string.Empty;

        private async void WriteScriptAsync()
        {
            try
            {
                var filename = DialogHelper.GetFileNameFromSaveTextDialog();
                LogWrapper.WriteInfo($"Starting to write script to {filename}", "Writing script ...");
                await Script.WriteTextAsync(DialogHelper.GetFileNameFromSaveTextDialog());
            }
            catch (Exception ex)
            {
                var msg = "Error occurred during a recording operation!";
                LogWrapper.WriteError(msg, ex, "Operation failed.");
                MessageBox.Show(msg);
            }
        }
    }
}
