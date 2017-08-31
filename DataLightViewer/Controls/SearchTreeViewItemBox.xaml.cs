using DataLightViewer.Helpers;
using DataLightViewer.Mediator;
using DataLightViewer.Memento;
using DataLightViewer.ViewModels;
using Loader.Components;
using Loader.Types;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DataLightViewer.Controls
{
    public partial class SearchTreeViewItemControl : UserControl
    {
        private NodeTreeViewModel _nodeTreeViewModel;

        public SearchTreeViewItemControl()
        {
            InitializeComponent();

            Messenger.Instance.Register<NodeMemento>(MessageType.OnOpeningProjectFile, nm => InitializeFromProjectFile(nm));
            Messenger.Instance.Register(MessageType.ConnectionEstablished, InitializeFromServerConnection);

            ClearFilter.Click += (obj, e) =>
            {
                SearchTextBox.Clear();
            };

            SearchTextBox.KeyDown += (obj, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    _nodeTreeViewModel.SearchCommand.Execute(null);
            };
        }

        // Move to Vm
        private void InitializeFromServerConnection()
        {
            var serverNode = new Node(DbSchemaConstants.Server);
            serverNode.AttachAttribute(new KeyValuePair<string, string>(SqlQueryConstants.Name, App.ServerConnectionString.GetServerName()));

            _nodeTreeViewModel = new NodeTreeViewModel(serverNode, lazyLoadChildren: true);
            DataContext = _nodeTreeViewModel;
        }

        // Move to Vm
        private void InitializeFromProjectFile(NodeMemento memento)
        {           
            _nodeTreeViewModel = new NodeTreeViewModel(memento.NodeViewModel);
            Dispatcher.Invoke(() => DataContext = _nodeTreeViewModel);
        }

    }
}
