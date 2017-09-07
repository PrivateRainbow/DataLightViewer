using System.Collections.ObjectModel;
using Loader.Components;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows;
using DataLightViewer.Memento;
using DataLightViewer.Mediator;
using Loader.Types;
using DataLightViewer.Helpers;
using DataLightViewer.Commands;
using DataLightViewer.Filters;

namespace DataLightViewer.ViewModels
{
    public sealed class NodeTreeViewModel : BaseViewModel
    {
        #region Data

        private NodeViewModel _rootNode;
        private IEnumerator<NodeViewModel> _filteredNodeEnumerator;

        #endregion

        #region Properties

        private ObservableCollection<NodeViewModel> _items;
        public ObservableCollection<NodeViewModel> Items
        {
            get { return _items; }
            set
            {
                if (ReferenceEquals(_items, value))
                    return;

                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (ReferenceEquals(_searchText, value))
                    return;

                _filteredNodeEnumerator = null;

                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        #endregion

        #region Init

        public NodeTreeViewModel()
        {
            Messenger.Instance.Register<NodeMemento>(MessageType.OnOpeningProjectFile, nm => InitializeFromProjectFile(nm));
            Messenger.Instance.Register<MainWindowViewModel>(MessageType.OnSavingProjectFile, wvm => InitializeMemento(wvm));
            Messenger.Instance.Register(MessageType.OnInitializingProjectFile, InitializeFromServerConnection);

            SearchCommand = new SearchNodeTreeCommand(this);
        }

        private NodeTreeViewModel(Node node, bool lazyLoadChildren):this()
        {
            _rootNode = new NodeViewModel(node, parent: null, lazyLoadChildren: lazyLoadChildren);
            Items = new ObservableCollection<NodeViewModel>(new List<NodeViewModel> { _rootNode });         
        }

        private void InitializeMemento(MainWindowViewModel sender)
        {
            var memento = new NodeMemento(_items.First());
            Messenger.Instance.NotifyColleagues(MessageType.MementoInitialized, memento);
        }

        #region Subscription

        private void InitializeFromServerConnection()
        {
            var serverNode = new Node(DbSchemaConstants.Server);
            serverNode.AttachAttribute(new KeyValuePair<string, string>(SqlQueryConstants.Name, App.ServerConnectionString.GetServerName()));

            _rootNode = new NodeViewModel(serverNode, parent: null, lazyLoadChildren: true);
            Items = new ObservableCollection<NodeViewModel>(new List<NodeViewModel> { _rootNode });
        }

        private void InitializeFromProjectFile(NodeMemento memento)
        {
            _rootNode = memento.NodeViewModel;

            SearchText = string.Empty;
            Items = new ObservableCollection<NodeViewModel>(new List<NodeViewModel> { _rootNode });
        }

        #endregion

        #endregion

        #region Search 

        public ICommand SearchCommand { get; }
        public ICommand ClearCommand {
            get {
                return new RelayCommand(() => {
                    SearchText = string.Empty;
                    _filteredNodeEnumerator = null;
                });
            }
        }

        private class SearchNodeTreeCommand : ICommand
        {
            private readonly NodeTreeViewModel _nodeTree;
            public SearchNodeTreeCommand(NodeTreeViewModel nodeTree)
            {
                _nodeTree = nodeTree;
            }

            #region Implementation

            event EventHandler ICommand.CanExecuteChanged
            {
                add { }
                remove { }
            }
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _nodeTree.PerformSearch();

            #endregion
        }

        private void PerformSearch()
        {
            LogWrapper.WriteInfo($"Searching node with {_searchText} name", "Searching ...");

            if (_filteredNodeEnumerator == null || !_filteredNodeEnumerator.MoveNext())
                VerifyMatchingNodeEnumerator();

            var node = _filteredNodeEnumerator.Current;

            if (node == null)
                return;

            if (node.Parent != null)
                node.Parent.IsExpanded = true;

            node.IsSelected = true;

            LogWrapper.WriteInfo($"Node with such name ({_searchText}) was found!");
        }
               
        private void VerifyMatchingNodeEnumerator()
        {
            var searchFilter = NodeSearchFilters.GetFilterBySearchType(SearchFilterType.ByName);

            var matches = Find(searchFilter, _rootNode, SearchText);
            _filteredNodeEnumerator = matches.GetEnumerator();

            if (!_filteredNodeEnumerator.MoveNext())
            {
                LogWrapper.WriteInfo($"Node with such name ({_searchText}) was not found!", "Not found.");
                MessageBox.Show(
                    "No matching names were found.",
                    "Try Again",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
            }
        }


        private static IEnumerable<NodeViewModel> Find(SearchFilter filter, NodeViewModel parent, string searchText)
        {
            if (filter(parent, searchText))
                yield return parent;

            foreach (var child in parent.Children)
            {
                foreach (var match in Find(filter, child, searchText))
                    yield return match;
            }
        }

        #endregion

    }

}
