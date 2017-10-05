using Loader.Components;
using Loader.Helpers;
using Loader.Types;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLightViewer.Helpers
{
    public static class DbConnectionHelper
    {
        private static readonly HashSet<string> _highLevelNodes = new HashSet<string> {
            DbSchemaConstants.Server, DbSchemaConstants.Databases
        };

        private static readonly Dictionary<string, string> _databaseConnectionStrings = new Dictionary<string, string>();
        private static readonly SqlConnectionStringBuilder _connectionBuilder = new SqlConnectionStringBuilder();

        public static string GetConnectionString(this Node node)
        {
            if (_highLevelNodes.Contains(node.Name))
                return App.ServerConnectionString;

            var connectionKey = node.Name == DbSchemaConstants.Database 
                ? node.Attributes["name"] 
                : node.GetParentNodeByCondition(n => n.Name == DbSchemaConstants.Database).Attributes["name"];

            if (!_databaseConnectionStrings.ContainsKey(connectionKey))
            {
                _connectionBuilder.ConnectionString = App.ServerConnectionString;
                _connectionBuilder.InitialCatalog = connectionKey;

                _databaseConnectionStrings.Add(connectionKey, _connectionBuilder.ConnectionString);

                return _databaseConnectionStrings[connectionKey];
            }

            return _databaseConnectionStrings[connectionKey];
        }

        public static void InitializeConnections(List<Node> nodes) => nodes.ForEach(n => n.GetConnectionString());
        public static void InvalidateCash() => _databaseConnectionStrings?.Clear();

    }
}
