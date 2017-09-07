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
        public DatabaseXmlScanner(Stream stream) : base(stream) {}
        public DatabaseXmlScanner(Stream stream, IScanContext context) : base(stream, context) {}

        public override Node Scan()
        {
            const string NodeElementSuffix = "-value";

            var nodeWithArtificialValues = new HashSet<string>
            {
                string.Concat(DbSchemaConstants.View, NodeElementSuffix),
                string.Concat(DbSchemaConstants.Procedure, NodeElementSuffix)
            };

            var arteficialRoot = new Node("Arteficial");
            var parents = new Stack<Node>();
            var tags = new Stack<string>();

            parents.Push(arteficialRoot);

            using (var reader = XmlReader.Create(_input, _readerSettings))
            {
                while (reader.Read())
                {
                    if (!_isValid)
                    {
                        _isInterrupted = true;
                        break;
                    }

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

                                if (reader.HasAttributes)
                                    FillNodeAttributes(child, reader);

                                tags.Push(elementName);
                                if (!nodeWithArtificialValues.Contains(tags.Peek()))
                                    parents.Peek().Add(child);
                                parents.Push(child);

                                break;

                            case XmlNodeType.Text:
                                if (!nodeWithArtificialValues.Contains(tags.Peek()))
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

                if (_isInterrupted)
                    throw new InvalidOperationException(_onValidationFailedMessage);
                    
                // dispose arteficial root because of it was used only for building of the main node
                _scannedNode = arteficialRoot.Children[0];
                _scannedNode.Parent = null;

                return _scannedNode;
            }
        }
    }
}
