using System.Collections;

namespace UAssetEditor.Classes;

public abstract class Container<T> : IEnumerable<T>
{
    internal List<T> Items { get; }

    public Container(List<T> items)
    {
        Items = items;
    }

    public void Add(T item) => Items.Add(item);

    public virtual int GetIndex(string str)
    {
        throw new  NotImplementedException();
    }

    public bool Contains(T i) => Items.Contains(i);
    
    public T this[int index] => Items[index];
    public T this[uint index] => Items[(int)index];
    
    public int Length => Items.Count;
    
    public IEnumerator<T> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}