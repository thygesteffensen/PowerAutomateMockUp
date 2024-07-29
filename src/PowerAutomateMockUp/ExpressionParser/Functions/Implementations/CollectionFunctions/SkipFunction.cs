using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.CollectionFunctions
{
    public class SkipFunction : Function
    {
        public SkipFunction() : base("skip")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            var value = parameters[0];
            var count = parameters[1].GetValue<int>();

            return value.Type() switch
            {
                ValueContainer.ValueType.Array => new ValueContainer(
                    value.GetValue<IEnumerable<ValueContainer>>().Skip(count)),
                _ => throw new PowerAutomateMockUpException(
                    $"Empty expression can only operate on String or Array types, not {value.Type()}.")
            };
        }
    }
}