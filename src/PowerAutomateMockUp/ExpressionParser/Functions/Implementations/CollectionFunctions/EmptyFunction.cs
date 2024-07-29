using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.CollectionFunctions
{
    public class EmptyFunction : Function
    {
        public EmptyFunction() : base("empty")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            var value = parameters[0];

            return value.Type() switch
            {
                ValueContainer.ValueType.String => new ValueContainer(string.IsNullOrEmpty(value.GetValue<string>())),
                ValueContainer.ValueType.Array => new ValueContainer(
                    !value.GetValue<IEnumerable<ValueContainer>>().Any()),
                ValueContainer.ValueType.Object => new ValueContainer(
                    value.GetValue<Dictionary<string, ValueContainer>>().Count == 0),
                ValueContainer.ValueType.Null => new ValueContainer(true),
                _ => throw new PowerAutomateMockUpException(
                    $"Empty expression can only operate on String, Array or Object types, not {value.Type()}.")
            };
        }
    }
}