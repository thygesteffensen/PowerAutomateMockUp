using System.Linq;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.ConversionFunctions
{
    public class CreateArrayFunction : Function
    {
        public CreateArrayFunction() : base("createArray")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length == 0)
            {
                throw InvalidTemplateException.BuildInvalidLanguageFunction("SomeActon", "createArray");
            }

            return new ValueContainer(parameters.ToList());
        }
    }
}