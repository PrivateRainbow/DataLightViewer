using DataLightViewer.Mediator;
using Loader.Components;

using System.Collections.Generic;

namespace DataLightViewer.ViewModels
{
    public sealed class NodePropertyViewModel : BaseViewModel
    {
        private Dictionary<string, string> _nodeProperties;
        public Dictionary<string,string> Properties
        {
            get => _nodeProperties;
            set
            {
                if (ReferenceEquals(_nodeProperties, value))
                    return;

                _nodeProperties = value;
                OnPropertyChanged(nameof(Properties));
            }
        }

        public NodePropertyViewModel()
        {
            Messenger.Instance.Subscribe(MessageType.NodeSelection, UpdateProperties);
        }

        private void UpdateProperties(object source)
        {
            var node = source as Node;
            Properties = node?.Attributes;
        }
    }
}
