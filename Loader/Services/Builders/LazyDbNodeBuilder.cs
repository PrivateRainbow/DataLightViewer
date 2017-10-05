using System.Collections.Generic;
using Loader.Components;
using Loader.Helpers;
using Loader.Services.Types;
using Loader.Types;
using System;

namespace Loader.Services.Builders
{
    public class LazyDbNodeBuilder : BaseDbNodeBuilder
    {
        #region Init

        public LazyDbNodeBuilder() { }
        public LazyDbNodeBuilder(string connectionString) : base(connectionString) { }

        #endregion

        #region Implementation

        public override List<Node> MakeNode(BuildContext context)
        {
            var node = context.Node;
            var type = node.ResolveDbNodeType();

            SetConnection(context.Connection);

            try
            {
                switch (type)
                {
                    case DbSchemaObjectType.Server:
                        return LazyDataFetchingHandler(base.MakeDatabases);

                    case DbSchemaObjectType.Database:
                        return GetDatabaseBundle();

                    case DbSchemaObjectType.Tables:
                        return LazyDataFetchingHandler(base.MakeTables);

                    case DbSchemaObjectType.Table:
                        return GetTableBundle();

                    case DbSchemaObjectType.Views:
                        return LazyDataFetchingHandler(base.MakeViews);

                    case DbSchemaObjectType.View:
                        return GetViewBundle();

                    case DbSchemaObjectType.Procedures:
                        return LazyDataFetchingHandler(base.MakeProcedures);

                    case DbSchemaObjectType.Procedure:
                        return GetProcedureBundle();

                    case DbSchemaObjectType.Columns:
                        return LazyDataFetchingHandler(MakeColumns, node.GetNavigationContext(NavigationType.FromParent));

                    case DbSchemaObjectType.Keys:
                        return LazyDataFetchingHandler(MakeKeys, node.GetNavigationContext(NavigationType.FromParent));

                    case DbSchemaObjectType.Constraints:
                        return LazyDataFetchingHandler(MakeConstraints, node.GetNavigationContext(NavigationType.FromParent));

                    case DbSchemaObjectType.Indexes:
                        return LazyDataFetchingHandler(MakeIndexes, node.GetNavigationContext(NavigationType.FromParent));

                    case DbSchemaObjectType.Parameters:
                        return LazyDataFetchingHandler(MakeProcParams, node.GetNavigationContext(NavigationType.FromParent));

                    default:
                        throw new ArgumentException($"{nameof(type)}");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region Helpers

        protected virtual List<Node> GetDatabaseBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Tables),
                new Node(DbSchemaConstants.Views),
                new Node(DbSchemaConstants.Procedures)
            };
        }

        protected virtual List<Node> GetTableBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Columns),
                new Node(DbSchemaConstants.Keys),
                new Node(DbSchemaConstants.Constraints),
                new Node(DbSchemaConstants.Indexes)
            };
        }

        protected virtual List<Node> GetViewBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.Columns),
                new Node(DbSchemaConstants.Indexes)
            };
        }

        protected virtual List<Node> GetProcedureBundle()
        {
            return new List<Node>
            {
                new Node(DbSchemaConstants.ProcParameters)
            };
        }

        #endregion
    }
}
