using System;
using System.Linq;

namespace Loader.Helpers
{
    public static class PredicateHelper
    {
        public static Predicate<T> Or<T>(params Predicate<T>[] predicates)
        {
            return delegate (T item)
            {
                return predicates.Any(predicate => predicate(item));
            };
        }

        public static Predicate<T> And<T>(this Predicate<T> first, Predicate<T> second)
        {
            return item => first(item) && second(item);
        }

        public static Predicate<T> And<T>(params Predicate<T>[] predicates)
        {
            return delegate(T item)
            {
                return predicates.All(predicate => predicate(item));
            };
        }
    }
}
