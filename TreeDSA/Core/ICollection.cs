namespace TreeDSA.Core
{
    public interface ICollection<T>
    {
        int Count { get; }

        bool IsEmpty();
        void Clear();
        List<T> ToList();
        T[] ToArray();
    }
}
