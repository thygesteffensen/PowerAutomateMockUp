using System.Linq;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public class ConcatFunction : Function
    {
        public ConcatFunction() : base("concat")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ArgumentError("Too few arguments");
            }

            return new ValueContainer(parameters.Aggregate("", (current, value) => current + AuxiliaryMethods.VcIsString(value)));
        }
    }
}