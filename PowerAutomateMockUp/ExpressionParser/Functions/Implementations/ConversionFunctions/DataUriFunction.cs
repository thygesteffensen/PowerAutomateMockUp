using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class DataUriFunction : Function
    {
        public DataUriFunction() : base("dataUri")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.String =>
                    new ValueContainer(
                        $"data:text/plain;charset=utf-8;base64,{System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(parameters[0].GetValue<string>()))}"),
                _ => throw new PowerAutomateMockUpException(
                    $"Array function can only operate on strings, not {parameters[0].Type()}.")
            };
        }
    }
}