using System;
using System.Windows.Markup;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class EnumToBooleanExtension : MarkupExtension
    {
        [ConstructorArgument("value")]
        public Enum Value { get; set; }

        public EnumToBooleanExtension(Enum value)
        {
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new EnumToBooleanConverter(Value);
        }
    }
}
