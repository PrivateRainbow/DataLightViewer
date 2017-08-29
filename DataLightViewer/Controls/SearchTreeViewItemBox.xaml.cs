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

            Messenger.Instance.Subscribe(MessageType.FileProjectOpened, InitializeFromProjectFile);
            Messenger.Instance.Subscribe(MessageType.ConnectionEstablished, InitializeFromServerConnection);

            ClearFilter.Click += (obj, e) => {
                SearchTextBox.Clear();
            };

            SearchTextBox.KeyDown += (obj, e) => {
                if (e.Key == System.Windows.Input.Key.Enter)
                    _nodeTreeViewModel.SearchCommand.Execute(null);
            };
        }

        private void InitializeFromServerConnection(object authorized)
        {
            var auth = (bool)authorized;
            if (!auth) return;
            
            var serverNode = new Node(DbSchemaConstants.Server);
            serverNode.AttachAttribute(new KeyValuePair<string, string>(SqlQueryConstants.Name, App.ServerConnectionString.GetServerName()));
            
            _nodeTreeViewModel = new NodeTreeViewModel(serverNode, lazyLoadChildren: true);
            DataContext = _nodeTreeViewModel;
        }

        private void InitializeFromProjectFile(object projectFile)
        {
            if (projectFile is NodeMemento nodeMemento)
            {
                var serverNode = nodeMemento.NodeViewModel.InnerNode();
                serverNode.AttachAttribute(new KeyValuePair<string, string>(SqlQueryConstants.Name, App.ServerConnectionString.GetServerName()));

                _nodeTreeViewModel = new NodeTreeViewModel(serverNode, lazyLoadChildren: false);
                DataContext = _nodeTreeViewModel;
            }
        }

    }
}
