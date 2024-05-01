using System.Text;

namespace GameRealisticMap.Toolkit
{
    public static class Base36Converter
    {
        private const int Base = 36;
        private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string Convert(long value)
        {
            if (value == 0)
            {
                return "0";
            }
            if (value < 0)
            {
                if (value == long.MinValue)
                {
                    return "-" + Convert(((ulong)long.MaxValue)+1);
                }
                return "-" + Convert(-value);
            }
            return Convert((ulong)value);
        }

        public static string Convert(ulong value)
        {
            if (value == 0)
            {
                return "0";
            }
            var result = new StringBuilder();
            while (value > 0)
            {
                result.Insert(0, Chars[(int)(value % Base)]);
                value /= Base;
            }
            return result.ToString();
        }
    }
}
