using System;
using System.Collections.Generic;

namespace DataLightViewer.Mediator
{
    public class MultiDictionary<T, K> : Dictionary<T, List<K>>
    {
        public void AttachItem(T key, K item)
        {
            EnsureKey(key);
            this[key].Add(item);
        }

        public void AttachItems(T key, IEnumerable<K> items)
        {
            EnsureKey(key);
            this[key].AddRange(items);
        }

        public bool DetachItem(T key, K item)
        {
            if (!ContainsKey(key))
                return false;

            this[key].Remove(item);

            if (this[key].Count == 0)
                Remove(key);

            return true;
        }

        public bool DetachItemsWithKey(T key)
        {
            if (!ContainsKey(key))
                return false;

            this[key].RemoveAll(p => p != null);
            Remove(key);

            return true;
        }

        public bool DetachItemsWithKey(T key, Predicate<K> condition)
        {
            if (!ContainsKey(key))
                return false;

            this[key].RemoveAll(condition);
            if (this[key].Count == 0)
                Remove(key);

            return true;
        }

        private void EnsureKey(T key)
        {
            if (!ContainsKey(key))
                this[key] = new List<K>(1);
        }
    }
}
