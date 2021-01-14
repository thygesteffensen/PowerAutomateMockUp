using System;
using System.Linq;
using System.Text;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class Base64ToStringFunction : Function
    {
        public Base64ToStringFunction() : base("base64ToString")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.String =>
                    new ValueContainer(Encoding.UTF8.GetString(Convert.FromBase64String(parameters[0].GetValue<string>()))),
                _ => throw new PowerAutomateMockUpException(
                    $"Array function can only operate on strings, not {parameters[0].Type()}.")
            };
        }
    }
}