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

        private static readonly HashSet<string> DbNodesWithValue;
           
        protected SqlConnection SqlConnection;
        protected SqlCommandHelper SqlCommandHelper;

        #endregion

        #region Init

        static BaseDbNodeBuilder()  
        {
            DbNodesWithValue = new HashSet<string>
            {
                DbSchemaConstants.View,
                DbSchemaConstants.Procedure
            };
        }

        protected BaseDbNodeBuilder() {}
        protected BaseDbNodeBuilder(string connectionString)
        {
            InitializeBuilder(connectionString);
        }

        public void InitializeBuilder(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)}");

            SqlConnection = new SqlConnection(connectionString);
            SqlCommandHelper = new SqlCommandHelper(SqlConnection);
        }

        public void SetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)}");

            if (!ReferenceEquals(SqlConnection.ConnectionString, connectionString))
                SqlConnection.ConnectionString = connectionString;
        }

        #endregion

        #region Abstract

        public abstract List<Node> MakeNode(BuildContext context);
        protected abstract List<Node> MakeDatabases();
        protected abstract List<Node> MakeTables();
        protected abstract List<Node> MakeViews();
        protected abstract List<Node> MakeProcedures();
        protected abstract List<Node> MakeColumns(string objectId);
        protected abstract List<Node> MakeKeys(string objectId);
        protected abstract List<Node> MakeConstraints(string objectId);
        protected abstract List<Node> MakeIndexes(string objectId);
        protected abstract List<Node> MakeProcParams(string objectId);

        #endregion

        #region Helpers

        protected List<Node> MakeNodeCollection(string nodeName, DbDataReader dataReader)
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
                    if (DbNodesWithValue.Contains(nodeName))
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
