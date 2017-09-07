using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Loader.Components;
using System.Xml.Schema;
using System.Diagnostics;

namespace Loader.Scanners
{
    internal abstract class BaseXmlScanner : INodeScanner
    {
        #region Data

        protected Node _scannedNode;

        protected Stream _input;
        protected IScanContext _context;
        protected XmlReaderSettings _readerSettings;

        protected bool _scanWithSchemaValidation;
        protected bool _isValid;
        protected bool _isInterrupted;
        protected string _onValidationFailedMessage;

        public object LogWrapper { get; private set; }

        #endregion

        #region Init

        protected BaseXmlScanner(Stream input)
        {
            _input = input ?? throw new ArgumentException($"{nameof(input)}");

            _readerSettings = new XmlReaderSettings { IgnoreWhitespace = true };

            _isValid = true;
            _isInterrupted = false;
        }

        protected BaseXmlScanner(Stream input, IScanContext context)
        {
            _input = input ?? throw new ArgumentException($"{nameof(input)}");
            _context = context ?? throw new ArgumentException($"{nameof(context)}");

            _isValid = true;
            _isInterrupted = false;

            XmlSchemaSet schema;
            if (_context is XmlNodeValidationContext xmlValidationContext)
            {
                schema = xmlValidationContext.Schema;

                _readerSettings = new XmlReaderSettings();
                _readerSettings.IgnoreWhitespace = true;

                _readerSettings.Schemas = schema;
                _readerSettings.ValidationType = ValidationType.Schema;
                _readerSettings.ValidationEventHandler += _readerSettings_ValidationEventHandler;
            }
            else
                throw new ArgumentException($" Such {nameof(context)} was not expected. Use <code> XmlNodeValidationContext </code>.");
        }

        #endregion

        #region Abstract
        public abstract Node Scan();
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

        #region Validation

        private void _readerSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            _onValidationFailedMessage = e.Message;
            _isValid = false;
        }

        #endregion
    }
}
