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
            Messenger.Instance.Subscribe(MessageType.ExecutionStatus, UpdateStatus);
            Status = DefaultMessage;
        }

        private void UpdateStatus(object source)
        {
            var data = source as string;
            Status = data ?? DefaultMessage;
        }
    }
}
