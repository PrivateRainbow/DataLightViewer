using System.Collections.ObjectModel;
using Loader.Components;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows;
using DataLightViewer.Memento;
using DataLightViewer.Mediator;

namespace DataLightViewer.ViewModels
{
    public sealed class NodeTreeViewModel : BaseViewModel
    {
        #region Data

        private readonly NodeViewModel _rootNode;
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

                _searchText = value;
            }
        }

        #endregion

        #region Init
        public NodeTreeViewModel(NodeViewModel rootViewModel)
        {
            _rootNode = rootViewModel;
            Items = new ObservableCollection<NodeViewModel>(new List<NodeViewModel> { _rootNode });
        }

        public NodeTreeViewModel(Node node) : this(node, lazyLoadChildren: true) { }

        public NodeTreeViewModel(Node node, bool lazyLoadChildren)
        {
            _rootNode = new NodeViewModel(node, parent: null, lazyLoadChildren: lazyLoadChildren);
            Items = new ObservableCollection<NodeViewModel>(new List<NodeViewModel> { _rootNode });

            SearchCommand = new SearchNodeTreeCommand(this);

            Messenger.Instance.Register<MainWindowViewModel>(MessageType.OnSavingProjectFile, wvm => InitializeMemento(wvm));
        }

        private void InitializeMemento(MainWindowViewModel sender)
        {
            var memento = new NodeMemento(_items.First());
            Messenger.Instance.NotifyColleagues(MessageType.MementoInitialized, memento);
        }

        #endregion

        #region Search 

        public ICommand SearchCommand { get; }

        private class SearchNodeTreeCommand : ICommand
        {
            private readonly NodeTreeViewModel _nodeTree;
            public SearchNodeTreeCommand(NodeTreeViewModel familyTree)
            {
                _nodeTree = familyTree;
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
            var matches = FindMatches(_searchText, _rootNode);
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
        private static IEnumerable<NodeViewModel> FindMatches(string searchText, NodeViewModel node)
        {
            if (node.NameContainsText(searchText))
                yield return node;

            foreach (var child in node.Children)
            {
                foreach (var match in FindMatches(searchText, child))
                    yield return match;
            }
        }

        #endregion

    }

}
