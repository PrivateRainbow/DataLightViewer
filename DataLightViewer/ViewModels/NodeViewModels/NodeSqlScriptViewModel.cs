using DataLightViewer.Commands;
using DataLightViewer.Helpers;
using DataLightViewer.Mediator;
using DataLightViewer.Services;
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
            Messenger.Instance.Register<string>(MessageType.SqlPreparation, UpdateScript);            

            SaveScriptCommand = new RelayCommand(() => WriteScriptAsync(), CanDoScript);
            ClearScriptCommand = new RelayCommand(() => CleanContent(), CanDoScript);
        }

        private void UpdateScript(string source)
        {
            Script = string.Copy(source).TrimStart(Environment.NewLine.ToCharArray());
        }

        private bool CanDoScript(object o) => !string.IsNullOrEmpty(Script);

        private void CleanContent() => Script = string.Empty;

        private async void WriteScriptAsync()
        {
            try
            {
                var filename = DialogHelper.GetFileNameFromSaveDialog(DialogHelper.DialogSqlBundle);
                if (string.IsNullOrEmpty(filename))
                    return;

                LogWrapper.WriteInfo($"Starting to write script to {filename}");

                var writingScriptTask = new ExecutingTask("Writing script ...");
                TaskExecutionMonitor.AttachMonitoring(writingScriptTask);

                await Script.WriteToFileAsync(filename);

                TaskExecutionMonitor.DetachMonitoring(writingScriptTask);
                LogWrapper.WriteInfo($"Script has written.");
            }
            catch(ArgumentException ex)
            {
                var msg = "Error occurred during a recording operation!";
                LogWrapper.WriteError(msg, ex);
                TaskExecutionMonitor.MonitorOneTime( new ExecutingTask("Operation failed."));

                var result = MessageBox.Show("Invalid filename was entered. Do you want to try again !?", "DataToolsLight", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                    WriteScriptAsync();
                else
                    return;
            }
            catch (Exception ex)
            {
                var msg = "Error occurred during a recording operation!";
                LogWrapper.WriteError(msg, ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Operation failed."));

                MessageBox.Show(msg);
            }
        }
    }
}
