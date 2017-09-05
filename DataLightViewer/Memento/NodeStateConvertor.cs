using DataLightViewer.ViewModels;
using Loader.Components;
using System;
using System.Collections.Generic;

namespace DataLightViewer.Memento
{
    public static class NodeStateConvertor
    {
        #region Attributes for representing UI-state

        private const string IsExpandedAttr = "isExpanded";
        private const string IsSelectedAttr = "isSelected";

        #endregion

        /// <summary>
        /// Convert inner node of VM to common node with attributes 
        /// which are necessary for recovering UI-state of App after opening project file
        /// </summary>
        /// <returns></returns>
        public static Node ToUIStateNode(this NodeViewModel vm)
        {
            var node = new Node(vm.Name.Replace(' ', '_'));

            var isExpandedState = new KeyValuePair<string, string>(IsExpandedAttr, vm.IsExpanded.ToString());
            var isSelectedState = new KeyValuePair<string, string>(IsSelectedAttr, vm.IsSelected.ToString());

            node.AttachAttribute(isExpandedState);
            node.AttachAttribute(isSelectedState);

            return node;
        }

        public static void FillViewNodeStateFrom(this NodeViewModel vm, Node sourceNode)
        {
            vm.IsSelected = Convert.ToBoolean(sourceNode.Attributes[IsSelectedAttr]);
            vm.IsExpanded = Convert.ToBoolean(sourceNode.Attributes[IsExpandedAttr]);
        }

        public static void TransformToUiNode(NodeViewModel vmNode, Node uiNode)
        {
            var currentUiNode = vmNode.ToUIStateNode();
            uiNode.Add(currentUiNode);

            foreach (var child in vmNode.Children)
                TransformToUiNode(child, currentUiNode);
        }

        public static void TransformToViewModelNode(NodeViewModel vmNode, Node uiNode)
        {
            vmNode.FillViewNodeStateFrom(uiNode);

            if (vmNode.Children.Count == 0 && vmNode.IsExpandable)
                vmNode.Children.Add(NodeViewModel.ArtificialChildNode);
            else
            {
                for (var i = 0; i < vmNode.Children.Count; i++)
                    TransformToViewModelNode(vmNode.Children[i], uiNode.Children[i]);
            }
        }
    }

}
