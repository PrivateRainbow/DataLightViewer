using System;
using System.Collections.Generic;
using Loader.Scanners;
using Loader.Types;

namespace Loader.Factories
{
    public static class ScannerFactory
    {
        private static readonly Dictionary<SourceSchemaType, INodeScanner> _scanners;
        
        static ScannerFactory()
        {
            _scanners = new Dictionary<SourceSchemaType, INodeScanner>
            {
                {SourceSchemaType.File, new DefaultXmlScanner()},
                {SourceSchemaType.Database, new DatabaseXmlScanner()}
            };
        }

        public static INodeScanner MakeScanner(SourceSchemaType type)
        {
            if (!_scanners.ContainsKey(type))
                throw new ArgumentException($" Such {nameof(type)} was not expected!");

            return _scanners[type];
        }
    }
}
