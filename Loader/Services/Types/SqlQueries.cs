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
        internal static string GetQueryForDatabase()
        {
            var expr = $"SELECT d.[name], " +
                       $"d.[database_id], " +
                       $"d.[create_date] " +
                       $"FROM sys.databases as d " +
                       $"WHERE d.[database_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForTables()
        {
            var expr = $"SELECT s.[name] AS {SqlQueryConstants.SchemaName} , " +
                          $"t.[name] AS {SqlQueryConstants.Name} ," +
                          "t.[object_id], " +
                          "t.[schema_id], " +
                          "t.[type], " +
                          "t.[type_desc]," +
                          "t.[create_date], " +
                          "t.[modify_date] " +
                          "FROM sys.tables AS t INNER JOIN sys.schemas AS s ON t.[schema_id] = s.[schema_id]";
            return expr;
        }
        internal static string GetQueryForTable()
        {
            var expr = $"SELECT s.[name] AS {SqlQueryConstants.SchemaName} , " +
                          $"t.[name] AS {SqlQueryConstants.Name} ," +
                          "t.[object_id], " +
                          "t.[schema_id], " +
                          "t.[type], " +
                          "t.[type_desc]," +
                          "t.[create_date], " +
                          "t.[modify_date] " +
                          "FROM sys.tables AS t INNER JOIN sys.schemas AS s ON t.[schema_id] = s.[schema_id] " +
                          $"WHERE t.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForViews()
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
                        "FROM sys.views AS v INNER JOIN sys.schemas AS s ON v.[schema_id] = s.[schema_id] " +
                        $"WHERE v.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForProcedures()
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
                       "FROM sys.procedures AS p INNER JOIN sys.schemas AS s ON p.[schema_id] = s.[schema_id] " +
                       $"WHERE p.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForColumns()
        {
            var expr = $"SELECT cols.[name] AS {SqlQueryConstants.Name}," +
                       $" cols.[object_id], " +
                       "cols.[column_id], " +
                       $"ut.[name] AS {SqlQueryConstants.UserType}, " +
                       "cols.[precision], " +
                       "cols.[scale], " +
                       $"cols.[is_nullable] AS {SqlQueryConstants.IsNullable}, " +
                       $"cols.[is_identity] AS {SqlQueryConstants.IsIdentity}, " +
                       $"cols.[max_length] AS {SqlQueryConstants.MaxLength} " +
                       "FROM sys.[all_columns] AS cols " +
                       "INNER JOIN sys.[types] AS ut ON ut.[user_type_id] = cols.[user_type_id] " +
                       $"WHERE cols.[object_id] = {SqlQueryConstants.ParentIdParam}";

            return expr;
        }
        internal static string GetQueryForColumn()
        {
            var expr = $"SELECT cols.[name] AS {SqlQueryConstants.Name}, " +
                       $"cols.[object_id], " +
                       "cols.[column_id], " +
                       $"ut.[name] AS {SqlQueryConstants.UserType}, " +
                       "cols.[precision], " +
                       "cols.[scale], " +
                       $"cols.[is_nullable] AS {SqlQueryConstants.IsNullable}, " +
                       $"cols.[is_identity] AS {SqlQueryConstants.IsIdentity}, " +
                       $"cols.[max_length] AS {SqlQueryConstants.MaxLength} " +
                       "FROM sys.[all_columns] AS cols " +
                       "INNER JOIN sys.[types] AS ut ON ut.[user_type_id] = cols.[user_type_id] " +
                       $"WHERE cols.[object_id] = {SqlQueryConstants.SelfIdParam} " +
                       $"AND cols.[column_id] = {SqlQueryConstants.IdentifyParam}";

            return expr;
        }

        internal static string GetQueryForForeignKeys()
        {
            var expr = $"SELECT f.[name] AS {SqlQueryConstants.Name}," +
                       $" f.[object_id], " +
                       "f.[parent_object_id], " +
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
                       $"WHERE f.[parent_object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }
        internal static string GetQueryForForeignKey()
        {
            var expr = $"SELECT f.[name] AS {SqlQueryConstants.Name}, " +
                       "f.[object_id], " +
                       "f.[parent_object_id], " +
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
                       $"WHERE f.[parent_object_id] = {SqlQueryConstants.ParentIdParam} AND f.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForPrimaryKey()
        {
            var expr = "SELECT k.[name], " +
                       "k.[object_id], " +
                        "k.[parent_object_id], " +
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
                       $"WHERE i.[is_primary_key] = 1 AND k.[parent_object_id] = {SqlQueryConstants.ParentIdParam} " +
                       $"AND k.[object_id] = {SqlQueryConstants.SelfIdParam} AND k.[parent_object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }

        internal static string GetQueryForPrimaryKeys()
        {
            var expr = "SELECT " +
              "i.[name], " +
              "i.[object_id], " +
              "i.[index_id], " +
              "i.[type_desc], " +
              "obj.[type], " +
              "i.[is_primary_key], " +
              "i.[is_unique], " +
              "i.[is_unique_constraint], " +
              "STUFF(" +
              "(SELECT ', ' + COL_NAME(ic.[object_id], ic.[column_id]), " +
              "CASE ic.[is_descending_key] " +
              "WHEN 0 THEN ' ASC' " +
              "WHEN 1 THEN ' DESC' " +
              "END " +
              "FROM sys.index_columns AS ic " +
              "WHERE ic.[object_id] = i.[object_id] " +
              "AND " +
              "ic.[index_id] = i.[index_id] " +
              "ORDER BY ic.[index_column_id] " +
              $"FOR XML PATH('')), 1, 1, '') AS {SqlQueryConstants.Columns} " +
              "FROM sys.key_constraints AS k " +
              "INNER JOIN sys.indexes AS i ON i.[object_id] = k.[parent_object_id] " +
              "INNER JOIN sys.objects AS obj ON i.[object_id] = obj.[parent_object_id] AND i.[name] = obj.[name]" +
              $"WHERE k.[parent_object_id] = {SqlQueryConstants.ParentIdParam} AND i.[is_primary_key] = 1 " +
              "GROUP BY i.[name], " +
              " i.[object_id], " +
              " i.[index_id], " +
              " i.[type_desc], " +
              " obj.[type], " +
              " i.[is_primary_key], " +
              " i.[is_unique], " +
              " i.[is_unique_constraint]";

            return expr;
        }

        internal static string GetQueryForDefaultConstraints()
        {
            var expr = "SELECT " +
                       "dc.[definition], " +
                       "dc.[object_id], " +
                       "dc.[parent_object_id], " +
                       "dc.[name], " +
                       $"COL_NAME(dc.[parent_object_id], dc.[parent_column_id]) AS { SqlQueryConstants.ColumnName}, " +
                       "dc.[type], " +
                       "dc.[type_desc], " +
                       "dc.[create_date], " +
                       "dc.[modify_date] " +
                       "FROM sys.default_constraints AS dc " +
                       $"WHERE dc.[parent_object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }
        internal static string GetQueryForDefaultConstraint()
        {
            var expr = "SELECT " +
                       "dc.[definition], " +
                       "dc.[object_id], " +
                       "dc.[parent_object_id], " +
                       "dc.[name], " +
                       $"COL_NAME(dc.[parent_object_id], dc.[parent_column_id]) AS { SqlQueryConstants.ColumnName}, " +
                       "dc.[type], " +
                       "dc.[type_desc], " +
                       "dc.[create_date], " +
                       "dc.[modify_date] " +
                       "FROM sys.default_constraints AS dc " +
                       $"WHERE dc.[parent_object_id] = {SqlQueryConstants.ParentIdParam} AND dc.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForCheckedConstraints()
        {
            var expr = "SELECT " +
                       "cc.[definition], " +
                       "cc.[object_id], " +
                       "cc.[parent_object_id], " +
                       "cc.[name], " +
                       $"COL_NAME(cc.[parent_object_id], cc.[parent_column_id]) AS {SqlQueryConstants.ColumnName}, " +
                       "cc.[type], " +
                       "cc.[type_desc], " +
                       "cc.[create_date], " +
                       "cc.[modify_date] " +
                       "FROM sys.check_constraints AS cc " +
                       $"WHERE cc.[parent_object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }
        internal static string GetQueryForCheckedConstraint()
        {
            var expr = "SELECT " +
                       "cc.[definition], " +
                       "cc.[object_id], " +
                       "cc.[parent_object_id], " +
                       "cc.[name], " +
                       $"COL_NAME(cc.[parent_object_id], cc.[parent_column_id]) AS {SqlQueryConstants.ColumnName}, " +
                       "cc.[type], " +
                       "cc.[type_desc], " +
                       "cc.[create_date], " +
                       "cc.[modify_date] " +
                       "FROM sys.check_constraints AS cc " +
                       $"WHERE cc.[parent_object_id] = {SqlQueryConstants.ParentIdParam} AND cc.[object_id] = {SqlQueryConstants.SelfIdParam}";
            return expr;
        }

        internal static string GetQueryForUniqueConstraintsOld()
        {
            var expr = "SELECT " +
                       "i.[name], " +
                       "i.[object_id], " +
                       "i.[index_id], " +
                       "ic.[index_column_id], " +
                       "i.[type_desc], " +
                      $"i.[ignore_dup_key] AS {SqlQueryConstants.UcIgnoreDupKey}, " +
                       "obj.[type], " +
                       $"COL_NAME(ic.[object_id], ic.[column_id]) AS {SqlQueryConstants.ColumnName} " +
                       "FROM sys.indexes AS i " +
                       "INNER JOIN sys.index_columns AS ic ON i.[object_id] = ic.[object_id] " +
                       "AND i.[index_id] = ic.[index_id] " +
                       "INNER JOIN sys.objects AS obj ON i.[object_id] = obj.[parent_object_id] AND i.[name] = obj.[name]" +
                       $"WHERE i.[is_unique_constraint] = 1 AND i.[object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }

        internal static string GetQueryForUniqueConstraints()
        {
            var expr = "SELECT " +
              "i.[name], " +
              "i.[object_id], " +
              "i.[index_id], " +
              "i.[type_desc], " +
              "i.[is_unique_constraint], " +
              $"i.[ignore_dup_key] AS {SqlQueryConstants.UcIgnoreDupKey}, " +
              "obj.[type], " +
              "STUFF(" +
              "(SELECT ', ' + COL_NAME(ic.[object_id], ic.[column_id]), " +
              "CASE ic.[is_descending_key] " +
              "WHEN 0 THEN ' ASC' " +
              "WHEN 1 THEN ' DESC' " +
              "END " +
              "FROM sys.index_columns AS ic " +
              "WHERE ic.[object_id] = i.[object_id] " +
              "AND " +
              "ic.[index_id] = i.[index_id] " +
              "ORDER BY ic.[index_column_id] " +
              $"FOR XML PATH('')), 1, 1, '')  AS {SqlQueryConstants.Columns} " +
              "FROM sys.indexes AS i " +
              "INNER JOIN sys.objects AS obj ON i.[object_id] = obj.[parent_object_id] AND i.[name] = obj.[name]" +
              $"WHERE i.[is_unique_constraint] = 1 AND i.[object_id] = {SqlQueryConstants.ParentIdParam} " +
              "GROUP BY i.[name]," +
              " i.[object_id]," +
              " i.[index_id]," +
              " i.[type_desc], " +
              " obj.[type], " +
              "i.[ignore_dup_key], " +
              " i.[is_unique_constraint] ";
            return expr;
        }

        internal static string GetQueryForUniqueConstraint()
        {
            var expr = "SELECT " +
                       "i.[name], " +
                       "i.[object_id], " +
                       "i.[index_id], " +
                       "ic.[index_column_id], " +
                       "i.[type_desc], " +
                      $"i.[ignore_dup_key] AS {SqlQueryConstants.UcIgnoreDupKey}, " +
                       "obj.[type], "+
                       $"COL_NAME(ic.[object_id], ic.[column_id]) AS {SqlQueryConstants.ColumnName} " +
                       "FROM sys.indexes AS i " +
                       "INNER JOIN sys.index_columns AS ic ON i.[object_id] = ic.[object_id] " +
                       "AND i.[index_id] = ic.[index_id] " +
                       "INNER JOIN sys.objects AS obj ON i.[object_id] = obj.[parent_object_id] AND i.[name] = obj.[name]" +
                       $"WHERE i.[is_unique_constraint] = 1 AND i.[object_id] = {SqlQueryConstants.ParentIdParam} AND ic.[index_column_id] = { SqlQueryConstants.IdentifyParam}";
            return expr;
        }


        internal static string GetQueryForIndexes()
        {
            var expr = "SELECT " +
              "i.[name], " +
              "i.[object_id], " +
              "i.[index_id], " +
              "i.[type_desc], " +
              "i.[is_primary_key], " +
              "i.[is_unique], " +
              "STUFF(" +
              "(SELECT ', ' + COL_NAME(ic.[object_id], ic.[column_id]), " +
              "CASE ic.[is_descending_key] " +
              "WHEN 0 THEN ' ASC' " +
              "WHEN 1 THEN ' DESC' " +
              "END " +
              "FROM sys.index_columns AS ic " +
              "WHERE ic.[object_id] = i.[object_id] " +
              "AND " +
              "ic.[index_id] = i.[index_id] " +
              "ORDER BY ic.[index_column_id] " +
              $"FOR XML PATH('')), 1, 1, '')  AS {SqlQueryConstants.Columns}  " +
              "FROM sys.indexes AS i " +
              $"WHERE i.[object_id] = {SqlQueryConstants.ParentIdParam} " +
              "GROUP BY i.[name]," +
              " i.[object_id]," +
              " i.[index_id]," +
              " i.[type_desc], " +
              " i.[is_primary_key], " +
              " i.[is_unique] ";
            return expr;
        }
        internal static string GetQueryForIndex()
        {
            var expr = "SELECT " +
            "i.[name], " +
            "i.[object_id], " +
            "i.[type_desc], " +
            "i.[is_unique], " +
            "i.[is_primary], " +
            "STUFF(" +
            "(SELECT ',' + COL_NAME(ic.object_id, ic.column_id), " +
            "CASE ic.[is_descending_key] " +
            "WHEN 0 THEN ' ASC' " +
            "WHEN 1 THEN ' DESC' " +
            "END " +
            "FROM sys.index_columns AS ic " +
            "WHERE ic.[object_id] = i.[object_id] " +
            "AND" +
            "ic.[index_id] = i.[index_id] " +
            "ORDER BY ic.[index_column_id] " +
            $"FOR XML PATH(''), 1, 1, '') {SqlQueryConstants.Columns} " +
            "FROM sys.indexes AS i " +
            $"WHERE i.[object_id] = {SqlQueryConstants.SelfIdParam} AND i.[index_id] = {SqlQueryConstants.IdentifyParam} " +
            "GROUP BY i.[name], i.[object_id], i.[index_id], i.[type_desc], i.[is_unique], i.[is_primary]  ";

            return expr;
        }

        internal static string GetQueryForProcParameters()
        {
            var expr = "SELECT prm.[name], " +
                        "prm.[object_id], " +
                        "prm.[parameter_id], " +
                       "t.[name] AS type, " +
                       "prm.[is_output], " +
                       "prm.[has_default_value], " +
                       "prm.[default_value] " +
                       "FROM sys.parameters AS prm INNER JOIN sys.types AS t ON prm.[user_type_id] = t.[user_type_id] AND prm.[system_type_id] = t.[system_type_id] " +
                       $"WHERE prm.[object_id] = {SqlQueryConstants.ParentIdParam}";
            return expr;
        }
        internal static string GetQueryForProcParameter()
        {
            var expr = "SELECT prm.[name], " +
                        "prm.[object_id], " +
                        "prm.[parameter_id], " +
                       "t.[name] AS type, " +
                       "prm.[is_output], " +
                       "prm.[has_default_value], " +
                       "prm.[default_value] " +
                       "FROM sys.parameters AS prm INNER JOIN sys.types AS t ON prm.[user_type_id] = t.[user_type_id] AND prm.[system_type_id] = t.[system_type_id] " +
                       $"WHERE prm.[object_id] = {SqlQueryConstants.ParentIdParam} AND prm.[parameter_id] = {SqlQueryConstants.IdentifyParam}";
            return expr;
        }

        #endregion
    }
}
