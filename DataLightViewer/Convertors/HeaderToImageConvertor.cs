using Loader.Types;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System;
using System.Globalization;

namespace DataLightViewer.Convertors
{
    [ValueConversion(typeof(DbSchemaObjectType), typeof(BitmapImage))]
    public class HeaderToImageConvertor : IValueConverter
    {
        public static readonly HeaderToImageConvertor Instance = new HeaderToImageConvertor();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var image = "Icons/artificial.png";

            switch ((DbSchemaObjectType) value)
            {
                case DbSchemaObjectType.Database:
                    image = "Icons/database.png";
                    break;
                case DbSchemaObjectType.Table:
                    image = "Icons/table-data.png";
                    break;
                case DbSchemaObjectType.View:
                    image = "Icons/view-data.png";
                    break;
                case DbSchemaObjectType.Procedure:
                    image = "Icons/procedure-data.png";
                    break;
                case DbSchemaObjectType.Key:
                    image = "Icons/key.png";
                    break;

            }

            return new BitmapImage(new Uri($"pack://application:,,,/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
