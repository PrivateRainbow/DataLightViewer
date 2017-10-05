using System;
using System.Data.SqlClient;
using Loader.Types;

namespace Loader.Services.Helpers
{

    public sealed class SqlCommandHelper
    {
        private readonly SqlConnection _connection;

        #region Init

        public SqlCommandHelper(SqlConnection connection)
        {
            if(connection == null)
                throw new ArgumentException($"{nameof(connection)}");
            _connection = connection;
        }

        public void SetConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)}");

            if (!ReferenceEquals(_connection.ConnectionString, connectionString))
                _connection.ConnectionString = connectionString;
        }

        #endregion

        public SqlCommand MakeCommand(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException($"{nameof(expression)}");

            return new SqlCommand(expression, _connection);
        }

        public SqlCommand MakeCommandWithParam(string expression, string objectId)
        {
            if (string.IsNullOrEmpty(expression)) throw new ArgumentException($"{nameof(expression)}");
            if (string.IsNullOrEmpty(objectId)) throw new ArgumentException($"{nameof(objectId)}");

            var param = new SqlParameter(SqlQueryConstants.ParentIdParam, objectId);
            var cmd = new SqlCommand(expression, _connection);
            cmd.Parameters.Add(param);

            return cmd;
        }

        public SqlCommand MakeCommandWithParams(string expression, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(expression)) throw new ArgumentException($"{nameof(expression)}");

            var cmd = new SqlCommand(expression, _connection);
            if (parameters != null && parameters.Length > 0)
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

            return cmd;
        }

    }
}
