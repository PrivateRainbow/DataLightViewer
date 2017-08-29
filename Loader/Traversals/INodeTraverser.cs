using System;
using Loader.Components;

namespace Loader.Traversals
{
    public interface INodeTraverser
    {
        Node Traverse(Node target, Predicate<Node> condition);
    }
}
