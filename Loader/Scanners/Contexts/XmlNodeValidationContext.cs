using System.Xml.Schema;
using System;

namespace Loader.Scanners
{
    public class XmlNodeValidationContext : IScanContext
    {
        public XmlSchemaSet Schema { get; }
        public XmlNodeValidationContext(XmlSchemaSet schema)
        {
            Schema = schema ?? throw new ArgumentException($"{nameof(schema)}");
        }
    }
}
