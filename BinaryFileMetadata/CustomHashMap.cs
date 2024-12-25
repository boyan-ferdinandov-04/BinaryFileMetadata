using System;
using System.Collections.Generic;

namespace BinaryFileMetadata
{
    public class CustomHashMap<TKey, TValue>
    {

        private List<KeyValuePair<TKey, TValue>>[] buckets;
        private int capacity;

        // Keep track of how many items are in the map
        private int count;

        public CustomHashMap(int initialCapacity = 16)
        {
            if (initialCapacity <= 0) initialCapacity = 16;
            capacity = initialCapacity;
            buckets = new List<KeyValuePair<TKey, TValue>>[capacity];
            for (int i = 0; i < capacity; i++)
            {
                buckets[i] = new List<KeyValuePair<TKey, TValue>>();
            }
            count = 0;
        }

        private int GetBucketIndex(TKey key)
        {
            int hash = key.GetHashCode() & 0x7FFFFFFF;
            return hash % capacity;
        }

        public void Add(TKey key, TValue value)
        {
            int index = GetBucketIndex(key);
            var bucket = buckets[index];

            // Check if the key already exists; if so, overwrite
            for (int i = 0; i < bucket.Count; i++)
            {
                if (bucket[i].Key.Equals(key))
                {
                    // Overwrite existing
                    bucket[i] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
            }
            // Otherwise add a new pair
            bucket.Add(new KeyValuePair<TKey, TValue>(key, value));
            count++;
        }


        public bool ContainsKey(TKey key)
        {
            int index = GetBucketIndex(key);
            var bucket = buckets[index];

            foreach (var kv in bucket)
            {
                if (kv.Key.Equals(key)) return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            int index = GetBucketIndex(key);
            var bucket = buckets[index];

            for (int i = 0; i < bucket.Count; i++)
            {
                if (bucket[i].Key.Equals(key))
                {
                    bucket.RemoveAt(i);
                    count--;
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < capacity; i++)
            {
                buckets[i].Clear();
            }
            count = 0;
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = GetBucketIndex(key);
                var bucket = buckets[index];

                foreach (var kv in bucket)
                {
                    if (kv.Key.Equals(key)) return kv.Value;
                }
                throw new KeyNotFoundException($"Key '{key}' not found.");
            }
            set
            {
                Add(key, value);
            }
        }


        public int Count
        {
            get { return count; }
        }
    }
}
