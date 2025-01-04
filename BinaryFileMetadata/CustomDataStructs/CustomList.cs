using System;

namespace BinaryFileMetadata
{

    public class CustomList<T>
    {
        private T[] data;
        private int count;

        private const int INITIAL_CAPACITY = 8;

        public CustomList()
        {
            data = new T[INITIAL_CAPACITY];
            count = 0;
        }

        // Returns the count of elements 
        public int Count
        {
            get 
            { 
                return count; 
            }
        }

        //Indexer to access or modify elements at a specific index.
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException("Index out of range in CustomArray.");
                return data[index];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException("Index out of range in CustomArray.");
                data[index] = value;
            }
        }

 
        public void Add(T item)
        {
            if (count == data.Length)
            {
                // Resize by doubling
                T[] bigger = new T[data.Length * 2];
                for (int i = 0; i < data.Length; i++)
                {
                    bigger[i] = data[i];
                }
                data = bigger;
            }
            data[count++] = item;
        }


        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                return false;

            for (int i = index; i < count - 1; i++)
            {
                data[i] = data[i + 1];
            }
            data[count - 1] = default(T);
            count--;
            return true;
        }
    }
}
