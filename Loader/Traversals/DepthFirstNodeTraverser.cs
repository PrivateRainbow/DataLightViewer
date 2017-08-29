using System;
using Loader.Components;

namespace Loader.Traversals
{
    internal class DepthFirstNodeTraverser : INodeTraverser
    {
        private Node _searchNode;
        private bool _founded;

        public Node Traverse(Node target, Predicate<Node> condition)
        {
            if (target == null)
                throw new ArgumentNullException($"{nameof(target)}");
            if (condition == null)
                throw new ArgumentNullException($"{nameof(condition)}");

            if (_founded) return _searchNode;
            if (condition(target)) return target;

            foreach (var child in target.Children)
            {
                if (_founded) break;
                if (condition(child))
                {
                    _searchNode = child;
                    _founded = true;
                    break;
                }
                Traverse(child, condition);
            }
            return _searchNode;
        }      
    }
}
