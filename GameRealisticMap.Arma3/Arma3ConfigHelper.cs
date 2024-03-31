using System.Text.RegularExpressions;

namespace GameRealisticMap.Arma3
{
    public static class Arma3ConfigHelper
    {
        public static readonly Regex ValidClassName = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.CultureInvariant);

        public static readonly Regex ValidPboPrefix = new Regex(@"^[a-zA-Z0-9_\\]+$", RegexOptions.CultureInvariant);

        public static bool IsValidClassName(string name)
        {
            return ValidClassName.IsMatch(name);
        }

        public static void ValidateWorldName(string worldName)
        {
            if (!ValidClassName.IsMatch(worldName))
            {
                throw new ArgumentException(ValidClassNameMessage(worldName));
            }
        }

        public static string ValidClassNameMessage(string worldName)
        {
            return $"'{worldName}' is not a valid name. You can use only letters (a-Z), numbers (0-9) and underscore (_).";
        }

        public static void ValidatePboPrefix(string pboPrefix)
        {
            if (!ValidPboPrefix.IsMatch(pboPrefix))
            {
                throw new ArgumentException($"'{pboPrefix}' is not a valid PboPrefix. You can use only letters (a-Z), numbers (0-9), underscore (_) and path separator (\\).");
            }
        }
    }
}
