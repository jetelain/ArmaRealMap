using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using StringToExpression;
using StringToExpression.Exceptions;
using StringToExpression.GrammerDefinitions;
using StringToExpression.Tokenizer;
using StringToExpression.Util;

namespace GameRealisticMap.Conditions
{
    public class TagFilterLanguage
    {
        public static readonly TagFilterLanguage Instance = new TagFilterLanguage();

        private readonly Language language;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagFilterLanguage"/> class.
        /// </summary>
        public TagFilterLanguage()
        {
            language = new Language(AllDefinitions().ToArray());
        }

        public List<Token> Tokenize(string text) => language.Tokenizer.Tokenize(text).ToList();

        /// <summary>
        /// Parses the specified text converting it into a predicate expression
        /// </summary>
        /// <typeparam name="T">The input type</typeparam>
        /// <param name="text">The text to parse.</param>
        /// <returns></returns>
        public Expression<Func<T, bool>> Parse<T>(string text, string paramName = "point")
        {
            try
            {
                var parameters = new[] { Expression.Parameter(typeof(T), paramName) };
                var body = language.Parse(text, parameters);

                ExpressionConversions.TryBoolean(ref body);

                return Expression.Lambda<Func<T, bool>>(body, parameters);
            }
            catch (OperationInvalidException ex) when (ex.InnerException != null)
            {
                throw new TagFilterLanguageException(ex.ErrorSegment, $"Expression '{text}' is invalid: {ex.InnerException.Message}", ex.InnerException);
            }
            catch (ParseException ex)
            {
                throw new TagFilterLanguageException(ex.ErrorSegment, $"Expression '{text}' is invalid: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Expression '{text}' is invalid: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns all the definitions used by the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> AllDefinitions()
        {
            IEnumerable<FunctionCallDefinition> functions;
            var definitions = new List<GrammerDefinition>();
            definitions.AddRange(TypeDefinitions());
            definitions.AddRange(functions = FunctionDefinitions());
            definitions.AddRange(BracketDefinitions(functions));
            definitions.AddRange(LogicalOperatorDefinitions());
            definitions.AddRange(ArithmeticOperatorDefinitions());
            definitions.AddRange(PropertyDefinitions());
            definitions.AddRange(WhitespaceDefinitions());
            return definitions;
        }

        /// <summary>
        /// Returns the definitions for types used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> TypeDefinitions()
        {
            return new[]
            {
                new OperandDefinition(
                    name:"STRING",
                    regex: @"'(?:\\.|[^'])*'",
                    expressionBuilder: x => Expression.Constant(x.Trim('\'')
                        .Replace("\\'", "'")
                        .Replace("\\r", "\r")
                        .Replace("\\f", "\f")
                        .Replace("\\n", "\n")
                        .Replace("\\\\", "\\")
                        .Replace("\\b", "\b")
                        .Replace("\\t", "\t"))),
                new OperandDefinition(
                    name:"NULL",
                    regex: @"null",
                    expressionBuilder: x => Expression.Constant(null)),
                new OperandDefinition(
                    name:"BOOL",
                    regex: @"true|false",
                    expressionBuilder: x => Expression.Constant(bool.Parse(x))),
                new OperandDefinition(
                    name:"FLOAT",
                    regex: @"\-?\d+\.\d+",
                    expressionBuilder: x => Expression.Constant(float.Parse(x, CultureInfo.InvariantCulture))),
                new OperandDefinition(
                    name:"INTEGER",
                    regex: @"\-?\d+",
                    expressionBuilder: x => Expression.Constant(float.Parse(x, CultureInfo.InvariantCulture))),
            };
        }

        /// <summary>
        /// Returns the definitions for logic operators used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> LogicalOperatorDefinitions()
        {
            return new GrammerDefinition[]
            {
                new BinaryOperatorDefinition(
                    name:"EQ",
                    regex: @"==",
                    orderOfPrecedence:11,
                    expressionBuilder: ConvertEnumsIfRequired((left,right) => Expression.Equal(left, right))),
                new BinaryOperatorDefinition(
                    name:"NE",
                    regex: @"!=",
                    orderOfPrecedence:12,
                    expressionBuilder: ConvertEnumsIfRequired((left,right) => Expression.NotEqual(left, right))),

                new BinaryOperatorDefinition(
                    name:"GE",
                    regex: @"\>=",
                    orderOfPrecedence:14,
                    expressionBuilder: (left,right) => Expression.GreaterThanOrEqual(left, right)),

                new BinaryOperatorDefinition(
                    name:"GT",
                    regex: @"\>",
                    orderOfPrecedence:13,
                    expressionBuilder: (left,right) => Expression.GreaterThan(left, right)),

                new BinaryOperatorDefinition(
                    name:"LE",
                    regex: @"\<=",
                    orderOfPrecedence:16,
                    expressionBuilder: (left,right) => Expression.LessThanOrEqual(left, right)),
                new BinaryOperatorDefinition(
                    name:"LT",
                    regex: @"\<",
                    orderOfPrecedence:15,
                    expressionBuilder: (left,right) => Expression.LessThan(left, right)),

                new BinaryOperatorDefinition(
                    name:"AND",
                    regex: @"&&",
                    orderOfPrecedence:17,
                    expressionBuilder: (left,right) => Expression.And(left, right)),
                new BinaryOperatorDefinition(
                    name:"OR",
                    regex: @"\|\|",
                    orderOfPrecedence:18,
                    expressionBuilder: (left,right) => Expression.Or(left, right)),

                new UnaryOperatorDefinition(
                    name:"NOT",
                    regex: @"!",
                    orderOfPrecedence:19,
                    operandPosition: RelativePosition.Right,
                    expressionBuilder: (arg) => {
                        ExpressionConversions.TryBoolean(ref arg);
                        return Expression.Not(arg);
                    })
            };
        }

        /// <summary>
        /// Returns the definitions for arithmetic operators used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> ArithmeticOperatorDefinitions()
        {
            return new[]
            {
                 new BinaryOperatorDefinition(
                    name:"ADD",
                    regex: @"\+",
                    orderOfPrecedence: 2,
                    expressionBuilder: (left,right) => Expression.Add(left, right)),
                new BinaryOperatorDefinition(
                    name:"SUB",
                    regex: @"\-",
                    orderOfPrecedence: 2,
                    expressionBuilder: (left,right) => Expression.Subtract(left, right)),
                new BinaryOperatorDefinition(
                    name:"MUL",
                    regex: @"\*",
                    orderOfPrecedence: 1,
                    expressionBuilder: (left,right) => Expression.Multiply(left, right)),
                new BinaryOperatorDefinition(
                    name:"DIV",
                    regex: @"\/",
                    orderOfPrecedence: 1,
                    expressionBuilder: (left,right) => Expression.Divide(left, right)),
                new BinaryOperatorDefinition(
                    name:"MOD",
                    regex: @"%",
                    orderOfPrecedence: 1,
                    expressionBuilder: (left,right) => Expression.Modulo(left, right)),
            };
        }

        /// <summary>
        /// Returns the definitions for brackets used within the language.
        /// </summary>
        /// <param name="functionCalls">The function calls in the language. (used as opening brackets)</param>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> BracketDefinitions(IEnumerable<FunctionCallDefinition> functionCalls)
        {
            BracketOpenDefinition openBracket;
            ListDelimiterDefinition delimeter;
            return new GrammerDefinition[] {
                openBracket = new BracketOpenDefinition(
                    name: "OPEN_BRACKET",
                    regex: @"\("),
                delimeter = new ListDelimiterDefinition(
                    name: "COMMA",
                    regex: ","),
                new BracketCloseDefinition(
                    name: "CLOSE_BRACKET",
                    regex: @"\)",
                    bracketOpenDefinitions: new[] { openBracket }.Concat(functionCalls),
                    listDelimeterDefinition: delimeter)
            };
        }

        /// <summary>
        /// Returns the definitions for functions used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<FunctionCallDefinition> FunctionDefinitions()
        {
            return new FunctionCallDefinition[]
            {
                 //new FunctionCallDefinition(
                 //   name:"FN_STARTSWITH",
                 //   regex: @"startswith\(",
                 //   argumentTypes: new[] {typeof(string), typeof(string) },
                 //   expressionBuilder: (parameters) => {
                 //       return Expression.Call(
                 //           instance:parameters[0],
                 //           method:StringMembers.StartsWith,
                 //           arguments: new [] { parameters[1] });
                 //   })

            };
        }

        /// <summary>
        /// Returns the definitions for property names used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> PropertyDefinitions()
        {
            return new[]
            {
                 //Properties
                 new OperandDefinition(
                    name:"PROPERTY_PATH",
                    regex: @"(?<![0-9])([A-Za-z_][A-Za-z0-9_]*)+",
                    expressionBuilder: (value, parameters) => {
                        var property = parameters[0].Type.GetProperty(value, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                        if ( property == null)
                        {
                            throw new ArgumentException($"Property '{value}' does not exists.");
                        }
                        return Expression.MakeMemberAccess((Expression)parameters[0], property);
                    }),
            };
        }

        /// <summary>
        /// Returns the definitions for whitespace used within the language.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<GrammerDefinition> WhitespaceDefinitions()
        {
            return new[]
            {
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
            };
        }


        /// <summary>
        /// Wraps the function to convert any constants to enums if required
        /// </summary>
        /// <param name="expFn">Function to wrap</param>
        /// <returns></returns>
        protected Func<Expression, Expression, Expression> ConvertEnumsIfRequired(Func<Expression, Expression, Expression> expFn)
        {
            return (left, right) =>
            {
                var didConvertEnum = ExpressionConversions.TryEnumNumberConvert(ref left, ref right)
                    || ExpressionConversions.TryEnumStringConvert(ref left, ref right, ignoreCase: true);

                return expFn(left, right);
            };
        }
    }
}
