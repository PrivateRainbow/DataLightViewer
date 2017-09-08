using System.Collections.Generic;
using Loader.Components;
using Loader.Services.Types;
using Loader.Helpers;
using Loader.Types;
using System;

namespace Loader.Services.Builders
{
    public class BunchLazyDbNodeBuilder : LazyDbNodeBuilder
    {
        #region Init

        public BunchLazyDbNodeBuilder() { }
        public BunchLazyDbNodeBuilder(string connectionString) : base(connectionString) { }

        #endregion

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
                    case DbSchemaObjectType.Table: return GetTableBundle(node);

                    case DbSchemaObjectType.Views: return MakeViews();
                    case DbSchemaObjectType.View: return GetViewBundle(node);

                    case DbSchemaObjectType.Procedures: return MakeProcedures();
                    case DbSchemaObjectType.Procedure: return GetProcedureBundle(node);

                    case DbSchemaObjectType.Columns:
                        return LazyDataFetchingHandler(MakeColumns, node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Keys:
                        return LazyDataFetchingHandler(MakeKeys, node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Constraints:
                        return LazyDataFetchingHandler(MakeConstraints, node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Indexes:
                        return LazyDataFetchingHandler(MakeIndexes, node.GetParentId(node.Parent.Name));

                    case DbSchemaObjectType.Parameters:
                        return LazyDataFetchingHandler(MakeProcParams, node.GetParentId(node.Parent.Name));

                    default: return new List<Node>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region Overriden

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
        
        #endregion


        #region Helpers

        private List<Node> LazyDataFetchingHandler(Func<string, List<Node>> queryFetchingAction, params string[] param)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();
                return queryFetchingAction(param[0]);
            }
        }
        private List<Node> BulkDataFetchingHandler(Func<string, List<Node>> queryFetchingAction, params string[] param) => queryFetchingAction(param[0]);

        private static Node GetNode(string name) => new Node(name);

        protected List<Node> GetTableBundle(Node node)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var columnsHeader = GetNode(DbSchemaConstants.Columns);
                var keysHeader = GetNode(DbSchemaConstants.Keys);
                var constraintsHeader = GetNode(DbSchemaConstants.Constraints);
                var indexesHeader = GetNode(DbSchemaConstants.Indexes);

                var columns = BulkDataFetchingHandler(MakeColumns, node.GetId());
                var keys = BulkDataFetchingHandler(MakeKeys, node.GetId());
                var constraints = BulkDataFetchingHandler(MakeConstraints, node.GetId());
                var indexes = BulkDataFetchingHandler(MakeIndexes, node.GetId());

                columns.ForEach(cols => columnsHeader.Add(cols));
                keys.ForEach(k => keysHeader.Add(k));
                constraints.ForEach(c => constraintsHeader.Add(c));
                indexes.ForEach(i => indexesHeader.Add(i));

                var bunch = new List<Node> { columnsHeader, keysHeader, constraintsHeader, indexesHeader };
                return bunch;
            }
        }
        protected List<Node> GetViewBundle(Node node)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var columnsHeader = GetNode(DbSchemaConstants.Columns);
                var indexesHeader = GetNode(DbSchemaConstants.Indexes);

                var columns = BulkDataFetchingHandler(MakeColumns, node.GetId());
                var indexes = BulkDataFetchingHandler(MakeIndexes,node.GetId());

                columns.ForEach(cols => columnsHeader.Add(cols));
                indexes.ForEach(i => indexesHeader.Add(i));

                var bunch = new List<Node> { columnsHeader, indexesHeader };
                return bunch;
            }
        }
        protected List<Node> GetProcedureBundle(Node node)
        {
            using (SqlConnection)
            {
                SqlConnection.Open();

                var procParametersHeader = GetNode(DbSchemaConstants.ProcParameters);
                var procParams = BulkDataFetchingHandler(MakeProcParams, node.GetId());

                procParams.ForEach(pp => procParametersHeader.Add(pp));

                var bunch = new List<Node> { procParametersHeader };
                return bunch;
            }
        }

        #endregion
    }
}
