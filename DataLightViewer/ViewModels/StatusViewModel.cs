using DataLightViewer.Mediator;
using DataLightViewer.Services;
using System.Collections.ObjectModel;

namespace DataLightViewer.ViewModels
{
    public class StatusViewModel : BaseViewModel
    {
        private const string DefaultMessage = "Ready";

        private string _statusExecutionMessage;
        public string Message
        {
            get => _statusExecutionMessage;
            set
            {
                _statusExecutionMessage = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private ObservableCollection<ExecutingTask> _executingTasks;
        private ObservableCollection<ExecutingTask> ExecutingTasks
        {
            get => _executingTasks;
            set
            { 
                _executingTasks = value;
                OnPropertyChanged(nameof(ExecutingTasks));
            }
        }

        public StatusViewModel()
        {
            Message = DefaultMessage;
            ExecutingTasks = new ObservableCollection<ExecutingTask>();
            
            Messenger.Instance.Register<ExecutingTask>(MessageType.ExecutionStatus, UpdateStatus);
        }

        private void UpdateStatus(ExecutingTask task)
        {
            if(task.IsActive)
                ExecutingTasks.Add(task);
            else
                ExecutingTasks.Remove(task);

            ProvideMessage();
        }

        private void ProvideMessage()
        {
            if (ExecutingTasks.Count > 1)
                Message = $"Executing {ExecutingTasks.Count} tasks ...";
            else if (ExecutingTasks.Count == 1)
                Message = ExecutingTasks[0]?.Message ?? DefaultMessage;
            else
                Message = DefaultMessage;
        }
    }
}
