using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using GameRealisticMap.Conditions;
using System.Linq;
using StringToExpression.Tokenizer;
using StringToExpression.Util;
using GameRealisticMap.Algorithms;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public class ConditionToken
    {
        private static readonly HashSet<string> neural = new HashSet<string>()
        {
            "AND", "OR", "OPEN_BRACKET", "COMMA", "CLOSE_BRACKET"
        };

        public ConditionToken(TokenType tokenType, Token token, Color color, string hint = "")
        {
            TokenType = tokenType;
            Text = token.Value;
            SourceMap = token.SourceMap;
            Hint = hint;
            Color = color;
        }

        public TokenType TokenType { get; set; }

        public string Text { get; }

        public StringSegment SourceMap { get; }

        public string Hint { get; }

        public Color Color { get; set; }

        public Brush Brush => new SolidColorBrush(Color); 
        
        public static List<ConditionToken> Create(string? text, StringSegment? error = null)
        {
            var newTokens = new List<ConditionToken>();
            if (string.IsNullOrEmpty(text))
            {
                return newTokens;
            }

            ConditionToken? previous = null;
            foreach (var token in TagFilterLanguage.Instance.Tokenize(text))
            {

                var color = Colors.Transparent;
                var type = TokenType.Neutral;
                if (error != null && token.SourceMap.Start <= error.End && token.SourceMap.End >= error.Start)
                {
                    type = TokenType.Error;
                }
                else
                {
                    if (token.Definition.Name == "NOT")
                    {
                        type = TokenType.Not;
                    }
                    else if (neural.Contains(token.Definition.Name))
                    {
                        CloseLastInList(previous);
                    }
                    else
                    {
                        if (previous == null || previous.TokenType == TokenType.Neutral)
                        {
                            type = TokenType.Begin;
                        }
                        else
                        {
                            type = TokenType.Middle;
                            color = previous?.Color ?? color;
                        }
                        if (token.Definition.Name == "PROPERTY_PATH")
                        {
                            color = GetColor(token.Value);
                            PropagateBack(newTokens, color);
                        }
                    }
                }
                newTokens.Add(previous = new ConditionToken(type, token, color));
            }
            CloseLastInList(previous);
            

            return newTokens;
        }

        private static void PropagateBack(IEnumerable<ConditionToken> newTokens, Color color)
        {
            foreach (var prev in newTokens.Reverse())
            {
                if (prev.TokenType == TokenType.Begin)
                {
                    prev.Color = color;
                    return;
                }
                if (prev.TokenType != TokenType.Middle)
                {
                    return;
                }
                prev.Color = color;
            }
        }

        public static Color GetColor(string value)
        {
            return nameToColor.GetOrAdd(value, AllocateColor);
        }

        private static ConcurrentDictionary<string, Color> nameToColor = new ConcurrentDictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        private static Color AllocateColor(string name)
        {
            var rnd = RandomHelper.CreateRandom(name);
            return Color.FromRgb((byte)rnd.Next(180, 250), (byte)rnd.Next(180, 250), (byte)rnd.Next(180, 250));
        }

        private static void CloseLastInList(ConditionToken? previous)
        {
            if (previous != null)
            {
                if (previous.TokenType == TokenType.Middle)
                {
                    previous.TokenType = TokenType.End;
                }
                else if (previous.TokenType == TokenType.Begin)
                {
                    previous.TokenType = TokenType.Single;
                }
            }
        }
    }
}