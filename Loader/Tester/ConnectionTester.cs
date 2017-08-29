using Loader.Tester.Contexts;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Loader.Helpers
{

    public sealed class ConnectionTester
    {
        private readonly IDbConnectionContext _connectionContext;

        public ConnectionTester(SqlServerConnectionContext connectionContext)
        {
            _connectionContext = connectionContext ?? throw new ArgumentException($"{nameof(connectionContext)}");
        }

        public async Task<bool> VerifyConnectionAsync(CancellationToken cancelToken)
        {            
            using (_connectionContext.Connection)
            {
                try
                {
                    await _connectionContext.Connection.OpenAsync(cancelToken);
                    return true;
                }
                catch (SqlException) { throw; }
            }
        }

    }
}
