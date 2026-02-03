using System;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
[Serializable]
public class TSet<T>
{
    private HashSet<T> set;

    public TSet()
    {
        set = new HashSet<T>();
    }

    public void Add(T item)
    {
        if (!set.Add(item))
        {
            throw new ArgumentException("Item already exists in the set.");
        }
    }

    public bool Remove(T item)
    {
        return set.Remove(item);
    }

    public bool Contains(T item)
    {
        return set.Contains(item);
    }

    public int Count => set.Count;

    public void Clear()
    {
        set.Clear();
    }

    public void UnionWith(TSet<T> otherSet)
    {
        set.UnionWith(otherSet.set);
    }

    public void IntersectWith(TSet<T> otherSet)
    {
        set.IntersectWith(otherSet.set);
    }

    public void ExceptWith(TSet<T> otherSet)
    {
        set.ExceptWith(otherSet.set);
    }

    public HashSet<T>.Enumerator GetEnumerator()
    {
        return set.GetEnumerator();
    }
}
}