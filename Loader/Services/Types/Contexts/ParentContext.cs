using System;
using System.Collections.Generic;

namespace Loader.Services.Types
{
    public enum NavigationType
    {
        Self = 0,
        FromParent = 1,
        WithParent = 2
    }

    public class SchemaNavigationContext
    {
        public string Id { get; internal set; }
        public string ParentId { get; internal set; }

        public Dictionary<string, string> SchemaAttributes { get; internal set; }
        public NavigationType Type { get; }

        private SchemaNavigationContext(string id, NavigationType type)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"{nameof(id)}");

            Id = id;
            Type = type;
        }
        private SchemaNavigationContext(string selfId, string parentId) 
        {
            if (string.IsNullOrEmpty(parentId))
                throw new ArgumentException($"{nameof(parentId)}");

            Id = selfId;
            ParentId = parentId;
            Type = NavigationType.WithParent;
        }

        public void With(KeyValuePair<string, string> attribute)
        {
            SchemaAttributes = SchemaAttributes ?? new Dictionary<string, string>();
            SchemaAttributes.Add(attribute.Key, attribute.Value);
        }

        public static SchemaNavigationContext GetFromId(string id) => new SchemaNavigationContext(id, NavigationType.Self);
        public static SchemaNavigationContext GetFromSelfWithParent(string id, string parentId) => new SchemaNavigationContext(id, parentId);
    }

}
