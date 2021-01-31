using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public static class SystemExtensions
{
    public static void zipWithNext<T>(this IEnumerable<T> source, Action<T,T> action)
    {
        T prev = source.FirstOrDefault();
        foreach (T next in source.Skip(1))
        {
            action(prev, next);
            prev = next;
        }
    }

    private static Random rand = new Random();

    public static T PickRandom<T>(this IList<T> source)
    {
        if (source == null || source.Count == 0) throw new InvalidOperationException();

        return source[rand.Next(source.Count)];
    }
}


public class SelectedSortedList<K, V> : SortedList<K,V>
{
    private Func<V, K> keySelector;

    public SelectedSortedList(Func<V,K> keySelector)
    {
        this.keySelector = keySelector;
    }

    public void Add(V value)
    {
        base.Add(keySelector(value), value);
    }

    public V RemoveAtIndex(int index)
    {
        V value = Values[index];
        Remove(Keys[index]);
        return value;
    }
}