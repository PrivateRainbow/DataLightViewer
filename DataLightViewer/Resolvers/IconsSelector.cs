using Loader.Types;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace DataLightViewer.Resolvers
{
    public static class IconsSelector
    {
        private static Dictionary<DbSchemaObjectType, BitmapImage> _iconsDictionary;
        private static HashSet<DbSchemaObjectType> _typesWithPredefinedIcons;

        private const string _uriSource = "pack://application:,,,/Icons/{0}.png";

        static IconsSelector()
        {
            _typesWithPredefinedIcons = new HashSet<DbSchemaObjectType>
            {
                DbSchemaObjectType.Database,
                DbSchemaObjectType.Table,
                DbSchemaObjectType.View,
                DbSchemaObjectType.Procedure,
                DbSchemaObjectType.Key,
                DbSchemaObjectType.Artificial
            };

            _iconsDictionary = new Dictionary<DbSchemaObjectType, BitmapImage>();

            foreach( var iconType in _typesWithPredefinedIcons)
            {
                var lowerIconName = iconType.ToString().ToLower();
                var uri = new Uri(string.Format(_uriSource, iconType));
                var image = new BitmapImage(uri);
                image.Freeze();
                _iconsDictionary.Add(iconType, image);
            }            
        }

        public static BitmapImage SelectIconByDbObjectType(DbSchemaObjectType type) => _iconsDictionary[type];        
    }
}
    

