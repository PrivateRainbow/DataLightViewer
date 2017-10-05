using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Text;

namespace DataLightViewer.Memento
{
    public static class ValidationSchemaProvider
    {
        public static XmlSchemaSet GetSchema(this ProjectFileType type)
        {
            string schemaUri = string.Empty;

            switch(type)
            {
                case ProjectFileType.Data:
                    schemaUri = Resources.DataToolsLight_Data;
                    break;

                case ProjectFileType.UI:
                    schemaUri = Resources.DataToolsLight_UI;
                    break;
            }

            return InitializeSchema(schemaUri);
        }

        private static XmlSchemaSet InitializeSchema(string schemaUri)
        {
            var schema = new XmlSchemaSet();

            byte[] content = Encoding.UTF8.GetBytes(schemaUri);
            using (var stream = new MemoryStream(content))
            {
                using (var reader = XmlReader.Create(stream))
                    schema.Add(null, reader);
            }

            return schema;
        }
    }

}
