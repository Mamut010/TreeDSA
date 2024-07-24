namespace TreeDSA.Utils
{
    public static class EnumerableUtil
    {
        public static string EnumerableToString<T>(IEnumerable<T> enumerable, string delimiter = ", ")
        {
            return $"[{string.Join(delimiter, enumerable)}]";
        }
    }
}
