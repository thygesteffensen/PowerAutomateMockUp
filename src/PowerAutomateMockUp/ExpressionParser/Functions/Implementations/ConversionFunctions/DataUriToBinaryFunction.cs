using System;
using System.Linq;
using System.Text;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class DataUriToBinaryFunction : Function
    {
        public DataUriToBinaryFunction() : base("dataUriToBinary")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length == 0)
            {
                throw InvalidTemplateException.BuildInvalidLanguageFunction("SomeActon", "dataUriToBinary");
            }

            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.String =>
                    new ValueContainer(Encoding.UTF8.GetBytes(parameters[0].GetValue<string>())
                        .Aggregate("", (s, b) => s + Convert.ToString(b, 2).PadLeft(8, '0'))),
                _ => throw new PowerAutomateMockUpException(
                    $"Array function can only operate on strings, not {parameters[0].Type()}.")
            };
        }
    }
}