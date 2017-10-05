using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Types;
using Loader.Services.Types;

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
                .Attributes[SqlQueryConstants.ObjectIdParamLiteral];

            return objectId;
        }

        public static string GetId(this Node node) => node.Attributes[SqlQueryConstants.ObjectIdParamLiteral];
        
        public static SchemaNavigationContext GetNavigationContext(this Node node, NavigationType selfContextType)
        {
            switch(selfContextType)
            {
                case NavigationType.Self:
                    var id = node.Name == DbSchemaConstants.Database ? node.Attributes["database_id"] : node.GetId();
                    return SchemaNavigationContext.GetFromId(id);

                case NavigationType.FromParent:
                    return SchemaNavigationContext.GetFromId(
                        node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table ||
                                                           n.Name == DbSchemaConstants.View ||
                                                           n.Name == DbSchemaConstants.Procedure).GetId()
                                                           );

                case NavigationType.WithParent:
                        return SchemaNavigationContext.GetFromSelfWithParent(
                            node.GetId(),
                            node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table ||
                                                               n.Name == DbSchemaConstants.View ||
                                                               n.Name == DbSchemaConstants.Procedure).GetId()
                                                               );

                default:
                    throw new ArgumentException($"{nameof(selfContextType)}");
            }
        }

    }
}
