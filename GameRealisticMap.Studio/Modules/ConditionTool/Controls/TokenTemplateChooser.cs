using System.Windows;
using System.Windows.Controls;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.ConditionTool.Controls
{
    internal class TokenTemplateChooser : DataTemplateSelector
    {
        public DataTemplate Neutral { get; set; }

        public DataTemplate Middle { get; set; }

        public DataTemplate Begin { get; set; }

        public DataTemplate Single { get; set; }

        public DataTemplate End { get; set; }

        public DataTemplate Not { get; set; }
        public DataTemplate Error { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var token = (ConditionToken)item;
            switch (token.TokenType)
            {
                case TokenType.Neutral: return Neutral;
                case TokenType.Middle: return Middle;
                case TokenType.Begin: return Begin;
                case TokenType.Single: return Single;
                case TokenType.End: return End;
                case TokenType.Not: return Not;
                case TokenType.Error: return Error;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
