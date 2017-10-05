using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Loader.Types;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DataLightViewer
{

    public static class SyntaxHighlightResolver
    {
        private static byte[] TransactHighlightSchema = Resources.t_sql_syntax;

        private static readonly Dictionary<SqlNodeBuilderType, byte[]> _higlightSchemas;

        static SyntaxHighlightResolver()
        {
            _higlightSchemas = new Dictionary<SqlNodeBuilderType, byte[]>
            {
                {SqlNodeBuilderType.TransactSql, TransactHighlightSchema}
            };
        }

        public static IHighlightingDefinition ResolveHiglightingDefinition(SqlNodeBuilderType syntaxType)
        {
            var resource = _higlightSchemas[syntaxType];

            using (var stream = new MemoryStream(resource))
            {
                using (var reader = XmlReader.Create(stream))
                {
                    var hl = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    return hl;
                }
            }
        }
    }
}
