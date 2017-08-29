using System;
using Loader.Types;
using Loader.Services.Factories;
using System.Collections.Generic;

namespace Loader.Factories
{
    public static class SqlNodeBuilderFactory
    {
        private static readonly Dictionary<SqlNodeBuilderType, ISqlNodeBuildFactory> SqlNodeBuildFactories;

        static SqlNodeBuilderFactory()
        {
            SqlNodeBuildFactories = new Dictionary<SqlNodeBuilderType, ISqlNodeBuildFactory>
            {
                {SqlNodeBuilderType.TransactSql, new TransactSqlNodeBuildFactory() }
            };
        }

        public static ISqlNodeBuildFactory Make(SqlNodeBuilderType type)
        {
            if (!SqlNodeBuildFactories.ContainsKey(type))
                throw new ArgumentException($" Such {nameof(type)} was not expected!");

            return SqlNodeBuildFactories[type];
        }
    }
}
