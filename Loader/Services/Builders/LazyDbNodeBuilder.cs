using System.Collections.Generic;
using Loader.Components;
using Loader.Helpers;
using Loader.Services.Types;
using Loader.Types;
using System;

namespace Loader.Services.Builders
{
    public sealed class LazyDbNodeBuilder : BaseDbNodeBuilder
    {
        #region Init

        public LazyDbNodeBuilder() { }
        public LazyDbNodeBuilder(string connectionString) : base(connectionString) { }

        #endregion

        #region Implementation

        protected override List<Node> MakeDatabases()
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForDatabases()).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.Database, reader);
            }

        }
        protected override List<Node> MakeTables()
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForTable()).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.Table, reader);
            }
        }
        protected override List<Node> MakeViews()
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForView()).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.View, reader);
            }
        }
        protected override List<Node> MakeProcedures()
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommand(SqlQueries.GetQueryForProcedure()).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.Procedure, reader);
            }
        }
        protected override List<Node> MakeColumns(string objectId)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForColumn(), objectId).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.Column, reader);
            }
        }
        protected override List<Node> MakeKeys(string objectId)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var collection = new List<Node>();

                var primaryKeys = MakeNodeCollection(DbSchemaConstants.Key,
                    SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForPrimaryKey(), objectId).ExecuteReader());

                var foreignKeys = MakeNodeCollection(DbSchemaConstants.Key,
                    SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForForeignKey(), objectId).ExecuteReader());

                collection.AddRange(primaryKeys);
                collection.AddRange(foreignKeys);

                return collection;
            }
        }
        protected override List<Node> MakeConstraints(string objectId)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

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
        }
        protected override List<Node> MakeIndexes(string objectId)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForIndex(), objectId).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.Index, reader);
            }
        }
        protected override List<Node> MakeProcParams(string objectId)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                var reader = SqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForProcParameter(), objectId).ExecuteReader();
                return MakeNodeCollection(DbSchemaConstants.ProcParameter, reader);
            }
        }

        public override List<Node> MakeNode(BuildContext context)
        {
            var node = context.Node;
            var type = node.ResolveDbNodeType();

            SetConnection(context.Connection);

            try
            {
                switch (type)
                {
                    case DbSchemaObjectType.Server: return MakeDatabases();
                    case DbSchemaObjectType.Database: return GetDatabaseBundle();

                    case DbSchemaObjectType.Tables: return MakeTables();
                    case DbSchemaObjectType.Table: return GetTableBundle();

                    case DbSchemaObjectType.Views: return MakeViews();
                    case DbSchemaObjectType.View: return GetViewBundle();

                    case DbSchemaObjectType.Procedures: return MakeProcedures();
                    case DbSchemaObjectType.Procedure: return GetProcedureBundle();

                    case DbSchemaObjectType.Columns:
                        return MakeColumns(node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Keys:
                        return MakeKeys(node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Constraints:
                        return MakeConstraints(node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Indexes:
                        return MakeIndexes(node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Parameters:
                        return MakeProcParams(node.GetParentId(node.Parent.Name));

                    default: return new List<Node>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Helpers

        private static List<Node> GetDatabaseBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Tables),
                new Node(DbSchemaConstants.Views),
                new Node(DbSchemaConstants.Procedures)
            };
        }

        private static List<Node> GetTableBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Columns),
                new Node(DbSchemaConstants.Keys),
                new Node(DbSchemaConstants.Constraints),
                new Node(DbSchemaConstants.Indexes)
            };
        }

        private static List<Node> GetViewBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Columns),
                new Node(DbSchemaConstants.Indexes)
            };
        }

        private static List<Node> GetProcedureBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.ProcParameters)
            };
        }

        #endregion
    }
}
