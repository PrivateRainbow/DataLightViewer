using System.Data.Common;

namespace Loader.Helpers
{
    public interface IDbConnectionContext
    {
        DbConnection Connection { get; }
        string ConnectionString { get; }
    }
}
