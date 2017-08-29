using Loader.Types;

namespace Loader.Helpers
{
    public static class StringHelper
    {
        public static string DbSchemaObjectNameToPluralForm(this string obj)
        {
            switch (obj)
            {
                case DbSchemaConstants.Table: return DbSchemaConstants.Tables;
                case DbSchemaConstants.View: return DbSchemaConstants.Views;
                case DbSchemaConstants.Procedure: return DbSchemaConstants.Procedures;
                case DbSchemaConstants.Column: return DbSchemaConstants.Columns;
                case DbSchemaConstants.Key: return DbSchemaConstants.Keys;
                case DbSchemaConstants.Index: return DbSchemaConstants.Indexes;
                case DbSchemaConstants.Constraint: return DbSchemaConstants.Constraints;
                case DbSchemaConstants.ProcParameter: return DbSchemaConstants.ProcParameters;

                default: return string.Empty;
            }
        }

    }
}
