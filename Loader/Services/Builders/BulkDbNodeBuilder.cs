using Loader.Services.Types;
using System;
using System.Collections.Generic;
using Loader.Components;
using Loader.Helpers;
using Loader.Types;


namespace Loader.Services.Builders
{
    public class BulkDbNodeBuilder : BaseDbNodeBuilder
    {
        public BulkDbNodeBuilder() { }
        public BulkDbNodeBuilder(string connectionString) : base(connectionString) { }

        #region Overriden

        protected override List<Node> MakeDatabases()
        {
            using (sqlConnection)
            {
                sqlConnection.Open();

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

                return new List<Node> { database };
            }
        }

        protected override List<Node> MakeTables()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForTables()).ExecuteReader();
            var tables = MakeNodeCollection(DbSchemaConstants.Table, reader);

            foreach (var t in tables)
            {
                var bundle = MakeBundleForTable(t.GetNavigationContext(NavigationType.Self));
                bundle.ForEach(b => t.Add(b));
            }

            return tables;
        }

        protected override List<Node> MakeViews()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForViews()).ExecuteReader();
            var views = MakeNodeCollection(DbSchemaConstants.View, reader);

            foreach (var v in views)
            {
                var bundle = MakeBundleForView(v.GetNavigationContext(NavigationType.Self));
                bundle.ForEach(b => v.Add(b));
            }

            return views;
        }

        protected override List<Node> MakeProcedures()
        {
            var reader = sqlCommandHelper.MakeCommand(SqlQueries.GetQueryForProcedures()).ExecuteReader();
            var procedures = MakeNodeCollection(DbSchemaConstants.Procedure, reader);

            foreach (var p in procedures)
            {
                var bundle = MakeBundleForProcedure(p.GetNavigationContext(NavigationType.Self));
                bundle.ForEach(b => p.Add(b));
            }

            return procedures;
        }

        protected override List<Node> MakeBundleForDatabase()
        {
            var tablesHeader = new Node(DbSchemaConstants.Tables);
            var viewsHeader = new Node(DbSchemaConstants.Views);
            var proceduresHeader = new Node(DbSchemaConstants.Procedures);

            var tables = MakeTables();
            var views = MakeViews();
            var procedures = MakeProcedures();

            tables.ForEach(t => tablesHeader.Add(t));
            views.ForEach(t => viewsHeader.Add(t));
            procedures.ForEach(t => proceduresHeader.Add(t));

            var bunch = new List<Node> { tablesHeader, viewsHeader, proceduresHeader };
            return bunch;
        }

        #endregion

        #region Implementation

        public override List<Node> MakeNode(BuildContext context)
        {
            var node = context.Node;
            var type = context.Node.ResolveDbNodeType();

            SetConnection(context.Connection);


            switch (type)
            {
                case DbSchemaObjectType.Database:
                    return LazyDataFetchingHandler(MakeBundleForDatabase);

                case DbSchemaObjectType.Tables:
                    return LazyDataFetchingHandler(MakeTables);

                case DbSchemaObjectType.Views:
                    return LazyDataFetchingHandler(MakeViews);

                case DbSchemaObjectType.Procedures:
                    return LazyDataFetchingHandler(MakeProcedures);


                case DbSchemaObjectType.Columns:
                    return LazyDataFetchingHandler(base.MakeColumns, node.GetNavigationContext(NavigationType.FromParent));

                case DbSchemaObjectType.Keys:
                    return LazyDataFetchingHandler(base.MakeKeys, node.GetNavigationContext(NavigationType.FromParent));

                case DbSchemaObjectType.Constraints:
                    return LazyDataFetchingHandler(base.MakeConstraints, node.GetNavigationContext(NavigationType.FromParent));

                case DbSchemaObjectType.Indexes:
                    return LazyDataFetchingHandler(base.MakeIndexes, node.GetNavigationContext(NavigationType.FromParent));


                case DbSchemaObjectType.Table:
                    return LazyDataFetchingHandler(MakeBundleForTable, node.GetNavigationContext(NavigationType.Self));

                case DbSchemaObjectType.View:
                    return LazyDataFetchingHandler(MakeBundleForView, node.GetNavigationContext(NavigationType.Self));

                case DbSchemaObjectType.Procedure:
                    return LazyDataFetchingHandler(MakeBundleForProcedure, node.GetNavigationContext(NavigationType.Self));

                default:
                    throw new ArgumentException($"{nameof(type)}");
            }

        }

        #endregion

    }
}
