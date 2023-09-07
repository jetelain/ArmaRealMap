using StringToExpression.Util;

namespace GameRealisticMap.Conditions
{
    internal class TagFilterLanguageException : StringToExpression.Exceptions.ParseException
    {
        public TagFilterLanguageException(StringSegment errorSegment, string message)
            : base(errorSegment, message)
        {
        }

        public TagFilterLanguageException(StringSegment errorSegment, string message, Exception innerException)
            : base(errorSegment, message, innerException)
        {
        }
    }
}
