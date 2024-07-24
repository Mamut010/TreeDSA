namespace TreeDSA.Extensions
{
    public static partial class IListExtensions
    {
        public static void AddOrSet<T>(this IList<T> @this, int index, T item)
        {
            if (index >= @this.Count)
            {
                @this.Add(item);
            }
            else
            {
                @this[index] = item;
            }
        }
    }
}
