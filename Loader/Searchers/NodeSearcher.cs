using System;
using System.Collections.Generic;

using Loader.Components;
using Loader.Helpers;
using Loader.Traversals;
using Loader.Types;


namespace Loader.Searchers
{
    public sealed class NodeSearcher
    {
        private readonly INodeTraverser _nodeTraverser;
        public NodeSearcher(INodeTraverser nodeTraverser)
        {          
            if(nodeTraverser == null)
                throw new ArgumentNullException($"{nameof(nodeTraverser)}");
            _nodeTraverser = nodeTraverser;
        }

        public Node SearchTargetNode(Node targetNode, string targetName = null, Dictionary<string, string> targetAttributes = null)
        {
            if(targetNode == null)
                throw new ArgumentNullException($"{nameof(targetNode)}");

            Predicate<Node> searchByName = n => n.Name == targetName;
            Predicate<Node> searchByAttr = node =>
            {
                if (!node.HasAttributes() || targetAttributes.Count > node.Attributes.Count)
                    return false;                             

                var coincidence = 0;
                var count = targetAttributes.Count;

                foreach (var attr in targetAttributes)
                {
                    if (string.IsNullOrEmpty(attr.Key)) return false;
                    if (node.Attributes.ContainsKey(attr.Key))
                    {
                        if (attr.Value == node.Attributes[attr.Key])
                            coincidence++;
                    }
                    else
                        return false;
                }
                return coincidence == count;
            };

            var request = SearchRequestType.Empty;

            if(!string.IsNullOrEmpty(targetName))
                request |= SearchRequestType.ByName;

            if(targetAttributes != null && targetAttributes.Count > 0)
                request |= SearchRequestType.ByAttributes;

            Predicate<Node> conditionsSet = null;
            switch (request)
            {
                case SearchRequestType.Empty:
                    break;
                case SearchRequestType.Empty | SearchRequestType.ByName:
                    conditionsSet = searchByName;
                    break;
                case SearchRequestType.Empty | SearchRequestType.ByAttributes:
                    conditionsSet = searchByAttr;
                    break;
                case SearchRequestType.Empty | SearchRequestType.ByName | SearchRequestType.ByAttributes:
                    conditionsSet = searchByName.And(searchByAttr);
                    break;
            }

            return conditionsSet != null ? _nodeTraverser.Traverse(targetNode, conditionsSet) : null;
        }
    }


}
