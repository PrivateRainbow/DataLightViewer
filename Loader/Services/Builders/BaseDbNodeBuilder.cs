using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Loader.Components;
using Loader.Helpers;
using Loader.Services.Helpers;
using Loader.Services.Types;
using Loader.Types;

namespace Loader.Services.Builders
{
    public abstract class BaseDbNodeBuilder
    {
        #region Data

        private static readonly HashSet<string> _dDbNodesWithValue;

        protected SqlConnection sqlConnection;
        protected SqlCommandHelper sqlCommandHelper;

        #endregion

        #region Init

        static BaseDbNodeBuilder()
        {
            _dDbNodesWithValue = new HashSet<string>
            {
                DbSchemaConstants.View,
                DbSchemaConstants.Procedure
            };
        }

        protected BaseDbNodeBuilder() { }
        protected BaseDbNodeBuilder(string connectionString)
        {
            InitializeBuilder(connectionString);
        }

        public void InitializeBuilder(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)}");

            sqlConnection = new SqlConnection(connectionString);
            sqlCommandHelper = new SqlCommandHelper(sqlConnection);
        }

        public void SetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)}");

            if (!ReferenceEquals(sqlConnection.ConnectionString, connectionString))
                sqlConnection.ConnectionString = connectionString;
        }

        #endregion

        #region Abstract

        public abstract List<Node> MakeNode(BuildContext context);

        #endregion

        #region Base

        #region AsCollection

        protected virtual List<Node> MakeDatabases()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForDatabases()).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.Database, reader);
        }

        protected virtual List<Node> MakeTables()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForTables()).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.Table, reader);
        }
        protected virtual List<Node> MakeViews()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForViews()).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.View, reader);
        }
        protected virtual List<Node> MakeProcedures()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForProcedures()).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.Procedure, reader);
        }

        protected virtual List<Node> MakeColumns(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForColumns(), context.Id).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.Column, reader);
        }
        protected virtual List<Node> MakeKeys(SchemaNavigationContext context)
        {
            var keys = new List<Node>();

            var primaryKeys = MakeNodeCollection(DbSchemaConstants.Key,
                sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForPrimaryKeys(), context.Id).ExecuteReader());

            var foreignKeys = MakeNodeCollection(DbSchemaConstants.Key,
                sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForForeignKeys(), context.Id).ExecuteReader());

            keys.AddRange(primaryKeys);
            keys.AddRange(foreignKeys);

            return keys;
        }
        protected virtual List<Node> MakeConstraints(SchemaNavigationContext context)
        {
            var constraints = new List<Node>();

            var defaultConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForDefaultConstraints(), context.Id).ExecuteReader());

            var checkedConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForCheckedConstraints(), context.Id).ExecuteReader());

            var uniqueConstraints = MakeNodeCollection(DbSchemaConstants.Constraint,
                sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForUniqueConstraints(), context.Id).ExecuteReader());

            constraints.AddRange(defaultConstraints);
            constraints.AddRange(checkedConstraints);
            constraints.AddRange(uniqueConstraints);

            return constraints;
        }
        protected virtual List<Node> MakeIndexes(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForIndexes(), context.Id).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.Index, reader);
        }
        protected virtual List<Node> MakeProcParams(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParam(SqlQueries.GetQueryForProcParameters(), context.Id).ExecuteReader();
            return MakeNodeCollection(DbSchemaConstants.ProcParameter, reader);
        }

        #endregion

        #region Bundles

        protected virtual List<Node> MakeBundleForDatabase()
        {
            var tables = new Node(DbSchemaConstants.Tables);
            var views = new Node(DbSchemaConstants.Views);
            var procedures = new Node(DbSchemaConstants.Procedures);

            tables.Children.AddRange(MakeTables());
            views.Children.AddRange(MakeViews());
            procedures.Children.AddRange(MakeProcedures());

            var bunch = new List<Node> { tables, views, procedures };
            return bunch;
        }

        protected virtual List<Node> MakeBundleForTable(SchemaNavigationContext context)
        {
            var columnsHeader = MakeNode(DbSchemaConstants.Columns);
            var keysHeader = MakeNode(DbSchemaConstants.Keys);
            var constraintsHeader = MakeNode(DbSchemaConstants.Constraints);
            var indexesHeader = MakeNode(DbSchemaConstants.Indexes);

            var columns = BulkDataFetchingHandler(MakeColumns, context);
            var keys = BulkDataFetchingHandler(MakeKeys, context);
            var constraints = BulkDataFetchingHandler(MakeConstraints, context);
            var indexes = BulkDataFetchingHandler(MakeIndexes, context);

            columns.ForEach(cols => columnsHeader.Add(cols));
            keys.ForEach(k => keysHeader.Add(k));
            constraints.ForEach(c => constraintsHeader.Add(c));
            indexes.ForEach(i => indexesHeader.Add(i));

            var bunch = new List<Node> { columnsHeader, keysHeader, constraintsHeader, indexesHeader };
            return bunch;
        }
        protected virtual List<Node> MakeBundleForView(SchemaNavigationContext context)
        {
            var columnsHeader = MakeNode(DbSchemaConstants.Columns);
            var indexesHeader = MakeNode(DbSchemaConstants.Indexes);

            var columns = BulkDataFetchingHandler(MakeColumns, context);
            var indexes = BulkDataFetchingHandler(MakeIndexes, context);

            columns.ForEach(cols => columnsHeader.Add(cols));
            indexes.ForEach(i => indexesHeader.Add(i));

            var bunch = new List<Node> { columnsHeader, indexesHeader };
            return bunch;

        }
        protected virtual List<Node> MakeBundleForProcedure(SchemaNavigationContext context)
        {
            var procParametersHeader = MakeNode(DbSchemaConstants.ProcParameters);
            var procParams = BulkDataFetchingHandler(MakeProcParams, context);

            procParams.ForEach(pp => procParametersHeader.Add(pp));

            var bunch = new List<Node> { procParametersHeader };
            return bunch;
        }

        #endregion

        #endregion

        #region Helpers

        protected List<Node> LazyDataFetchingHandler(Func<List<Node>> queryFetchingAction)
        {
            using (sqlConnection)
            {
                sqlConnection.Open();
                return queryFetchingAction();
            }
        }
        protected List<Node> LazyDataFetchingHandler(Func<SchemaNavigationContext, List<Node>> queryFetchingAction, SchemaNavigationContext context)
        {
            if (context == null)
                throw new ArgumentException($"{nameof(context)}");

            using (sqlConnection)
            {
                sqlConnection.Open();
                return queryFetchingAction(context);
            }
        }

        protected List<Node> BulkDataFetchingHandler(Func<List<Node>> queryFetchingAction) => queryFetchingAction();
        protected List<Node> BulkDataFetchingHandler(Func<SchemaNavigationContext, List<Node>> queryFetchingAction, SchemaNavigationContext context)
        {
            if (context == null)
                throw new ArgumentException($"{nameof(context)}");

            return queryFetchingAction(context);
        }

        protected static Node MakeNode(string name) => new Node(name);
        protected static List<Node> MakeNodeCollection(string nodeName, DbDataReader dataReader)
        {
            if (string.IsNullOrEmpty(nodeName)) throw new ArgumentException($"{nameof(nodeName)}");
            if (dataReader == null) throw new ArgumentException($"{dataReader}");

            var count = dataReader.FieldCount;
            var nodes = new List<Node>(count);
            var startFrom = 0;

            using (dataReader)
            {
                if (!dataReader.HasRows) return nodes;

                var objects = new object[count];
                while (dataReader.Read())
                {
                    dataReader.GetValues(objects);

                    var subNode = new Node(nodeName);
                    if (_dDbNodesWithValue.Contains(nodeName))
                    {
                        subNode.Value = dataReader.GetValue(0).ToString();
                        subNode.Attributes = new Dictionary<string, string>(count - 1);
                        startFrom = 1;
                    }
                    else
                        subNode.Attributes = new Dictionary<string, string>(count);

                    for (var i = startFrom; i < count; i++)
                    {
                        var key = dataReader.GetName(i);
                        var value = dataReader.IsDBNull(i)
                            ? dataReader.GetFieldType(i).GetValueByDefault().ToString()
                            : dataReader.GetValue(i).ToString();

                        subNode.Attributes.Add(key, value);
                    }

                    nodes.Add(subNode);
                }
            }
            return nodes;
        }

        #endregion

    }
}
