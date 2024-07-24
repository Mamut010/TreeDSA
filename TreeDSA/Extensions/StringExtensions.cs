using System.Text;

namespace TreeDSA.Extensions
{
    public static partial class StringExtensions
    {
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 0)
            {
                return input;
            }

            var builder = new StringBuilder(input.Length * count);

            for (var i = 0; i < count; i++) builder.Append(input);

            return builder.ToString();
        }
    }
}
