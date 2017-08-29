using System.Collections.Generic;
using System.IO;
using System.Xml;
using Loader.Components;

namespace Loader.Scanners
{
    internal abstract class BaseXmlScanner : INodeScanner
    {
        #region Data
        protected Node _scannedNode;
        #endregion

        #region Helpers
        protected static void FillNodeAttributes(Node targetNode, XmlReader reader)
        {
            var count = reader.AttributeCount;
            targetNode.Attributes = new Dictionary<string, string>(count);

            for (var i = 0; i < count; i++)
            {
                reader.MoveToAttribute(i);
                targetNode.Attributes[reader.Name] = reader.Value;
            }
        }
        #endregion

        #region Abstract
        public abstract Node Scan(Stream stream);
        #endregion
    }
}
