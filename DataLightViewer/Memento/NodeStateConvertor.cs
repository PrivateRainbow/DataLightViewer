using DataLightViewer.ViewModels;
using Loader.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLightViewer.Memento
{
    public static class NodeStateConvertor
    {
        #region Attributes for representing UI-state

        private const string IsExpandedAttr = "isExpanded";
        private const string IsSelectedAttr = "isSelected";
        private const string ShowChildrenQuantityAttr = "showChildrenQuantity";
        #endregion

        /// <summary>
        /// Convert inner node of VM to common node with attributes 
        /// which are necessary for recovering UI-state of App after opening project file
        /// </summary>
        /// <returns></returns>
        public static Node ToUIStateNode(this NodeViewModel vm)
        {
            // dummy node has no inner node
            var name = vm?.InnerNode?.Name ?? vm.Name;
            var node = new Node(name);

            var isExpandedState = new KeyValuePair<string, string>(IsExpandedAttr, vm.IsExpanded.ToString());
            var isSelectedState = new KeyValuePair<string, string>(IsSelectedAttr, vm.IsSelected.ToString());
            var showChildrenQuantityState = new KeyValuePair<string, string>(ShowChildrenQuantityAttr, vm.ShowChildrenQuantity.ToString());

            node.AttachAttribute(isExpandedState);
            node.AttachAttribute(isSelectedState);
            node.AttachAttribute(showChildrenQuantityState);

            return node;
        }

        public static void FillViewNodeStateFrom(this NodeViewModel vm, Node sourceNode)
        {
            vm.IsSelected = Convert.ToBoolean(sourceNode.Attributes[IsSelectedAttr]);
            vm.IsExpanded = Convert.ToBoolean(sourceNode.Attributes[IsExpandedAttr]);
            vm.ShowChildrenQuantity = Convert.ToBoolean(sourceNode.Attributes[ShowChildrenQuantityAttr]);
        }

        public static void TransformToUiNode(NodeViewModel vmNode, Node uiNode)
        {
            var currentUiNode = vmNode.ToUIStateNode();
            uiNode.Add(currentUiNode);

            foreach (var child in vmNode.Children)
            {
                if (ReferenceEquals(child, NodeViewModel.ArtificialChild) || child.MarkedAsDeleted)
                    continue;
                TransformToUiNode(child, currentUiNode);
            }
        }


        public static bool TraverseNodeForDataCompletion(NodeViewModel vmNode, Node uiNode)
        {
            var ui = new Queue<Node>();
            var vm = new Queue<NodeViewModel>();

            ui.Enqueue(uiNode);
            vm.Enqueue(vmNode);

            while (ui.Any())
            {
                var currentUI = ui.Dequeue();
                var currentVM = vm.Dequeue();

                currentVM.FillViewNodeStateFrom(currentUI);

                for (var i = 0; i < currentUI.Children.Count; i++)
                {
                    ui.Enqueue(currentUI.Children[i]);
                    vm.Enqueue(currentVM.Children[i]);
                }
            }
            return true;
        }

        public static void TransformToViewModelNode(NodeViewModel vmNode, Node uiNode)
        {
            vmNode.FillViewNodeStateFrom(uiNode);

            for (var i = 0; i < uiNode.Children.Count; i++)
                TransformToViewModelNode(vmNode.Children[i], uiNode.Children[i]);

        }
    }

}
