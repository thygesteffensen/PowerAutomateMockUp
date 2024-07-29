using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class DataUriFunction : Function
    {
        public DataUriFunction() : base("dataUri")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length == 0)
            {
                throw InvalidTemplateException.BuildInvalidLanguageFunction("SomeActon", "dataUri");
            }
            
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