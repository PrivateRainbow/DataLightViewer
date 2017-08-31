using DataLightViewer.Mediator;

namespace DataLightViewer.ViewModels
{
    public class StatusViewModel : BaseViewModel
    {
        private const string DefaultMessage = "Ready";

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public StatusViewModel()
        {
            Messenger.Instance.Register<string>(MessageType.ExecutionStatus, UpdateStatus);
            Status = DefaultMessage;
        }

        private void UpdateStatus(string source) => Status = source ?? DefaultMessage;
    }
}
