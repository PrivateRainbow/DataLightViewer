using Loader.Types;

namespace Loader.Services.Types
{
    internal static class SqlQueries
    {
        #region  Queries

        internal static string GetQueryForDatabases()
        {
            var expr = $"SELECT d.[name]," +
                       $"d.[database_id]," +
                       $"d.[create_date]" +
                       $"FROM sys.databases as d";
            return expr;
        }

        internal static string GetQueryForTable()
        {
            var expr = $"SELECT s.[name] AS {SqlQueryConstants.SchemaName} , " +
                          $"t.[name] AS {SqlQueryConstants.Name} ," +
                          "t.[object_id], " +
                          "t.[type], " +
                          "t.[type_desc]," +
                          "t.[create_date], " +
                          "t.[modify_date] " +
                          "FROM sys.tables AS t INNER JOIN sys.schemas AS s ON t.[schema_id] = s.[schema_id]";
            return expr;
        }
        internal static string GetQueryForView()
        {
            var expr = "SELECT " +
                        "OBJECT_DEFINITION(v.[object_id]), " +
                        "v.[name], " +
                        "v.[object_id]," +
                        $"s.[name] AS {SqlQueryConstants.SchemaName}, " +
                        "v.[schema_id], " +
                        "v.[create_date], " +
                        "v.[modify_date] " +
                        "FROM sys.views AS v INNER JOIN sys.schemas AS s ON v.[schema_id] = s.[schema_id]";
            return expr;
        }
        internal static string GetQueryForProcedure()
        {
            var expr = "SELECT " +
                       $"OBJECT_DEFINITION(p.[object_id]) AS {SqlQueryConstants.Definition}, " +
                       "p.[name], " +
                       "p.[object_id], " +
                       $"s.[name] AS {SqlQueryConstants.SchemaName}, " +
                       "p.[create_date], " +
                       "p.[modify_date], " +
                       "p.[type], " +
                       "p.[type_desc] " +
                       "FROM sys.procedures AS p INNER JOIN sys.schemas AS s ON p.[schema_id] = s.[schema_id]";
            return expr;
        }
        internal static string GetQueryForColumn()
        {
            var expr = $"SELECT cols.[name] AS {SqlQueryConstants.Name}, " +
                       $"ut.[name] AS {SqlQueryConstants.UserType}, " +
                       "cols.[precision], " +
                       "cols.[scale], " +
                       $"cols.[is_nullable] AS {SqlQueryConstants.IsNullable}, " +
                       $"cols.[is_identity] AS {SqlQueryConstants.IsIdentity}, " +
                       $"cols.[max_length] AS {SqlQueryConstants.MaxLength} " +
                       "FROM sys.[all_columns] AS cols " +
                       "INNER JOIN sys.[types] AS ut ON ut.[user_type_id] = cols.[user_type_id] " +
                       $"WHERE cols.[object_id] = {SqlQueryConstants.ObjectIdParam}";

            return expr;
        }
        internal static string GetQueryForForeignKey()
        {
            var expr = $"SELECT f.[name] AS {SqlQueryConstants.Name}, " +
                       $"f.[type] AS {SqlQueryConstants.Type}, " +
                       $"OBJECT_NAME(f.[parent_object_id]) AS {SqlQueryConstants.FkParentTable}, " +
                       $"COL_NAME(fc.[parent_object_id], fc.[parent_column_id]) AS {SqlQueryConstants.FkParentColumn}, " +
                       $"OBJECT_NAME(f.[referenced_object_id]) AS {SqlQueryConstants.FkReferentialTable}, " +
                       $"COL_NAME(fc.[referenced_object_id], fc.[referenced_column_id]) AS {SqlQueryConstants.FkReferentialColumn}, " +
                       "f.[is_disabled], " +
                       $"f.[delete_referential_action_desc] AS {SqlQueryConstants.FkOnDeleteAction}, " +
                       $"f.[update_referential_action_desc] AS {SqlQueryConstants.FkOnUpdateAction} " +
                       "FROM sys.foreign_keys AS f " +
                       "INNER JOIN sys.foreign_key_columns AS fc " +
                       "ON f.[object_id] = fc.[constraint_object_id] " +
                       $"WHERE f.[parent_object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        internal static string GetQueryForPrimaryKey()
        {
            var expr = "SELECT k.[name], " +
                       $"k.[type] AS {SqlQueryConstants.Type}, " +
                       "i.[type_desc], " +
                       "k.[create_date], " +
                       "k.[modify_date], " +
                       $"ic.[is_descending_key] AS {SqlQueryConstants.PkIsDescendingKey}, " +
                       $"COL_NAME(ic.[object_id], ic.[column_id]) AS {SqlQueryConstants.ColumnName} " +
                       "FROM sys.key_constraints AS k " +
                       "INNER JOIN sys.indexes AS i ON k.[parent_object_id] = i.[object_id] " +
                       "INNER JOIN sys.index_columns AS ic ON i.[object_id] = ic.[object_id] " +
                       "AND i.[index_id] = ic.[index_id] " +
                       $"WHERE i.[is_primary_key] = 1 AND k.[parent_object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        internal static string GetQueryForDefaultConstraint()
        {
            var expr = "SELECT " +
                       "dc.[definition], " +
                       "dc.[name], " +
                       $"COL_NAME(dc.[parent_object_id], dc.[parent_column_id]) AS { SqlQueryConstants.ColumnName}, " +
                       "dc.[type], " +
                       "dc.[type_desc], " +
                       "dc.[create_date], " +
                       "dc.[modify_date] " +
                       "FROM sys.default_constraints AS dc " +
                       $"WHERE dc.[parent_object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        internal static string GetQueryForCheckedConstraint()
        {
            var expr = "SELECT " +
                       "cc.[definition], " +
                       "cc.[name], " +
                       $"COL_NAME(cc.[parent_object_id], cc.[parent_column_id]) AS {SqlQueryConstants.ColumnName}, " +
                       "cc.[type], " +
                       "cc.[type_desc], " +
                       "cc.[create_date], " +
                       "cc.[modify_date] " +
                       "FROM sys.check_constraints AS cc " +
                       $"WHERE cc.[parent_object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        internal static string GetQueryForUniqueConstraint()
        {
            var expr = "SELECT " +
                       "i.[name], " +
                       "i.[type], " +
                       "i.[type_desc], " +
                      $"i.[ignore_dup_key] AS {SqlQueryConstants.UcIgnoreDupKey}, " +
                       $"COL_NAME(ic.[object_id], ic.[column_id]) AS {SqlQueryConstants.ColumnName} " +
                       "FROM sys.indexes AS i " +
                       "INNER JOIN sys.index_columns AS ic ON i.[object_id] = ic.[object_id] " +
                       "AND i.[index_id] = ic.[index_id] " +
                       $"WHERE i.[is_unique_constraint] = 1 AND i.[object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        internal static string GetQueryForIndex()
        {
            var expr = "SELECT " +
                       "i.[name], " +
                       $"COL_NAME(ic.[object_id], ic.[column_id]) AS {SqlQueryConstants.ColumnName}, " +
                       "i.[type_desc], " +
                       $"i.[is_primary_key] AS {SqlQueryConstants.IsPrimary}, " +
                       $"i.[is_unique] AS {SqlQueryConstants.IsUnique}, " +
                       "ic.[is_descending_key]" +
                       "FROM sys.indexes AS i INNER JOIN sys.index_columns AS ic ON i.[object_id] = ic.[object_id] " +
                       "AND i.[index_id] = ic.[index_id] " +
                       $"WHERE i.[object_id] = {SqlQueryConstants.ObjectIdParam}";

            return expr;
        }
        internal static string GetQueryForProcParameter()
        {
            var expr = "SELECT prm.[name], " +
                       "t.[name] AS type, " +
                       "prm.[is_output], " +
                       "prm.[has_default_value], " +
                       "prm.[default_value] " +
                       "FROM sys.parameters AS prm INNER JOIN sys.types AS t ON prm.[user_type_id] = t.[user_type_id] AND prm.[system_type_id] = t.[system_type_id] " +
                       $"WHERE prm.[object_id] = {SqlQueryConstants.ObjectIdParam}";
            return expr;
        }
        #endregion
    }
}
