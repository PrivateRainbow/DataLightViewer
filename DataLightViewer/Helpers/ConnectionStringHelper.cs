using System.Data.SqlClient;

namespace DataLightViewer.Helpers
{
    public static class ConnectionStringHelper
    {
        private static readonly SqlConnectionStringBuilder ConnectionBuilder = new SqlConnectionStringBuilder();
        public static string GetServerName(this string connectionString)
        {
            ConnectionBuilder.Clear();
            ConnectionBuilder.ConnectionString = connectionString;

            return ConnectionBuilder.DataSource;
        }
    }
}
