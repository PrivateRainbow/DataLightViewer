using DataLightViewer.ViewModels;
using Loader.Services.Builders;
using Loader.Factories;
using Loader.Types;
using Loader.Services.Types;
using System.Collections.Generic;
using DataLightViewer.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using Loader.Components;
using DataLightViewer.Services;

namespace DataLightViewer
{
    public static class IntegrityDataResolver
    {
        #region Data

        private static readonly byte ThresholdForTable = 2;
        private static readonly byte ThresholdForView = 1;
        private static readonly byte ThresholdForProcedure = 0;

        private static readonly BaseDbNodeBuilder _bulkNodeBuilder, _lazyNodeBuilder, _slimNodeBuilder;
        private static readonly Dictionary<DbSchemaObjectType, byte> _thresholdValues;

        #endregion

        #region Init

        static IntegrityDataResolver()
        {
            _bulkNodeBuilder = DbNodeBuilderFactory.Make(DbNodeBuilderType.Bulk, App.ServerConnectionString);
            _lazyNodeBuilder = DbNodeBuilderFactory.Make(DbNodeBuilderType.Lazy, App.ServerConnectionString);
            _slimNodeBuilder = DbNodeBuilderFactory.Make(DbNodeBuilderType.Slim, App.ServerConnectionString);

            _thresholdValues = new Dictionary<DbSchemaObjectType, byte>
            {
                { DbSchemaObjectType.Table, ThresholdForTable },
                { DbSchemaObjectType.View, ThresholdForView },
                { DbSchemaObjectType.Procedure, ThresholdForProcedure }
            };
        }
        #endregion

        public static void ResolveNodeStateForScriptGenerating(NodeViewModel node) {

            switch (node.Type)
            {
                case DbSchemaObjectType.Database:
                case DbSchemaObjectType.Tables:
                case DbSchemaObjectType.Views:
                case DbSchemaObjectType.Procedures:
                case DbSchemaObjectType.Columns:
                case DbSchemaObjectType.Keys:
                case DbSchemaObjectType.Constraints:
                case DbSchemaObjectType.Indexes:
                    BulkLoadingChildrenFor(node, withCleaning: true);
                    break;

                case DbSchemaObjectType.Table:
                case DbSchemaObjectType.View:
                case DbSchemaObjectType.Procedure:
                    LoadIfNeeded(node);
                    break;
            }
        }

        public static void SynchronizeNodeWithSource(NodeViewModel target)
        {
            try
            {
                switch (target.Type)
                {
                    case DbSchemaObjectType.Database:
                    case DbSchemaObjectType.Table:
                    case DbSchemaObjectType.View:
                    case DbSchemaObjectType.Procedure:
                    case DbSchemaObjectType.Column:
                    case DbSchemaObjectType.Key:
                    case DbSchemaObjectType.Constraint:
                    case DbSchemaObjectType.Index:
                    case DbSchemaObjectType.Parameter:

                        var source = _slimNodeBuilder.MakeNode(target.GetBuildContext()).Single();
                        target.RefreshStateOnSuccess(source);
                        break;

                    default:
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                LogWrapper.WriteError($"Node was deleted from source ({target.Name})", ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Node was deleted from source."));

                target.RefreshStateOnFail();
            }
            catch (Exception ex)
            {
                LogWrapper.WriteError("Server is not responding.", ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Can not load data from the server."));
                MessageBox.Show("Server is not responding.", "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Helpers

        private static void RefreshOnSuccess(NodeViewModel target, Node source)
        {
            var parent = target.Parent;
            parent.Children.Remove(target);
            parent.InnerNode.Children.Remove(target.InnerNode);

            var local = new NodeViewModel(source, parent, lazyLoadChildren: true);
            parent.Children.Add(local);
            parent.InnerNode.Add(local.InnerNode);

            local.IsSelected = true;
            if (local.IsExpandable)
                local.IsExpanded = true;
        }

        private static void LoadIfNeeded(NodeViewModel node)
        {
            if (node.HasArtificialChild)
            {
                BulkLoadingChildrenFor(node);
                return;
            }
            
            var notLoadedChildren = GetNotLoadedChildren(node);

            if (notLoadedChildren.Count == 0)
                return;

            if (notLoadedChildren.Count > _thresholdValues[node.Type])
                BulkLoadingChildrenFor(node, withCleaning: true);
            else
                LazyLoadingChildrenFor(node, notLoadedChildren);
        }

        private static List<NodeViewModel> GetNotLoadedChildren(NodeViewModel node)
        {
            var notLoadedChildren = new List<NodeViewModel>();

            foreach (var child in node.Children)
                if (!child.ChildrenDownloaded)
                    notLoadedChildren.Add(child);
           
            return notLoadedChildren;
        }

        private static void LazyLoadingChildrenFor(NodeViewModel node, List<NodeViewModel> notLoadedChildren)
        {
            foreach (var child in notLoadedChildren)
            {
                var children = _lazyNodeBuilder.MakeNode(new BuildContext(child.InnerNode, child.InnerNode.GetConnectionString()));

                children.ForEach(ch => node.InnerNode.Add(ch));
                child.Children = new ObservableCollection<NodeViewModel>(children.Select(c => new NodeViewModel(c, parent: node, lazyLoadChildren: true)));
            }
        }

        private static void BulkLoadingChildrenFor(NodeViewModel node, bool withCleaning = false)
        {
            if(withCleaning)
                node.InnerNode.Children.Clear();

            var children = _bulkNodeBuilder.MakeNode(node.GetBuildContext());

            children.ForEach(ch => node.InnerNode.Add(ch));
            node.Children = new ObservableCollection<NodeViewModel>(children.Select(c => new NodeViewModel(c, parent: node, lazyLoadChildren: false)));
        }

        #endregion
    }
}
