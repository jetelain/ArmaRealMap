using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Toolkit
{
    public static class Base36Converter
    {
        private const int Base = 36;
        private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string Convert(long value)
        {
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
