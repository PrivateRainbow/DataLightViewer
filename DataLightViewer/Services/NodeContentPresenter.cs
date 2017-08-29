using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Types;
using Loader.Services.Helpers;

namespace DataLightViewer.Services
{
    internal static class NodeContentPresenter
    {
        private static readonly Dictionary<string, Func<Node, string>> ContentPresenters;
        private static readonly HashSet<string> ExpandableNodeTypes;
        private static readonly HashSet<string> ArtificialNodeTypes;

        static NodeContentPresenter()
        {
            ContentPresenters = new Dictionary<string, Func<Node, string>>
            {
                {DbSchemaConstants.Server, GetObjNameContent },
                {DbSchemaConstants.Table, GetTableObjNameContent },
                {DbSchemaConstants.View, GetFullObjNameContent },
                {DbSchemaConstants.Procedure, GetFullObjNameContent },

                {DbSchemaConstants.Database, GetObjNameContent},
                {DbSchemaConstants.Key, GetObjNameContent},
                {DbSchemaConstants.Constraint, GetObjNameContent},

                {DbSchemaConstants.Column, GetColumnContent },
                {DbSchemaConstants.Index, GetIndexContent },
                {DbSchemaConstants.ProcParameter, GetProcParameter }
            };

            ExpandableNodeTypes = new HashSet<string>
            {
                DbSchemaConstants.Server,
                DbSchemaConstants.Databases,
                DbSchemaConstants.Database,
                DbSchemaConstants.Tables,
                DbSchemaConstants.Table,
                DbSchemaConstants.Views,
                DbSchemaConstants.View,
                DbSchemaConstants.Procedures,
                DbSchemaConstants.Procedure,
                DbSchemaConstants.ProcParameters,
                DbSchemaConstants.Columns,
                DbSchemaConstants.Keys,
                DbSchemaConstants.Constraints,
                DbSchemaConstants.Indexes
            };

            ArtificialNodeTypes = new HashSet<string>
            {
                DbSchemaConstants.Server,
                DbSchemaConstants.Databases,
                DbSchemaConstants.Tables,
                DbSchemaConstants.Views,
                DbSchemaConstants.Procedures,
                DbSchemaConstants.Columns,
                DbSchemaConstants.Constraints,
                DbSchemaConstants.Keys,
                DbSchemaConstants.Indexes,
                DbSchemaConstants.ProcParameters
            };
        }

        public static bool CanBeExpanded(Node node)
            => ExpandableNodeTypes.Contains(node.Name);

        public static string GetContent(Node node)
            => ContentPresenters.ContainsKey(node.Name)
                ? ContentPresenters[node.Name](node)
                : node.Name;

        public static string GetName(Node node)
            => ArtificialNodeTypes.Contains(node.Name)
                ? node.Name
                : node.Attributes[SqlQueryConstants.Name];

        #region ContentFormatters 

        private static string GetTableObjNameContent(Node node)
            => $"{node.Attributes[SqlQueryConstants.SchemaName]}.{node.Attributes[SqlQueryConstants.Name]}";

        private static string GetFullObjNameContent(Node node)
            => $"{node.Attributes[SqlQueryConstants.SchemaName]}.{node.Attributes[SqlQueryConstants.Name]}";


        private static string GetObjNameContent(Node node)
            => node.Attributes[SqlQueryConstants.Name];

        private static string GetIndexContent(Node node)
        {

            var name = node.Attributes[SqlQueryConstants.Name];
            var isPrimary = Convert.ToBoolean(node.Attributes[SqlQueryConstants.IsPrimary]);
            var isUnique = Convert.ToBoolean(node.Attributes[SqlQueryConstants.IsUnique])
                ? "Unique"
                : "Non-Unique";
            var type = node.Attributes[SqlQueryConstants.TypeDesc];

            return isPrimary
                ? $"{name}({type})"
                : $"{name} ({isUnique},{type})";
        }

        private static string GetColumnContent(Node node)
        {
            var c = SqlNodeTypeResolver.ResolveColumnNodeType(node, presentMode: true);
            return c;
        }

        private static string GetProcParameter(Node node)
        {
            var name = node.Attributes[SqlQueryConstants.Name];
            var type = node.Attributes[SqlQueryConstants.UserType];
            var isOutput = Convert.ToBoolean(node.Attributes[SqlQueryConstants.IsOutputParam])
                ? "Output"
                : "Input";

            var hasDefault = Convert.ToBoolean(node.Attributes[SqlQueryConstants.HasDefaultValue]);
            var valueByDefault = hasDefault
                ? node.Attributes[SqlQueryConstants.DefaultValue]
                : "No Default";

            return $"{name} ({type}, {isOutput}, {valueByDefault})";
        }
        #endregion

    }
}
