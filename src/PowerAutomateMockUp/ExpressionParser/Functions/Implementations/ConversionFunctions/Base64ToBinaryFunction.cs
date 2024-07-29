using System;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class Base64ToBinaryFunction : Function
    {
        public Base64ToBinaryFunction() : base("base64ToBinary")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.String =>
                    new ValueContainer(Convert.FromBase64String(parameters[0].GetValue<string>())
                        .Aggregate("", (s, b) => s + Convert.ToString(b, 2).PadLeft(8, '0'))),
                _ => throw new PowerAutomateMockUpException(
                    $"Array function can only operate on strings, not {parameters[0].Type()}.")
            };
        }
    }
}