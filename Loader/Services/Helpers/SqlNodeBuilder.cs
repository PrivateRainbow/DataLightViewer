using System;
using System.Collections.Generic;
using System.Text;
using Loader.Components;
using Loader.Services.Factories;
using Loader.Types;
using Loader.Helpers;

namespace Loader.Services.Helpers
{
    public class SqlNodeBuilder
    {
        private readonly ISqlNodeBuildFactory _buildFactory;
        private readonly HashSet<string> _headerNodes;

        public SqlNodeBuilder(ISqlNodeBuildFactory buildFactory)
        {
            _buildFactory = buildFactory ?? throw new ArgumentNullException($"{nameof(buildFactory)}");

            _headerNodes = new HashSet<string>()
            {
                DbSchemaConstants.Tables,
                DbSchemaConstants.Views,
                DbSchemaConstants.Procedures,
                DbSchemaConstants.Columns,
                DbSchemaConstants.Keys,
                DbSchemaConstants.Constraints,
                DbSchemaConstants.Indexes,
                DbSchemaConstants.ProcParameters
            };
        }

        public string BuildScript(Node node)
        {
            if( node == null)
                throw new ArgumentException($"{nameof(node)}");

            var scriptBuilder = new StringBuilder();

            if (IsRootNode(node))
            {
                var database = _buildFactory.BuildDatabase(node);
                var tables = node.Children.Find(t => t.Name == DbSchemaConstants.Tables);
                var views = node.Children.Find(t => t.Name == DbSchemaConstants.Views);
                var procedures = node.Children.Find(t => t.Name == DbSchemaConstants.Procedures);

                scriptBuilder.AppendLine(database);
                scriptBuilder.AppendLine(BuildHeaderNode(tables));
                scriptBuilder.AppendLine(BuildHeaderNode(views));
                scriptBuilder.AppendLine(BuildHeaderNode(procedures));
            }

            if (IsHeaderNode(node))
                scriptBuilder.AppendLine(BuildHeaderNode(node));
            else
                scriptBuilder.AppendLine(BuildCommonNode(node));

            return scriptBuilder.ToString();
        }

        private string CustomDatabaseNodeConstruction(Node node, bool withChildren = false)
        {
            var scriptBuilder = new StringBuilder();

            var database = _buildFactory.BuildDatabase(node);
            scriptBuilder.AppendLine(database);

            if (withChildren)
            {
                var tables = node.Children.Find(t => t.Name == DbSchemaConstants.Tables);
                var views = node.Children.Find(t => t.Name == DbSchemaConstants.Views);
                var procedures = node.Children.Find(t => t.Name == DbSchemaConstants.Procedures);

                scriptBuilder.AppendLine(BuildHeaderNode(tables));
                scriptBuilder.AppendLine(BuildHeaderNode(views));
                scriptBuilder.AppendLine(BuildHeaderNode(procedures));
            }

            return scriptBuilder.ToString();
        }

        private string BuildHeaderNode(Node node)
        {
            var script = string.Empty;

            if (!node.HasChildren())
                return script;

            var builder = new StringBuilder();
            builder.AppendLine(Environment.NewLine);

            try
            {
                foreach (var n in node.Children)
                {
                    var data = BuildCommonNode(n);
                    builder.AppendLine(data);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return builder.ToString();
        }

        private string BuildCommonNode(Node node)
        {
            var type = node.Name;
            String script;

            switch (type)
            {
                case DbSchemaConstants.Table:
                    script = _buildFactory.BuildTable(node);
                    break;

                case DbSchemaConstants.View:
                    script = _buildFactory.BuildView(node);
                    break;

                case DbSchemaConstants.Procedure:
                    script = _buildFactory.BuildProcedure(node);
                    break;

                case DbSchemaConstants.Column:
                    script = _buildFactory.BuildColumn(node);
                    break;

                case DbSchemaConstants.Key:
                    script = _buildFactory.BuildKey(node);
                    break;

                case DbSchemaConstants.Constraint:
                    script = _buildFactory.BuildConstraint(node);
                    break;

                case DbSchemaConstants.Index:
                    script = _buildFactory.BuildIndex(node);
                    break;

                case DbSchemaConstants.ProcParameter:
                    script = _buildFactory.BuildProcParameter(node);
                    break;

                default:
                    script = string.Empty;
                    break;
            }

            return script;
        }

        private bool IsHeaderNode(Node node) => _headerNodes.Contains(node.Name);
        private bool IsRootNode(Node node) => node.Name == DbSchemaConstants.Database;
    }
}
