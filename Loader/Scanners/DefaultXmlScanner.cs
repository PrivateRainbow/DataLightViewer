using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Loader.Components;

namespace Loader.Scanners
{
    internal class DefaultXmlScanner : BaseXmlScanner
    {
        public override Node Scan(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException($"{nameof(stream)}");

            var arteficialRoot = new Node("Arteficial");
            var parents = new Stack<Node>();
            var tags = new Stack<string>();

            parents.Push(arteficialRoot);

            using (var reader = new XmlTextReader(stream) { WhitespaceHandling = WhitespaceHandling.None})
            {
                while (reader.Read())
                {
                    if (reader.IsEmptyElement)
                    {
                        var child = new Node(reader.Name);
                        if (reader.HasAttributes)
                            FillNodeAttributes(child, reader);
                        parents.Peek().Add(child);
                    }
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            var elementName = reader.Name;
                            var child = new Node(elementName);

                            if (reader.HasAttributes) FillNodeAttributes(child, reader);

                            tags.Push(elementName);
                            parents.Peek().Add(child);
                            parents.Push(child);
                        }
                        else if (reader.NodeType == XmlNodeType.Text)
                        {
                            parents.Peek().Value = reader.Value;
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (reader.Name != tags.Peek()) continue;
                            tags.Pop();
                            parents.Pop();
                        }
                    }
                }

                // dispose arteficial root because of it was used only for building of the main node
                _scannedNode = arteficialRoot.Children[0];
                _scannedNode.Parent = null;

                return _scannedNode;
            }
        }
    }
}
