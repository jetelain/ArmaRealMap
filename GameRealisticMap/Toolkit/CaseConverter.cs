using System.Text;

namespace GameRealisticMap.Toolkit
{
    public static class CaseConverter
    {
        public static string ToPascalCase(string name)
        {
            var sb = new StringBuilder();
            foreach (var token in name.Split('_', '-'))
            {
                if (token.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(token[0]));
                    if (token.Length > 1)
                    {
                        sb.Append(token.Substring(1).ToLowerInvariant());
                    }
                }
            }
            return sb.ToString();
        }
    }
}
