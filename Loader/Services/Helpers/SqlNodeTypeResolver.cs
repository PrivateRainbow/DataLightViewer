using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Types;
using System.Text;
using System.Text.RegularExpressions;

namespace Loader.Services.Helpers
{

    public static class SqlNodeTypeResolver
    {
        private static readonly Dictionary<string, Func<Node, bool, string>> _nodeTypeFormatters;
        static SqlNodeTypeResolver()
        {
            _nodeTypeFormatters = new Dictionary<string, Func<Node, bool, string>>
            {
                {"char", FormatCharacterType },
                {"varchar", FormatCharacterType },
                {"binary", FormatCharacterType },
                {"nchar", FormatCharacterType },
                {"nvarchar", FormatCharacterType },

                {"decimal", FormatPrecisionType },
                {"numeric", FormatPrecisionType }            
            };

        }

        public static string ResolveColumnNodeType(Node node, bool presentMode = false)
        {
            if (node == null)
                throw new ArgumentException($"{node}");
            if (node.Name != DbSchemaConstants.Column)
                throw new ArgumentException($"{node} was not expected as a param!");

            var type = node.Attributes[SqlQueryConstants.UserType];

            return _nodeTypeFormatters.ContainsKey(type) 
                ? _nodeTypeFormatters[type](node, presentMode) 
                : FormatCommonType(node, presentMode);
        }

        #region Formatters

        private static string FormatCharacterType(Node sourceNode, bool presentMode = false)
        {
            var name = sourceNode.Attributes[SqlQueryConstants.Name];

            var type = sourceNode.Attributes[SqlQueryConstants.UserType];
            var length = sourceNode.Attributes[SqlQueryConstants.MaxLength];
            var nullable = !Convert.ToBoolean(sourceNode.Attributes[SqlQueryConstants.IsNullable])
                ? "NOT NULL"
                : "NULL";

            if (length == "-1")
                length = "MAX";

            if (presentMode == true)
                return $"{name} ({type}({length}), {nullable})".TrimEnd(' ');

            return $"[{name}] {type}({length}) {nullable}".TrimEnd(' ');
        }

        private static string FormatPrecisionType(Node sourceNode, bool presentMode = false)
        {
            var name = sourceNode.Attributes[SqlQueryConstants.Name];
            var type = sourceNode.Attributes[SqlQueryConstants.UserType];
            var precision = sourceNode.Attributes[SqlQueryConstants.Precision];
            var scale = sourceNode.Attributes[SqlQueryConstants.Scale];

            var nullable = !Convert.ToBoolean(sourceNode.Attributes[SqlQueryConstants.IsNullable])
                ? "NOT NULL"
                : "NULL";

            if (presentMode == true)
                return $"{name} ({type}({precision},{scale}), {nullable})".TrimEnd(' ');

            return $"[{name}] {type}({precision},{scale}) {nullable}".TrimEnd(' ');
        }

        private static string FormatCommonType(Node sourceNode, bool presentMode = false)
        {
            var name = sourceNode.Attributes[SqlQueryConstants.Name];
            var type = sourceNode.Attributes[SqlQueryConstants.UserType];

            var nullable = !Convert.ToBoolean(sourceNode.Attributes[SqlQueryConstants.IsNullable])
                ? "NOT NULL"
                : "NULL";

            var identity = Convert.ToBoolean(sourceNode.Attributes[SqlQueryConstants.IsIdentity])
                ? "IDENTITY(1,1)"
                : string.Empty;

            if (presentMode == true)
                return $"{name} ({type}, {nullable})";

            return $"[{name}] {type} {nullable} {identity} ".TrimEnd(' ');            
        }
        #endregion

    }
}
