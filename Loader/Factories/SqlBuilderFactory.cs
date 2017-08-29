using System;
using Loader.Services.Factories;
using System.Collections.Generic;
using Loader.Types;

namespace Loader.Factories
{
    public static class SqlBuilderFactory
    {
        private static readonly Dictionary<SqlNodeBuilderType, ISqlNodeBuildFactory> _sqlNodeBuildFactories;

        static SqlBuilderFactory()
        {
            _sqlNodeBuildFactories = new Dictionary<SqlNodeBuilderType, ISqlNodeBuildFactory>
            {
                {SqlNodeBuilderType.TransactSql, new TransactSqlNodeBuildFactory() }
            };
        }

        public static ISqlNodeBuildFactory Make(SqlNodeBuilderType type)
        {
            if (!_sqlNodeBuildFactories.ContainsKey(type))
                throw new ArgumentException($" Such {nameof(type)} was not expected!");

            return _sqlNodeBuildFactories[type];
        }
    }
}
