using System;
using System.Text;
using System.Linq;
using Loader.Components;
using Loader.Helpers;
using Loader.Types;
using Loader.Services.Helpers;

namespace Loader.Services.Factories
{
    internal class TransactSqlNodeBuildFactory : BaseSqlNodeBuildFactory
    {
        public override string BuildDatabase(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject(DatabaseHeaderLiteral, node.Name));
            var pattern = $"CREATE DATABASE {node.Name};";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);
            scriptBuilder.AppendLine($"{UseLiteral} {node.Name};");

            return scriptBuilder.ToString();
        }
        public override string BuildTable(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Table)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var scriptBuilder = new StringBuilder();

            var tableName = GetTableName(node);
            var pattern = $"CREATE TABLE {tableName} (";

            if(!node.HasChildren())
            {
                scriptBuilder.AppendLine(pattern);
                scriptBuilder.AppendLine(");");
                scriptBuilder.AppendLine(GoLiteral);
                return scriptBuilder.ToString();
            }

            var columns = node.Children.Find(c => c.Name == DbSchemaConstants.Columns).Children;
            var keyHeaderNode = node.Children.Find(k => k.Name == DbSchemaConstants.Keys);
            var foreignKeys = keyHeaderNode.Children?.Where(fk => fk.Attributes[SqlQueryConstants.Type].TrimEnd(' ') == DbSchemaConstants.ForeignKeyTypeLiteral).Select(pk => pk);
            var constraints = node.Children.Find(c => c.Name == DbSchemaConstants.Constraints).Children;
            var indexes = node.Children.Find(i => i.Name == DbSchemaConstants.Indexes).Children;

            scriptBuilder.AppendLine(GetCaptionForDbObject(TableHeaderLiteral, tableName));
            scriptBuilder.AppendLine(pattern);

            if (columns.Any())
            {
                var cols = columns.ToArray();
                var count = cols.GetLength(0);

                for (var i = 0; i < count; i++)
                    if (i != count - 1)
                        scriptBuilder.AppendLine(BuildColumn(cols[i], creationInsideParentTable: true) + ',');
                    else
                        scriptBuilder.AppendLine(BuildColumn(cols[i], creationInsideParentTable: true));
            }

            scriptBuilder.AppendLine(");");
            scriptBuilder.AppendLine(GoLiteral);
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine();

            if(keyHeaderNode.HasChildren())
                scriptBuilder.AppendLine(BuildKey(keyHeaderNode));

            foreach (var fk in foreignKeys)
                scriptBuilder.AppendLine(BuildForeignKey(fk));

            foreach (var c in constraints)
                scriptBuilder.AppendLine(BuildConstraint(c));

            foreach (var i in indexes)
            {
                var index = BuildIndex(i);
                if (!string.IsNullOrEmpty(index))
                    scriptBuilder.AppendLine(index);
            }

            return scriptBuilder.ToString();
        }
        public override string BuildView(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");
            if (node.Name != DbSchemaConstants.View)
                throw new InvalidOperationException($" Such {node} was not expected!");
            if (!node.HasValue())
                throw new InvalidOperationException($" Such {node} must have a Value!");

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject(ViewHeaderLiteral));
            scriptBuilder.AppendLine(node.Value);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        public override string BuildProcedure(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");
            if (node.Name != DbSchemaConstants.Procedure)
                throw new InvalidOperationException($" Such {node} was not expected!");
            if (!node.HasValue())
                throw new InvalidOperationException($" Such {node} must have a Value!");

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject(ProcedureHeaderLiteral));
            scriptBuilder.AppendLine(node.Value);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        public override string BuildColumn(Node node, bool creationInsideParentTable = false)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Column)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var script = string.Empty;
            if (creationInsideParentTable)
                script = SqlNodeTypeResolver.ResolveColumnNodeType(node);
            else
            {
                var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
                var scriptBuilder = new StringBuilder();
                scriptBuilder.AppendLine(GetCaptionForDbObject(ColumnHeaderLiteral));
                var column = SqlNodeTypeResolver.ResolveColumnNodeType(node);
                var pattern = $"ALTER TABLE {table} " +
                              $"ADD {column}";
                scriptBuilder.AppendLine(pattern);
                script = scriptBuilder.ToString();
            }

            return script;
        }

        public override string BuildIndex(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Index)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
            var name = node.Attributes[SqlQueryConstants.Name];
            var column = node.Attributes[SqlQueryConstants.ColumnName];
            var clustered = node.Attributes[SqlQueryConstants.TypeDesc];

            var isUnique = Convert.ToBoolean(node.Attributes[SqlQueryConstants.IsUnique])
                ? "UNIQUE"
                : string.Empty;

            var isPrimary = Convert.ToBoolean(node.Attributes[SqlQueryConstants.IsPrimary]);
            if (isPrimary)
                return string.Empty;

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject(IndexHeaderLiteral));
            var pattern = $"CREATE {isUnique} INDEX [{name}] " +
                          $"ON {table} ([{column}]);";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        public override string BuildProcParameter(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.ProcParameter)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var parent = node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Procedure);
            return parent.Attributes[SqlQueryConstants.Definition];
        }
        public override string BuildKey(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Key && node.Name != DbSchemaConstants.Keys)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes() && node.Name != DbSchemaConstants.Keys)
                throw new InvalidOperationException($"{node} has no attributes!");

            var keyScript = string.Empty;

            if (node.Name == DbSchemaConstants.Keys)
                keyScript = BuildPrimaryKey(node);
            else
            {
                if (node.Attributes[SqlQueryConstants.Type].Trim() == DbSchemaConstants.PrimaryKeyTypeLiteral)
                    keyScript = BuildPrimaryKey(node);
                if (node.Attributes[SqlQueryConstants.Type].Trim() == DbSchemaConstants.ForeignKeyTypeLiteral)
                    keyScript = BuildForeignKey(node);
            }
            return keyScript;
        }
        public override string BuildConstraint(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Constraint)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            if (node.Attributes[SqlQueryConstants.Type].Trim() == DbSchemaConstants.DefaultConstraintTypeLiteral)
                return BuildDefaultConstraint(node);

            if (node.Attributes[SqlQueryConstants.Type].Trim() == DbSchemaConstants.CheckedConstraintTypeLiteral)
                return BuildCheckedConstraint(node);

            return BuildUniqueConstraint(node);
        }

        protected override string BuildPrimaryKey(Node node)
        {
            string fields;
            string name;
            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));


            if (node.Name == DbSchemaConstants.Key)
            {
                fields = node.Attributes[SqlQueryConstants.ColumnName];
                name = node.Attributes[SqlQueryConstants.Name];
            }
            else
            {
                var keys = node.Children.Where(n => n.Attributes[SqlQueryConstants.Type] == DbSchemaConstants.PrimaryKeyTypeLiteral).Select(n => n);

                var keyFields = keys.Select(n => n.Attributes[SqlQueryConstants.ColumnName]).ToArray();
                fields = string.Join(",", keyFields);

                var key = keys.First();
                name = key.Attributes[SqlQueryConstants.Name];
            }

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject("PRIMARY KEY"));
            var pattern = $"ALTER TABLE {table} " +
                          $"ADD CONSTRAINT {name} " +
                          $"PRIMARY KEY({fields})";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        protected override string BuildForeignKey(Node node)
        {
            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var name = node.Attributes[SqlQueryConstants.Name];
            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
            var parentTable = string.IsNullOrEmpty(table)
                ? node.Attributes[SqlQueryConstants.FkParentTable]
                : table;

            var parentColumn = node.Attributes[SqlQueryConstants.FkParentColumn];
            var refTable = node.Attributes[SqlQueryConstants.FkReferentialTable];
            var refColumn = node.Attributes[SqlQueryConstants.FkReferentialColumn];
            var onDeleteOption = node.Attributes[SqlQueryConstants.FkOnDeleteAction];
            var onUpdateOption = node.Attributes[SqlQueryConstants.FkOnUpdateAction];

            var onDelete = onDeleteOption == "NO_ACTION" ? string.Empty : $"ON DELETE {onDeleteOption}";
            var onUpdate = onUpdateOption == "NO_ACTION" ? string.Empty : $"ON UPDATE {onDeleteOption}";

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject(ForeignKeyHeaderLiteral));
            var pattern = $"ALTER TABLE {parentTable} ADD CONSTRAINT [{name}] " +
                          $"FOREIGN KEY ([{parentColumn}]) REFERENCES [{refTable}]([{refColumn}]) " +
                          $"{onDelete} " +
                          $"{onUpdate} ";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        protected override string BuildDefaultConstraint(Node node)
        {
            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
            var name = node.Attributes[SqlQueryConstants.Name];
            var column = node.Attributes[SqlQueryConstants.ColumnName];
            var definition = node.Attributes[SqlQueryConstants.Definition];

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject("DEFAULT CONSTRAINT"));
            var pattern = $"ALTER TABLE {table} " +
                          $"ADD CONSTRAINT [{name}] " +
                          $"DEFAULT {definition} FOR [{column}]";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        protected override string BuildCheckedConstraint(Node node)
        {
            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
            var name = node.Attributes[SqlQueryConstants.ColumnName];
            var definition = node.Attributes[SqlQueryConstants.Definition];

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject("CHECKED CONSTRAINT"));
            var pattern = $"ALTER TABLE {table} ADD CONSTRAINT {name} " +
                          $"CHECK {definition};";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
        protected override string BuildUniqueConstraint(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            if (node.Name != DbSchemaConstants.Constraint)
                throw new InvalidOperationException($" Such {node} was not expected!");

            if (!node.HasAttributes())
                throw new InvalidOperationException($"{node} has no attributes!");

            var table = GetTableName(node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Table));
            var name = node.Attributes[SqlQueryConstants.Name];
            var column = node.Attributes[SqlQueryConstants.ColumnName];
            var ignore = Convert.ToBoolean(node.Attributes[SqlQueryConstants.UcIgnoreDupKey])
                ? "WITH IGNORE_DUP_KEY"
                : string.Empty;

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(GetCaptionForDbObject("UNIQUE CONSTRAINT"));
            var pattern = $"ALTER TABLE {table} " +
                          $"ADD CONSTRAINT {name} " +
                          $"UNIQUE ({column}) {ignore}";
            scriptBuilder.AppendLine(pattern);
            scriptBuilder.AppendLine(GoLiteral);

            return scriptBuilder.ToString();
        }
    }
}
