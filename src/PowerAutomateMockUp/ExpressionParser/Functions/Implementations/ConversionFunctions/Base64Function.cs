using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class Base64Function : Function
    {
        public Base64Function() : base("base64")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.String =>
                    new ValueContainer(
                        System.Convert.ToBase64String(
                            System.Text.Encoding.UTF8.GetBytes(parameters[0].GetValue<string>()))),
                _ => throw new PowerAutomateMockUpException(
                    $"Array function can only operate on strings, not {parameters[0].Type()}.")
            };
        }
    }
}