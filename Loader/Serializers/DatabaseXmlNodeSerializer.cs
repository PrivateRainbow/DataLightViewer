using System;
using System.IO;
using System.Xml;
using Loader.Components;

namespace Loader.Serializers
{
    public class DatabaseXmlNodeSerializer : INodeSerializer
    {
        private readonly TextWriter _textWriter;
        private const string NodeElementSuffix = "-value";

        public DatabaseXmlNodeSerializer(TextWriter writer)
        {
            _textWriter = writer ?? throw new ArgumentNullException($"{nameof(writer)}");
        }

        public void Serialize(Node node)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}");

            using (var writer = new XmlTextWriter(_textWriter) { Formatting = Formatting.Indented })
            {
                writer.WriteStartDocument();
                WriteXmlNode(node, writer);
                writer.WriteEndDocument();
            }
        }


        private static void WriteXmlNode(Node target, XmlWriter wr)
        {          
            wr.WriteStartElement(target.Name);

            if (target.HasAttributes())
                foreach (var attr in target.Attributes)
                    wr.WriteAttributeString(attr.Key, attr.Value);

            if (target.HasValue())
            {
                var artificialName = string.Concat(target.Name, NodeElementSuffix);
                wr.WriteStartElement(artificialName);
                wr.WriteValue(target.Value);
                wr.WriteEndElement();
            }

            if (target.HasChildren())
                foreach (var child in target.Children)
                    WriteXmlNode(child, wr);

            wr.WriteEndElement();
        }
    }
}
