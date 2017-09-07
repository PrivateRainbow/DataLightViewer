using System;
using System.Collections.Generic;
using Loader.Scanners;
using Loader.Types;
using System.IO;

namespace Loader.Factories
{
    public static class ScannerFactory
    {
        private static readonly Dictionary<SourceSchemaType, Type> _scanners;
        
        static ScannerFactory()
        {
            _scanners = new Dictionary<SourceSchemaType, Type>
            {
                {SourceSchemaType.File, typeof(DefaultXmlScanner)},
                {SourceSchemaType.Database, typeof(DatabaseXmlScanner)}
            };
        }

        public static INodeScanner Make(SourceSchemaType type, Stream stream)
        {
            if (!_scanners.ContainsKey(type))
                throw new ArgumentException($" Such {nameof(type)} was not expected!");

            return (INodeScanner) Activator.CreateInstance(_scanners[type], stream);
        }

        public static INodeScanner Make(SourceSchemaType type, Stream stream, IScanContext context)
        {
            if (!_scanners.ContainsKey(type))
                throw new ArgumentException($" Such {nameof(type)} was not expected!");

            return (INodeScanner)Activator.CreateInstance(_scanners[type], stream, context);
        }
    }
}
