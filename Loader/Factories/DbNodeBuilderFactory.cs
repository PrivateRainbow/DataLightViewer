using System.Collections.Generic;
using Loader.Services.Builders;
using Loader.Types;

namespace Loader.Factories
{
    public static class DbNodeBuilderFactory
    {
        private static readonly Dictionary<DbNodeBuilderType, BaseDbNodeBuilder > DbNodeBuilders;

        static DbNodeBuilderFactory()
        {
            DbNodeBuilders = new Dictionary<DbNodeBuilderType, BaseDbNodeBuilder>
            {
                {DbNodeBuilderType.Bulk, new BulkDbNodeBuilder()},
                {DbNodeBuilderType.Lazy, new LazyDbNodeBuilder()},
                {DbNodeBuilderType.Slim, new SlimDbNodeBuilder()}
            };
        }

        public static BaseDbNodeBuilder Make(DbNodeBuilderType type, string connectionString)
        {
            var builder = DbNodeBuilders[type];
            builder.InitializeBuilder(connectionString);
            return builder;
        }
    }
}
