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
using System.Windows.Media.Imaging;

namespace DataLightViewer.ViewModels
{
    public class NodeViewModel : BaseViewModel
    {
        public static readonly NodeViewModel ArtificialChild = new NodeViewModel() { Name = "DummyChild" };

        #region Private members

        private static readonly BaseDbNodeBuilder _dbNodeBuilder;
        private static readonly SqlNodeBuilder _sqlNodeBuilder;
        private static bool _scriptInConstruction;
        private static bool _dataInConstruction;

        private string _name;
        private DbSchemaObjectType _type;
        private string _content;
        private string _sqlScript;
        private int _childrenQuantity;

        private Node _node;
        private NodeViewModel _parent;
        private BitmapImage _image;
        private NodeViewModel _selectedItem;
        private ObservableCollection<NodeViewModel> _children;

        private bool _isExpandable;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _markedAsDeleted;
        private bool _showChildrenQuantity;

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
        public DbSchemaObjectType Type
        {
            get => _type;
            set
            {
                if (_type == value)
                    return;
                _type = value;
            }
        }
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
        public string Script
        {
            get { return _sqlScript; }
            set
            {
                if (ReferenceEquals(_sqlScript, value))
                    return;
                _sqlScript = value;
                OnPropertyChanged(nameof(Script));
            }
        }
        public int ChildrenQuantity
        {
            get => _childrenQuantity;
            set
            {
                if (_childrenQuantity == value)
                    return;
                _childrenQuantity = value;
                OnPropertyChanged(nameof(ChildrenQuantity));
            }
        }

        public Node InnerNode
        {
            get => _node;
            private set
            {
                if (ReferenceEquals(_node, value))
                    return;
                _node = value;
            }
        }
        public BitmapImage Image
        {
            get => _image;
            set
            {
                if (ReferenceEquals(value, _image))
                    return;
                _image = value;
            }
        }
        public NodeViewModel Parent
        {
            get => _parent;
            private set
            {
                if (ReferenceEquals(_parent, value))
                    return;
                _parent = value;
            }
        }
        public ObservableCollection<NodeViewModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;

                ChildrenQuantity = _children.Count;
                ShowChildrenQuantityIfNeed();

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

                    if (_isSelected)
                    {
                        _selectedItem = this;
                        Messenger.Instance.NotifyColleagues(MessageType.NodeSelection, InnerNode);
                    }

                    OnPropertyChanged(nameof(IsSelected));
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

                    if (_dataInConstruction)
                    {
                        RefreshWhenLoading();
                        return;
                    }

                    if (CanExpand())
                        ExpandAsync();

                    OnPropertyChanged(nameof(IsExpanded));
                }

                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;
            }
        }

        public bool IsExpandable
        {
            get => _isExpandable;
            set
            {
                if (value != _isExpandable)
                {
                    _isExpandable = value;
                    OnPropertyChanged(nameof(IsExpandable));
                }
            }
        }
        public bool MarkedAsDeleted
        {
            get => _markedAsDeleted;
            set
            {
                if (value != _markedAsDeleted)
                {
                    _markedAsDeleted = value;
                    OnPropertyChanged(nameof(MarkedAsDeleted));
                }
            }
        }
        public bool ShowChildrenQuantity
        {
            get => _showChildrenQuantity;
            set
            {
                if (value != _showChildrenQuantity)
                {
                    _showChildrenQuantity = value;
                    OnPropertyChanged(nameof(ShowChildrenQuantity));
                }
            }
        }

        public bool ChildrenDownloaded => Children.Count > 0 && !Children.Contains(ArtificialChild);
        public bool HasArtificialChild => Children.Count == 1 && Children[0] == ArtificialChild;

        public bool CanExpand()
        {
            if (!IsExpandable)
                return false;

            if (MarkedAsDeleted)
                return false;

            return !ChildrenDownloaded;
        }
        #endregion

        #region Commands

        public ICommand BuildDataCommand { get; }
        public ICommand BuildSqlCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Init

        static NodeViewModel()
        {
            _dbNodeBuilder = DbNodeBuilderFactory.Make(DbNodeBuilderType.Lazy, App.ServerConnectionString);
            _sqlNodeBuilder = new SqlNodeBuilder(SqlNodeBuilderFactory.Make(SqlNodeBuilderType.TransactSql));
        }

        /// <summary>
        /// Such ctor is used for creating artificial node, 
        /// which is used by UI-part ( TreeView ) to expand node in lazy style 
        /// </summary>
        private NodeViewModel()
        {
            Children = new ObservableCollection<NodeViewModel>();
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
            Parent = parent;
            InnerNode = node;
            Type = InnerNode.ResolveDbNodeType();
            Image = InnerNode.GetIcon();
            Content = InnerNode.GetContent();
            Name = InnerNode.GetName();
            IsExpandable = InnerNode.IsExpandable();

            if (lazyLoadChildren)
                SetArtificalNode();
            else
            {
                var children = InnerNode.Children.Select(n => new NodeViewModel(n, parent: this, lazyLoadChildren: lazyLoadChildren));

                if (children.Any())
                    Children = new ObservableCollection<NodeViewModel>(children);
                else
                    SetArtificalNode();
            }

            BuildSqlCommand = new RelayCommand(CreateScriptAsync, CanBuildSqcriptCommand);
            RefreshCommand = new RelayCommand(Refresh, CanBeRefreshed);
        }

        #endregion

        #region Tasks

        private async void ExpandAsync()
        {
            _dataInConstruction = true;

            Children.Clear();

            var context = new BuildContext
            {
                Node = _selectedItem.InnerNode,
                Connection = _selectedItem.InnerNode.GetConnectionString()
            };

            LogWrapper.WriteInfo($"Loading data for {_selectedItem.InnerNode.Name}");

            var loadingTask = new ExecutingTask("Loading ...");
            TaskExecutionMonitor.AttachMonitoring(loadingTask);

            try
            {
                await Task.Run(() => _dbNodeBuilder.MakeNode(context)).ContinueWith(pr => InitializeChildren(pr.Result));
                TaskExecutionMonitor.DetachMonitoring(loadingTask);

                _dataInConstruction = false;
            }
            catch (Exception ex)
            {
                Refresh();
                _dataInConstruction = false;
                LogWrapper.WriteError("Server is not responding.", ex);

                TaskExecutionMonitor.DetachMonitoring(loadingTask);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Can not load data from server."));

                MessageBox.Show("Server is not responding. Use Tools -> Connect To Server in order to check the server`s state.", "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void CreateScriptAsync()
        {
            var message = $"Building sql-script for {_selectedItem._node.Name}";
            LogWrapper.WriteInfo(message);

            var constuctingTask = new ExecutingTask("Constructing script ...");
            TaskExecutionMonitor.AttachMonitoring(constuctingTask);

            if (!string.IsNullOrEmpty(Script))
            {
                SendSqlScript(Script);
                TaskExecutionMonitor.DetachMonitoring(constuctingTask);
                return;
            }

            try
            {
                _scriptInConstruction = true;

                if (App.WorkState == Models.AppWorkState.Offline)
                    await BuildScriptAsync();
                else
                    await ResolveDataAsync().ContinueWith(pr => BuildScriptAsync());

                _scriptInConstruction = false;
                TaskExecutionMonitor.DetachMonitoring(constuctingTask);
            }
            catch (Exception ex)
            {
                Refresh();
                _scriptInConstruction = false;
                TaskExecutionMonitor.DetachMonitoring(constuctingTask);

                LogWrapper.WriteError("Sql construction error.", ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Sql-script construction failed."));

                MessageBox.Show("An error has occurred during SQL construction", "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanBuildSqcriptCommand(object o)
        {
            if (Type == DbSchemaObjectType.Server)
                return false;

            if (MarkedAsDeleted)
                return false;

            if (_scriptInConstruction)
                return false;

            return NodeDataPresenter.CanChildrenBeCreatedWithoutParent(InnerNode);
        }

        private Task ResolveDataAsync()
        {
            try
            {
                return Task.Run(() => IntegrityDataResolver.ResolveNodeStateForScriptGenerating(this));
            }
            catch (Exception ex) { throw; }
        }

        private Task BuildScriptAsync()
        {
            return Task.Run(() => _sqlNodeBuilder.BuildScript(_selectedItem._node)).ContinueWith(data =>
            {
                try
                {
                    Script = data.Result;
                    SendSqlScript(Script);
                }
                catch (Exception ex) { throw; }
            });
        }

        #endregion

        #region Methods 

        private void InitializeChildren(List<Node> children)
        {
            if (children.Any())
            {
                var first = children.First();
                if (!first.IsArtificalSchemaNode())
                    children.Sort((dboA, dboB) => dboA.Attributes["name"].CompareTo(dboB.Attributes["name"]));
            }

            children.ForEach(ch => _selectedItem._node.Add(ch));
            Children = new ObservableCollection<NodeViewModel>(children.Select(c => new NodeViewModel(c, parent: this, lazyLoadChildren: false)));

            LogWrapper.WriteInfo($"Data for {_selectedItem._node.Name} were downloaded");

        }

        private void ShowChildrenQuantityIfNeed()
        {
            if (InnerNode != null && Children != null)
                ShowChildrenQuantity = InnerNode.IsArtificalSchemaNode() && ChildrenDownloaded;
        }

        private void SendSqlScript(string script)
        {
            Messenger.Instance.NotifyColleagues(MessageType.SqlPreparation, script);

            var message = $"Sql-script for {_selectedItem._node.Name} was succesfully constructed!";
            LogWrapper.WriteInfo(message);
        }

        private void RefreshWhenLoading()
        {
            IsExpanded = false;
            OnPropertyChanged(nameof(IsExpanded));
        }

        private void Refresh()
        {
            if (NodeDataPresenter.IsArtificalSchemaNode(InnerNode))
            {
                CleanCashedData();
                SetArtificalNode();
                return;
            }

            IntegrityDataResolver.SynchronizeNodeWithSource(this);
        }
        private bool CanBeRefreshed(object o)
        {
            return !MarkedAsDeleted;
        }

        public void RefreshStateOnSuccess(Node source)
        {
            var selfVmIndex = Parent.Children.IndexOf(this);
            var selfInnerIndex = Parent.InnerNode.Children.IndexOf(InnerNode);

            var local = new NodeViewModel(source, Parent, lazyLoadChildren: true);
            local.CleanCashedData();
            local.SetArtificalNode();

            Parent.Children.Remove(this);
            Parent.InnerNode.Children.Remove(InnerNode);

            Parent.Children.Insert(selfVmIndex, local);
            Parent.InnerNode.Insert(selfInnerIndex, local.InnerNode);

            local.IsSelected = true;
        }

        public void RefreshStateOnFail()
        {
            MarkedAsDeleted = true;

            CleanCashedData();
            ResetInvalidatingScriptCashes();

            Parent.InnerNode.Children.Remove(InnerNode);
            Parent.IsSelected = true;
        }

        private void SetArtificalNode()
        {
            Children = new ObservableCollection<NodeViewModel>();

            if (IsExpandable)
                Children.Add(ArtificialChild);
        }

        private void CleanCashedData()
        {
            IsExpanded = false;
            Children.Clear();
            InnerNode?.Children.Clear();
            Script = string.Empty;
        }

        private void ResetInvalidatingScriptCashes()
        {
            var current = Parent;
            while (current.Type != DbSchemaObjectType.Database)
            {
                if (!string.IsNullOrEmpty(current.Script))
                    current.Script = string.Empty;

                current = current.Parent;
            }
        }

        #endregion

    }
}
