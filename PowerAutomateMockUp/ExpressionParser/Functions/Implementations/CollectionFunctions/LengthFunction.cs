using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.CollectionFunctions
{
    public class LengthFunction : Function
    {
        public LengthFunction() : base("length")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            var value = parameters[0];

            return value.Type() switch
            {
                ValueContainer.ValueType.String => new ValueContainer(
                    value.GetValue<string>().Length),
                ValueContainer.ValueType.Array => new ValueContainer(
                    value.GetValue<IEnumerable<ValueContainer>>().Count()),
                _ => throw new PowerAutomateMockUpException(
                    $"Empty expression can only operate on String or Array types, not {value.Type()}.")
            };
        }
    }
}