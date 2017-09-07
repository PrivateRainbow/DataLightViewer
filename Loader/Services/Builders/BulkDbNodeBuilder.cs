using Loader.Services.Types;
using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Helpers;
using Loader.Types;


namespace Loader.Services.Builders
{
    public sealed class BulkDbNodeBuilder : BaseDbNodeBuilder
    {
        public BulkDbNodeBuilder() {}
        public BulkDbNodeBuilder(string connectionString) : base(connectionString) { }

        #region Implementation
        protected override List<Node> MakeDatabases()
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var database = new Node(DbSchemaConstants.Database);
                var tables = new Node(DbSchemaConstants.Tables);
                var views = new Node(DbSchemaConstants.Views);
                var procedures = new Node(DbSchemaConstants.Procedures);

                tables.Children.AddRange(MakeTables());
                views.Children.AddRange(MakeViews());
                procedures.Children.AddRange(MakeProcedures());

                database.Add(tables);
                database.Add(views);
                database.Add(procedures);

                return new List<Node> {database};
            }
        }
        protected override List<Node> MakeTables()
        {
            var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForTable()).ExecuteReader();
            var tables = MakeNodeCollection(DbSchemaConstants.Table, reader);

            foreach (var t in tables)
            {
                var id = t.Attributes[SqlQueryConstants.ObjectIdLiteral];

                var columns = new Node(DbSchemaConstants.Columns);
                var keys = new Node(DbSchemaConstants.Keys);
                var constraints = new Node(DbSchemaConstants.Constraints);
                var indexes = new Node(DbSchemaConstants.Indexes);

                columns.Children.AddRange(MakeColumns(id));
                keys.Children.AddRange(MakeKeys(id));
                constraints.Children.AddRange(MakeConstraints(id));
                indexes.Children.AddRange(MakeIndexes(id));

                var subNodes = new[] { columns, keys, constraints, indexes };
                t.Children.AddRange(subNodes);
            }

            return tables;
        }
        protected override List<Node> MakeViews()
        {
            var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForView()).ExecuteReader();
            var views = MakeNodeCollection(DbSchemaConstants.View, reader);

            foreach (var v in views)
            {
                var id = v.Attributes[SqlQueryConstants.ObjectIdLiteral];
                var columns = new Node(DbSchemaConstants.Columns);
                var indexes = new Node(DbSchemaConstants.Indexes);

                columns.Children.AddRange(MakeColumns(id));
                indexes.Children.AddRange(MakeIndexes(id));

                var subNodes = new[] { columns, indexes };
                v.Children.AddRange(subNodes);
            }

            return views;
        }
        protected override List<Node> MakeProcedures()
        {
            var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForProcedure()).ExecuteReader();
            var procedures = MakeNodeCollection(DbSchemaConstants.Procedure, reader);

            foreach (var p in procedures)
            {
                var id = p.Attributes[SqlQueryConstants.ObjectIdLiteral];
                var parameters = new Node(DbSchemaConstants.ProcParameters);

                parameters.Children.AddRange(MakeProcParams(id));

                p.Add(parameters);
            }

            return procedures;
        }
        protected override List<Node> MakeColumns(string objectId)
        {
            return MakeNodeCollection(DbSchemaConstants.Column,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForColumn(), objectId).ExecuteReader());
        }
        protected override List<Node> MakeKeys(string objectId)
        {
            var collection = new List<Node>();

            var primaryKeys = MakeNodeCollection(DbSchemaConstants.Key,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForPrimaryKey(), objectId).ExecuteReader());

            var foreignKeys = MakeNodeCollection(DbSchemaConstants.Key,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForForeignKey(), objectId).ExecuteReader());

            collection.AddRange(primaryKeys);
            collection.AddRange(foreignKeys);

            return collection;
        }
        protected override List<Node> MakeConstraints(string objectId)
        {
            var collection = new List<Node>();

            var defaultConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForDefaultConstraint(), objectId).ExecuteReader());

            var checkedConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForCheckedConstraint(), objectId).ExecuteReader());

            var uniqueConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForUniqueConstraint(), objectId).ExecuteReader());

            collection.AddRange(defaultConstraints);
            collection.AddRange(checkedConstraints);
            collection.AddRange(uniqueConstraints);

            return collection;
        }
        protected override List<Node> MakeIndexes(string objectId)
        {
            return MakeNodeCollection(DbSchemaConstants.Index,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForIndex(), objectId).ExecuteReader());
        }
        protected override List<Node> MakeProcParams(string objectId)
        {
            return MakeNodeCollection(DbSchemaConstants.ProcParameter,
                SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForProcParameter(), objectId).ExecuteReader());
        }

        public override List<Node> MakeNode(BuildContext context)
        {
            var type = context.Node.ResolveDbNodeType();

            if (type != DbSchemaObjectType.Database)
                throw new ArgumentException($" Such {type} was not expected! Use DbSchemaObjectType.Database .");

            return MakeDatabases();
        }

        #endregion

    }
}
