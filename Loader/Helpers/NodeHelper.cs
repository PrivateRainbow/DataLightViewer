using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Types;

namespace Loader.Helpers
{
    public static class NodeHelper
    {
        public static DbSchemaObjectType ResolveDbNodeType(this Node node)
        {
            if (!Enum.TryParse(node.Name, true, out DbSchemaObjectType type))
                throw new ArgumentException($"{nameof(DbSchemaObjectType)} is not valid!");

            return type;
        }

        public static Node GetParentNodeByCondition(this Node current, Predicate<Node> condition)
        {
            var targetNode = current.Parent;
            while (targetNode != null)
            {
                if (condition(targetNode))
                    break;
                targetNode = targetNode.Parent;
            }
            return targetNode;
        }

        public static string GetParentId(this Node node, string expectedName)
        {
            var parentNodes = new HashSet<string>
            {
                DbSchemaConstants.Table,
                DbSchemaConstants.View,
                DbSchemaConstants.Procedure
            };

            if (string.IsNullOrEmpty(expectedName))
                throw new ArgumentNullException($"{nameof(expectedName)}");

            if (!parentNodes.Contains(expectedName))
                throw new ArgumentNullException($"{nameof(expectedName)}");

            var objectId = node.GetParentNodeByCondition(n => n.Name == expectedName)
                .Attributes[SqlQueryConstants.ObjectIdLiteral];

            return objectId;
        }

        public static string GetId(this Node node)
        {
            var parentNodes = new HashSet<string>
            {
                DbSchemaConstants.Table,
                DbSchemaConstants.View,
                DbSchemaConstants.Procedure
            };

            if (!parentNodes.Contains(node.Name))
                throw new ArgumentNullException($"{nameof(node.Name)}");

            return node.Attributes[SqlQueryConstants.ObjectIdLiteral];
        }


    }
}
