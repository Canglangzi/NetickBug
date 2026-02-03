using System;
using System.Collections.Generic;

namespace CockleBurs.GameFramework.Utility
{
    public class MaxHeap<T>
    {
        private List<T> elements;
        private readonly IComparer<T> comparer;

        public MaxHeap(IComparer<T> comparer, int capacity = 16)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            elements = new List<T>(capacity);
        }

        public MaxHeap(int capacity = 16) : this(Comparer<T>.Default, capacity)
        {
        }

        public int Count => elements.Count;

        public void Add(T item)
        {
            elements.Add(item);
            HeapifyUp(elements.Count - 1);
        }

        public T Remove()
        {
            if (elements.Count == 0) throw new InvalidOperationException("Heap is empty");

            T root = elements[0];
            T last = elements[^1];
            elements[0] = last;
            elements.RemoveAt(elements.Count - 1);

            if (elements.Count > 0)
                HeapifyDown(0);

            return root;
        }

        public T Peek()
        {
            if (elements.Count == 0) throw new InvalidOperationException("Heap is empty");
            return elements[0];
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (comparer.Compare(elements[index], elements[parent]) <= 0)
                    break;
                Swap(index, parent);
                index = parent;
            }
        }

        private void HeapifyDown(int index)
        {
            int largest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left < elements.Count && comparer.Compare(elements[left], elements[largest]) > 0)
                largest = left;
            if (right < elements.Count && comparer.Compare(elements[right], elements[largest]) > 0)
                largest = right;

            if (largest != index)
            {
                Swap(index, largest);
                HeapifyDown(largest);
            }
        }

        private void Swap(int first, int second)
        {
            T temp = elements[first];
            elements[first] = elements[second];
            elements[second] = temp;
        }
    }
}