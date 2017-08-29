using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Loader.Components;
using Loader.Types;

namespace Loader.Scanners
{
    internal class DatabaseXmlScanner : BaseXmlScanner
    {
        private readonly HashSet<string> _nodeWithArtificialValues;
        private const string NodeElementSuffix = "-value";

        public DatabaseXmlScanner()
        {
            _nodeWithArtificialValues = new HashSet<string>
            {
                string.Concat(DbSchemaConstants.View, NodeElementSuffix),
                string.Concat(DbSchemaConstants.Procedure, NodeElementSuffix)
            };
        }

        public override Node Scan(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException($"{nameof(stream)}");

            var arteficialRoot = new Node("Arteficial");
            var parents = new Stack<Node>();
            var tags = new Stack<string>();

            parents.Push(arteficialRoot);

            using (var reader = new XmlTextReader(stream) { WhitespaceHandling = WhitespaceHandling.None })
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
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:

                                var elementName = reader.Name;
                                var child = new Node(elementName);

                                if (reader.HasAttributes) FillNodeAttributes(child, reader);

                                tags.Push(elementName);
                                if(!_nodeWithArtificialValues.Contains(tags.Peek()))
                                    parents.Peek().Add(child);
                                parents.Push(child);

                                break;

                            case XmlNodeType.Text:                                                                
                                if (!_nodeWithArtificialValues.Contains(tags.Peek()))
                                    parents.Peek().Value = reader.Value;
                                else
                                {
                                    tags.Pop();
                                    parents.Pop();
                                    parents.Peek().Value = reader.Value;
                                }
                                break;

                            case XmlNodeType.EndElement:

                                if (reader.Name != tags.Peek()) continue;
                                    parents.Pop();
                                    tags.Pop();
                                break;
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
