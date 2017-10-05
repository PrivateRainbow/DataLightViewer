using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Types;
using Loader.Helpers;
using Loader.Services.Helpers;
using System.Text;
using System.Windows.Media.Imaging;
using DataLightViewer.Resolvers;
using DataLightViewer.ViewModels;
using System.Text.RegularExpressions;

namespace DataLightViewer.Services
{
    internal static class NodeDataPresenter
    {
        private static readonly StringBuilder ContentBuilder = new StringBuilder();
        private static readonly Dictionary<string, Func<Node, string>> ContentPresenters;

        private static readonly HashSet<DbSchemaObjectType> ExpandableNodeTypes;
        private static readonly HashSet<DbSchemaObjectType> ArtificialNodeTypes;
        private static readonly HashSet<DbSchemaObjectType> WithPredefinedIconsTypes;


        static NodeDataPresenter()
        {
            ContentPresenters = new Dictionary<string, Func<Node, string>>
            {
                {DbSchemaConstants.Server, GetServerName },
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

            ExpandableNodeTypes = new HashSet<DbSchemaObjectType>
            {
                DbSchemaObjectType.Server,
                DbSchemaObjectType.Databases,
                DbSchemaObjectType.Database,
                DbSchemaObjectType.Tables,
                DbSchemaObjectType.Table,
                DbSchemaObjectType.Views,
                DbSchemaObjectType.View,
                DbSchemaObjectType.Procedures,
                DbSchemaObjectType.Procedure,
                DbSchemaObjectType.Parameters,
                DbSchemaObjectType.Columns,
                DbSchemaObjectType.Keys,
                DbSchemaObjectType.Constraints,
                DbSchemaObjectType.Indexes
            };

            ArtificialNodeTypes = new HashSet<DbSchemaObjectType>
            {
                DbSchemaObjectType.Server,
                DbSchemaObjectType.Databases,
                DbSchemaObjectType.Tables,
                DbSchemaObjectType.Views,
                DbSchemaObjectType.Procedures,
                DbSchemaObjectType.Columns,
                DbSchemaObjectType.Constraints,
                DbSchemaObjectType.Keys,
                DbSchemaObjectType.Indexes,
                DbSchemaObjectType.Parameters
            };

            WithPredefinedIconsTypes = new HashSet<DbSchemaObjectType>
            {
                DbSchemaObjectType.Database,
                DbSchemaObjectType.Table,
                DbSchemaObjectType.View,
                DbSchemaObjectType.Procedure,
                DbSchemaObjectType.Key
            };
        }

        public static string GetContent(this Node node)
            => ContentPresenters.ContainsKey(node.Name)
                ? ContentPresenters[node.Name](node)
                : node.Name;

        public static string GetName(this Node node)
            => ArtificialNodeTypes.Contains(node.ResolveDbNodeType())
                ? node.Name
                : node.Attributes[SqlQueryConstants.Name];

        public static bool IsArtificalSchemaNode(this Node node)
            => ArtificialNodeTypes.Contains(node.ResolveDbNodeType());

        public static bool IsExpandable(this Node node)
            => ExpandableNodeTypes.Contains(node.ResolveDbNodeType());

        public static bool CanChildrenBeCreatedWithoutParent(this Node node)
        {
            var parent = node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.View ||
                                                           n.Name == DbSchemaConstants.Procedure);

            return parent == null ? true : false;
        }

        public static BitmapImage GetIcon(this Node node)
        {
            var type = node.ResolveDbNodeType();
            if (WithPredefinedIconsTypes.Contains(type))
                return IconsSelector.SelectIconByDbObjectType(type);

            return IconsSelector.SelectIconByDbObjectType(DbSchemaObjectType.Artificial);
        }

        #region ContentFormatters 

        private static string GetTableObjNameContent(Node node)
            => $"{node.Attributes[SqlQueryConstants.SchemaName]}.{node.Attributes[SqlQueryConstants.Name]}";

        private static string GetFullObjNameContent(Node node)
            => $"{node.Attributes[SqlQueryConstants.SchemaName]}.{node.Attributes[SqlQueryConstants.Name]}";


        private static string GetObjNameContent(Node node)
            => node.Attributes[SqlQueryConstants.Name];

        private static string GetServerName(Node node)
        {
            var server = node.Attributes[SqlQueryConstants.Name];

            if (server.Contains(","))
            {
                var splitted = Regex.Split(server, @",");
                return splitted[0];
            }

            return server;
        }

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
