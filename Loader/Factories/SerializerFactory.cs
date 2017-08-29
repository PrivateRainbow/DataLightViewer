using Loader.Types;
using System.IO;
using Loader.Serializers;

namespace Loader.Factories
{
    public static class SerializerFactory
    {
        public static INodeSerializer MakeSerializer(SourceSchemaType type, TextWriter writer)
        {
            switch(type)
            {
                case SourceSchemaType.File:
                    return new DefaultXmlNodeSerializer(writer);

                case SourceSchemaType.Database:
                    return new DatabaseXmlNodeSerializer(writer);                    
            }

            return null;
        }
    }
}
