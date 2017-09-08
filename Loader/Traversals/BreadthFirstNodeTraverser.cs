using System;
using System.Collections.Generic;
using System.Linq;
using Loader.Components;

namespace Loader.Traversals
{
    internal class BreadthFirstNodeTraverser : INodeTraverser
    {
        public Node Traverse(Node targetNode, Predicate<Node> condition)
        {
            if (targetNode == null)
                throw new ArgumentNullException($"{nameof(targetNode)}");
            if (condition == null)
                throw new ArgumentNullException($"{nameof(condition)}");

            if (condition(targetNode))
                return targetNode;

            var queue = new Queue<Node>();
            queue.Enqueue(targetNode);

            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var child in current.Children)
                {
                    if (condition(child))
                        return child;

                    queue.Enqueue(child);
                }
            }

            return null;
        }
    }
}