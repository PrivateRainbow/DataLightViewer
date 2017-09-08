using System;

namespace Loader.Types
{
    public enum TraversalStrategy
    {
        Depth,
        Breadth
    }

    public enum SourceSchemaType
    {
        File,
        Database
    }

    [Flags]
    internal enum SearchRequestType : byte
    {
        Empty = 0x0,
        ByName = 0x1,
        ByAttributes = 0x2
    }

    public enum SqlNodeBuilderType
    {
        TransactSql
    }

    public enum DbNodeBuilderType
    {
        Bulk,
        Lazy,
        PartialLazy
    }

    public enum DbSchemaObjectType
    {
        Server,
        Database,
        Table,
        View,
        Procedure,
        Index,
        Column,
        Key,
        Constraint,
        Parameter,

        Databases,
        Tables,
        Views,
        Procedures,
        Indexes,
        Columns,
        Keys,
        Constraints,
        Parameters
    }
}
