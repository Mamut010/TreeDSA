namespace TreeDSA.Extensions
{
    public static partial class IntExtensions
    {
        public static bool IsEqual(this int compareToResult)
        {
            return compareToResult == 0;
        }

        public static bool IsLess(this int compareToResult)
        {
            return compareToResult < 0;
        }

        public static bool IsLessOrEqual(this int compareToResult)
        {
            return compareToResult <= 0;
        }

        public static bool IsGreater(this int compareToResult)
        {
            return compareToResult > 0;
        }

        public static bool IsGreaterOrEqual(this int compareToResult)
        {
            return compareToResult >= 0;
        }
    }
}
