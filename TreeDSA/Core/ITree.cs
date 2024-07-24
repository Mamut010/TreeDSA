namespace TreeDSA.Core
{
    public interface ITree<T> : ICollection<T>, IEnumerable<T>, IDisplayable
    {
        bool Add(T item);
        bool Contains(T item);
        bool Remove(T item);
    }
}
