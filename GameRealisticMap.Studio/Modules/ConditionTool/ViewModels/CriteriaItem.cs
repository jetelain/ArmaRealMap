using System;
using System.Windows.Media;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal class CriteriaItem
    {
        public CriteriaItem(string context, string name, Type propertyType)
        {
            Name = name;
            IsBoolean = propertyType == typeof(bool);
            Color = ConditionToken.GetColor(name);
            Description = Labels.ResourceManager.GetString("Criteria" + context + name) ?? Labels.ResourceManager.GetString("Criteria" + name) ?? string.Empty;

            InitText = string.Empty;
            if (!IsBoolean)
            {
                if (propertyType == typeof(float))
                {
                    InitText = " > 10";
                }
                else if (propertyType == typeof(string))
                {
                    InitText = " == 'value'";
                }
            }
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsBoolean { get; }

        public Color Color { get; }

        public Brush Brush => new SolidColorBrush(Color);

        public string InitText { get; }
    }
}