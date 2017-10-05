using System;
using Loader.Components;
using Loader.Services.Helpers;
using Loader.Types;
using System.Collections.Generic;

namespace Loader.Services.Factories
{
    internal abstract class BaseSqlNodeBuildFactory : ISqlNodeBuildFactory
    {
        #region Consts

        protected const string GoLiteral = "GO";
        protected const string UseLiteral = "USE";
        protected const string DatabaseHeaderLiteral = "DATABASE";
        protected const string TableHeaderLiteral = "TABLE";
        protected const string ViewHeaderLiteral = "VIEW";
        protected const string ProcedureHeaderLiteral = "PROCEDURE";
        protected const string ProcedureParamHeaderLiteral = "PROCEDURE PARAM";
        protected const string ColumnHeaderLiteral = "COLUMN";
        protected const string PrimaryKeyHeaderLiteral = "PRIMARY KEY";
        protected const string ForeignKeyHeaderLiteral = "FOREIGN KEY";
        protected const string IndexHeaderLiteral = "INDEX";
        protected const string DefaultConstraintHeaderLiteral = "DEFAULT CONSTRAINT";
        protected const string CheckedConstraintHeaderLiteral = "CHECKED CONSTRAINT";
        protected const string UniqueConstraintHeaderLiteral = "UNIQUE CONSTRAINT";

        #endregion

        #region Init

        protected DateTime CreationTime;
        protected HashSet<string> ParentTypes;

        protected BaseSqlNodeBuildFactory()
        {
            CreationTime = DateTime.Now;
            ParentTypes = new HashSet<string>
            {
                DbSchemaConstants.Table, DbSchemaConstants.View, DbSchemaConstants.Procedure
            };
        }

        #endregion

        #region Helpers
        
        protected string GetCaptionForDbObject(string type, string name = null)
        {
            return $@"/****** Object:  {type} {name ?? string.Empty}    Script Date: {CreationTime.ToString()} ******/";
        }

        protected string WrapInComments(string script)
        {
            return $@"/***** {script} ******/";
        }

        protected string GetParentName(Node node)
        {
            
            if (!ParentTypes.Contains(node.Name))
                throw new InvalidOperationException($" Such {node} was not expected!");
            return $"[{node.Attributes[SqlQueryConstants.SchemaName]}].[{node.Attributes[SqlQueryConstants.Name]}]";
        }

        #endregion

        #region Abstract

        public abstract string BuildDatabase(Node node);
        public abstract string BuildTable(Node node);
        public abstract string BuildView(Node node);
        public abstract string BuildProcedure(Node node);
        public abstract string BuildColumn(Node node, bool withParent = false);
        public abstract string BuildKey(Node node);
        public abstract string BuildConstraint(Node node);
        public abstract string BuildIndex(Node node);
        public abstract string BuildProcParameter(Node node);

        protected abstract string BuildPrimaryKey(Node node);
        protected abstract string BuildForeignKey(Node node);
        protected abstract string BuildDefaultConstraint(Node node);
        protected abstract string BuildCheckedConstraint(Node node);
        protected abstract string BuildUniqueConstraint(Node node);

        #endregion
    }
}
