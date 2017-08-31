using System.Collections.ObjectModel;
using System.Linq;
using DataLightViewer.Services;
using DataLightViewer.Commands;
using Loader.Components;
using Loader.Factories;
using Loader.Services.Types;
using Loader.Types;
using Loader.Helpers;
using System.Windows.Input;
using System.Threading.Tasks;
using Loader.Services.Builders;
using Loader.Services.Helpers;
using DataLightViewer.Helpers;
using System.Collections.Generic;
using DataLightViewer.Mediator;
using System;
using System.Windows;

namespace DataLightViewer.ViewModels
{
    public class NodeViewModel : BaseViewModel
    {
        #region Private members

        private static readonly NodeViewModel ArtificialChild = new NodeViewModel() { Name = "DummyChild" };
        public static NodeViewModel ArtificialChildNode => ArtificialChild;

        private static readonly BaseDbNodeBuilder DbNodeBuilder;
        private static readonly SqlNodeBuilder SqlNodeBuilder;


        private Node _node;
        private NodeViewModel _selectedItem;
        private ObservableCollection<NodeViewModel> _children;

        private string _name;
        private bool _isExpandable;
        private bool _isExpanded;
        private bool _isSelected;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (ReferenceEquals(_name, value))
                    return;
                _name = value;
            }
        }
        public string Content { get; }
        public NodeViewModel Parent { get; }
        public DbSchemaObjectType Type { get; }
        public Node InnerNode => _node;
        public ObservableCollection<NodeViewModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    _selectedItem = this;

                    OnPropertyChanged(nameof(IsSelected));
                    Messenger.Instance.NotifyColleagues(MessageType.NodeSelection, _selectedItem._node);
                }
            }
        }
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _selectedItem = this;
                    _isExpanded = value;

                    if (HasArtificialChild)
                        ExpandAsync();

                    OnPropertyChanged(nameof(IsExpanded));
                }

                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;

            }
        }
        public bool IsExpandable => _isExpandable;

        public bool HasArtificialChild => Children.Count == 1 && Children[0] == ArtificialChild;
        public bool ChildrenDownloaded => Children.Count > 0 && !Children.Contains(ArtificialChild);


        #endregion

        #region Commands

        public ICommand BuildSqlCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Init

        static NodeViewModel()
        {
            DbNodeBuilder = DbNodeBuilderFactory.Make(DbNodeBuilderType.Lazy, App.ServerConnectionString);
            SqlNodeBuilder = new SqlNodeBuilder(SqlNodeBuilderFactory.Make(SqlNodeBuilderType.TransactSql));
        }

        /// <summary>
        /// Such ctor is used for creating artificial node, 
        /// which is used by UI-part ( TreeView ) to expand node in lazy style 
        /// </summary>
        private NodeViewModel()
        {
            _children = new ObservableCollection<NodeViewModel>();
        }

        /// <summary>
        /// Such ctor is used for creating of root node
        /// </summary>
        public NodeViewModel(Node node) : this(node, null, true) { }


        /// <summary>
        /// Such ctor is used for expanding nodes in lazy style 
        /// by setting artifical node as a child for current node
        /// </summary>
        public NodeViewModel(Node node, NodeViewModel parent, bool lazyLoadChildren)
        {
            _node = node;
            Parent = parent;

            Type = _node.ResolveDbNodeType();
            Content = NodeContentPresenter.GetContent(_node);
            Name = NodeContentPresenter.GetName(_node);
            _isExpandable = NodeContentPresenter.CanBeExpanded(_node);

            _children = new ObservableCollection<NodeViewModel>();

            if (lazyLoadChildren)
                SetArtificalNode();
            else
            {
                var children = _node.Children.Select(n => new NodeViewModel(n, parent: this, lazyLoadChildren: false));
                Children = new ObservableCollection<NodeViewModel>(children);
            }

            BuildSqlCommand = new RelayCommand(async () => await BuildSqlAsync());
            RefreshCommand = new RelayCommand(Refresh);
        }

        #endregion

        #region Tasks

        private void ExpandAsync()
        {
            if (ChildrenDownloaded)
                return;

            Children.Clear();

            var context = new BuildContext
            {
                Node = _selectedItem._node,
                Connection = _node.GetConnectionString()
            };

            LogWrapper.WriteInfo($"Loading data for {_selectedItem._node.Name}", "Loading ...");
            Task.Run(() => DbNodeBuilder.MakeNode(context)).ContinueWith(pr => InitializeChildren(pr.Result));
        }

        private Task BuildSqlAsync()
        {
            var message = $"Building sql-script for {_selectedItem._node.Name}";

            LogWrapper.WriteInfo(message, message);

            return Task.Run(() => SqlNodeBuilder.BuildScript(_selectedItem._node))
                .ContinueWith(pr =>
                {
                    try
                    {
                        SendSqlScript(pr.Result);
                    }
                    catch (Exception ex)
                    {
                        // Update UI-state
                        Refresh();

                        LogWrapper.WriteError("Server is not responding.", ex, "Can not load data from server.");
                        MessageBox.Show("Server is not responding.", "Error");
                    }
                });
        }

        private void Refresh()
        {
            //collapse current
            IsExpanded = false;

            // clear childrent of current NodeVM
            Children.Clear();

            // clear inner data in cashed node
            _selectedItem._node.Children.Clear();

            // make it available for further expand
            SetArtificalNode();
        }

        #endregion

        #region Methods 

        private void InitializeChildren(List<Node> children)
        {
            children.ForEach(ch => _selectedItem._node.Add(ch));
            Children = new ObservableCollection<NodeViewModel>(children.Select(c => new NodeViewModel(c, parent: this, lazyLoadChildren: true)));

            LogWrapper.WriteInfo($"Data for {_selectedItem._node.Name} was downloaded");
        }

        private void SendSqlScript(string script)
        {
            Messenger.Instance.NotifyColleagues(MessageType.SqlPreparation, script);

            var message = $"Sql-script for {_selectedItem._node.Name} was succesfully constructed!";
            LogWrapper.WriteInfo(message);
        }

        public bool NameContainsText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public void SetArtificalNode()
        {
            Children = new ObservableCollection<NodeViewModel>();

            if (_isExpandable)
                Children.Add(ArtificialChild);
        }

        #endregion

    }
}
