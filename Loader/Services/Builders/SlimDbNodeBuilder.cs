using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Services.Types;
using Loader.Types;
using System.Data.SqlClient;
using Loader.Helpers;
using Pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Loader.Services.Builders
{
    public class SlimDbNodeBuilder : BaseDbNodeBuilder
    {
        /// <summary>
        /// Return a collection with single instance of db object if it still exists on remote server.
        /// Use <code>MakeNode(context).Single();</code> in order to check that requested db object is still alive on server
        /// </summary>
        /// <returns></returns>
        public override List<Node> MakeNode(BuildContext context)
        {
            var node = context.Node;
            var type = node.ResolveDbNodeType();

            SetConnection(context.Connection);

            try
            {
                switch (type)
                {
                    case DbSchemaObjectType.Database:
                        return LazyDataFetchingHandler(MakeDatabases,node.GetNavigationContext(NavigationType.Self));

                    case DbSchemaObjectType.Table:
                        return LazyDataFetchingHandler(MakeTables, node.GetNavigationContext(NavigationType.Self));

                    case DbSchemaObjectType.View:
                        return LazyDataFetchingHandler(MakeViews, node.GetNavigationContext(NavigationType.Self));

                    case DbSchemaObjectType.Procedure:
                        return LazyDataFetchingHandler(MakeProcedures, node.GetNavigationContext(NavigationType.Self));

                    case DbSchemaObjectType.Column:
                        var colNavContext = node.GetNavigationContext(NavigationType.Self);
                        colNavContext.With(new Pair(SqlQueryConstants.ColumnId, node.Attributes[SqlQueryConstants.ColumnId]));

                        return LazyDataFetchingHandler(MakeColumns, colNavContext);

                    case DbSchemaObjectType.Key:
                        var keyNavContext = node.GetNavigationContext(NavigationType.WithParent);
                        keyNavContext.With(new Pair(SqlQueryConstants.Type, node.Attributes[SqlQueryConstants.Type]));

                        return LazyDataFetchingHandler(MakeKeys, keyNavContext);

                    case DbSchemaObjectType.Constraint:
                        var constraintNavContext = node.GetNavigationContext(NavigationType.WithParent);
                        constraintNavContext.With(new Pair(SqlQueryConstants.Type, node.Attributes[SqlQueryConstants.Type]));

                        if (node.Attributes[SqlQueryConstants.Type] == DbSchemaConstants.UniqueConstraintTypeLiteral)
                            constraintNavContext.With(new Pair(SqlQueryConstants.IndexColumnId, node.Attributes[SqlQueryConstants.IndexColumnId]));

                        return LazyDataFetchingHandler(MakeConstraints, constraintNavContext);

                    case DbSchemaObjectType.Index:
                        var indexNavContext = node.GetNavigationContext(NavigationType.Self);
                        indexNavContext.With(new Pair(SqlQueryConstants.IndexId, node.Attributes[SqlQueryConstants.IndexId]));

                        return LazyDataFetchingHandler(MakeIndexes, indexNavContext);

                    case DbSchemaObjectType.Parameter:
                        var paramNavContext = node.GetNavigationContext(NavigationType.Self);
                        paramNavContext.With(new Pair(SqlQueryConstants.ParameterId, node.Attributes[SqlQueryConstants.ParameterId]));

                        return LazyDataFetchingHandler(MakeProcParams, paramNavContext);
                    
                    default:
                        throw new ArgumentException($"{nameof(type)}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected virtual List<Node> MakeDatabases(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForDatabase(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id)).ExecuteReader();

            return MakeNodeCollection(DbSchemaConstants.Database, reader);
        }
        protected virtual List<Node> MakeTables(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForTable(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id)).ExecuteReader();

            return MakeNodeCollection(DbSchemaConstants.Table, reader);
        }
        protected virtual List<Node> MakeViews(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForView(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id)).ExecuteReader();

            return MakeNodeCollection(DbSchemaConstants.View, reader);
        }
        protected virtual List<Node> MakeProcedures(SchemaNavigationContext context)
        {
            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForProcedure(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id)).ExecuteReader();

            return MakeNodeCollection(DbSchemaConstants.Procedure, reader);
        }

        #region Overriden

        protected override List<Node> MakeColumns(SchemaNavigationContext context)
        {
            var columnId = context.SchemaAttributes[SqlQueryConstants.ColumnId];

            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForColumn(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id),
                new SqlParameter(SqlQueryConstants.IdentifyParam, columnId)
               ).ExecuteReader();

            var column = MakeNodeCollection(DbSchemaConstants.Column, reader);
            return column;
        }
        protected override List<Node> MakeKeys(SchemaNavigationContext context)
        {
            var type = context.SchemaAttributes[SqlQueryConstants.Type].Trim();

            var query = type == DbSchemaConstants.PrimaryKeyTypeLiteral
                ? SqlQueries.GetQueryForPrimaryKey()
                : SqlQueries.GetQueryForForeignKey();

            var reader = sqlCommandHelper.MakeCommandWithParams(query,
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id),
                new SqlParameter(SqlQueryConstants.ParentIdParam, context.ParentId)
                ).ExecuteReader();

            var key = MakeNodeCollection(DbSchemaConstants.Key, reader);
           


            return key;
        }
        protected override List<Node> MakeConstraints(SchemaNavigationContext context)
        {
            var constraintType = context.SchemaAttributes[SqlQueryConstants.Type].Trim();
            string query = string.Empty;

            var parameters = new List<SqlParameter> { new SqlParameter(SqlQueryConstants.ParentIdParam, context.ParentId) };

            switch (constraintType)
            {
                case DbSchemaConstants.DefaultConstraintTypeLiteral:
                    query = SqlQueries.GetQueryForDefaultConstraint();
                    parameters.Add(new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id));
                    break;

                case DbSchemaConstants.CheckedConstraintTypeLiteral:
                    query = SqlQueries.GetQueryForCheckedConstraint();
                    parameters.Add(new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id));
                    break;

                case DbSchemaConstants.UniqueConstraintTypeLiteral:
                    query = SqlQueries.GetQueryForUniqueConstraint();
                    parameters.Add(new SqlParameter(SqlQueryConstants.IdentifyParam, context.SchemaAttributes[SqlQueryConstants.IndexColumnId]));

                    break;
            }

            var reader = sqlCommandHelper.MakeCommandWithParams(query,parameters.ToArray()).ExecuteReader();

            var constraint = MakeNodeCollection(DbSchemaConstants.Constraint, reader);
            return constraint;
        }
        protected override List<Node> MakeIndexes(SchemaNavigationContext context)
        {
            var indexId = context.SchemaAttributes[SqlQueryConstants.IndexId];

            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForIndex(),
                new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id),
                new SqlParameter(SqlQueryConstants.IdentifyParam, indexId)
                ).ExecuteReader();

            var index = MakeNodeCollection(DbSchemaConstants.Index, reader);
            return index;
        }
        protected override List<Node> MakeProcParams(SchemaNavigationContext context)
        {
            var parameterId = context.SchemaAttributes[SqlQueryConstants.ParameterId];

            var reader = sqlCommandHelper.MakeCommandWithParams(SqlQueries.GetQueryForProcParameter(),
                        new SqlParameter(SqlQueryConstants.SelfIdParam, context.Id),
                        new SqlParameter(SqlQueryConstants.IdentifyParam, parameterId)
                        ).ExecuteReader();

            var procParam = MakeNodeCollection(DbSchemaConstants.ProcParameter, reader);
            return procParam;
        }

        #endregion
    }
}
