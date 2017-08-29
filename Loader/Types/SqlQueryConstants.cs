﻿namespace Loader.Types
{
    public static class SqlQueryConstants
    {
        #region Params
        public const string ObjectIdParam = "@table_object_id";
        public const string ObjectIdLiteral = "object_id";
        #endregion

        #region Common
        public const string Definition = "definition";
        public const string Type = "type";
        public const string UserType = "type";

        public const string TypeDesc = "type_desc";
        public const string Name = "name";
        public const string Precision = "precision";
        public const string Scale = "scale";

        public const string MaxLength = "maxLength";
        public const string IsNullable = "is_nullable";
        public const string IsIdentity = "is_identity";
        public const string IsOutputParam = "is_output";
        public const string HasDefaultValue = "has_default_value";
        public const string DefaultValue = "default_value";

        public const string TableName = "tableName";
        public const string SchemaName = "schemaName";
        public const string ColumnName = "columnName";
        #endregion

        public const string PkIsDescendingKey = "isDescendingKey";
        public const string IsUnique = "isUnique";
        public const string IsPrimary = "isPrimary";
        public const string IsForeign = "isForeign";

        #region FK
        public const string FkParentTable = "parentTable";
        public const string FkParentColumn = "parentColumn";
        public const string FkReferentialTable = "referentialTable";
        public const string FkReferentialColumn = "referentialColumn";
        public const string FkOnUpdateAction = "onUpdateAction";
        public const string FkOnDeleteAction = "onDeleteAction";
        #endregion

        #region Constraints
        public const string DcFieldName = "defaultForField";
        public const string UcIgnoreDupKey = "ignoreDuplicationKey";
        #endregion
    }
}