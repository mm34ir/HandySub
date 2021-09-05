using System;
using System.Collections.ObjectModel;

namespace HandySub.Common
{
    public static class CollectionExtension
    {
        public static void AddIfNotExists<T>(this ObservableCollection<T> collection, T value)
        {
            CheckObservableCollectionIsNull(collection);

            if (!collection.Contains(value))
            {
                collection.Add(value);
            }
        }
        public static void DeleteIfExists<T>(this ObservableCollection<T> collection, T value)
        {
            CheckObservableCollectionAndValueIsNull(collection, value);

            if (collection.Contains(value))
            {
                collection.Remove(value);
            }
        }
        private static void CheckObservableCollectionIsNull<T>(this ObservableCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
        }
        private static void CheckValueIsNull<T>(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
        private static void CheckObservableCollectionAndValueIsNull<T>(this ObservableCollection<T> collection, T value)
        {
            CheckObservableCollectionIsNull(collection);
            CheckValueIsNull(value);
        }
    }
}
