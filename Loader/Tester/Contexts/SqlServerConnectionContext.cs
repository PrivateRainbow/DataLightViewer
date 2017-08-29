using Loader.Helpers;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Loader.Tester.Contexts
{
    public sealed class SqlServerConnectionContext : IDbConnectionContext
    {
        private SqlConnection _sqlConnection;
        private string _connectionString;

        public string ConnectionString => _connectionString;
        public DbConnection Connection => _sqlConnection;

        public SqlServerConnectionContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)} can not be empty.");

            _connectionString = connectionString;
            _sqlConnection = new SqlConnection(_connectionString);
        }
    }
}
