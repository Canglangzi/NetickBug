using System;
using System.Collections.Generic;
using UnityEngine;

namespace CockleBurs.GameFramework.Utility
{
    public class MinHeap<T>
    {
        private List<T> elements;
        private Dictionary<T, int> indexMap;
        private readonly IComparer<T> comparer;
    
        public MinHeap(IComparer<T> comparer, int capacity = 16)
        {
            this.comparer = comparer;
            elements = new List<T>(capacity);
            indexMap = new Dictionary<T, int>(capacity);
        }
    
        public int Count => elements.Count;
    
        public void Add(T item)
        {
            elements.Add(item);
            int index = elements.Count - 1;
            indexMap[item] = index;
            HeapifyUp(index);
        }
    
        public T Remove()
        {
            if (elements.Count == 0) throw new InvalidOperationException("Heap is empty");
    
            T root = elements[0];
            T last = elements[^1];
            elements[0] = last;
            indexMap[last] = 0;
            elements.RemoveAt(elements.Count - 1);
            indexMap.Remove(root);
    
            if (elements.Count > 0)
                HeapifyDown(0);
    
            return root;
        }
    
        public T Peek()
        {
            if (elements.Count == 0) throw new InvalidOperationException("Heap is empty");
            return elements[0];
        }
    
        public void Update(T item)
        {
            if (!indexMap.TryGetValue(item, out int index)) return;
            HeapifyUp(index);
            HeapifyDown(index);
        }
    
        public void Remove(T item)
        {
            if (!indexMap.TryGetValue(item, out int index)) return;
    
            T last = elements[^1];
            elements[index] = last;
            indexMap[last] = index;
            elements.RemoveAt(elements.Count - 1);
            indexMap.Remove(item);
    
            if (index < elements.Count)
            {
                HeapifyUp(index);
                HeapifyDown(index);
            }
        }
    
        public void Clear()
        {
            elements.Clear();
            indexMap.Clear();
        }
    
        private void HeapifyUp(int index)
        {
            int parent = (index - 1) / 2;
            while (index > 0 && comparer.Compare(elements[index], elements[parent]) < 0)
            {
                Swap(index, parent);
                index = parent;
                parent = (index - 1) / 2;
            }
        }
    
        private void HeapifyDown(int index)
        {
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;
    
            if (left < elements.Count && comparer.Compare(elements[left], elements[smallest]) < 0)
                smallest = left;
            if (right < elements.Count && comparer.Compare(elements[right], elements[smallest]) < 0)
                smallest = right;
    
            if (smallest != index)
            {
                Swap(index, smallest);
                HeapifyDown(smallest);
            }
        }
    
        private void Swap(int first, int second)
        {
            T temp = elements[first];
            elements[first] = elements[second];
            elements[second] = temp;
    
            indexMap[elements[first]] = first;
            indexMap[elements[second]] = second;
        }
    }

}
