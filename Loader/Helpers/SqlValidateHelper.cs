using System;

namespace Loader.Helpers
{
    public static class SqlValidateHelper
    {
        public static object GetValueByDefault(this Type type) => type.IsValueType
            ? Activator.CreateInstance(type)
            : string.Empty;
    }
}
