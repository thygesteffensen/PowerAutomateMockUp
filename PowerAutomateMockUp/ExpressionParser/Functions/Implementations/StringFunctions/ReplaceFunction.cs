using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public class ReplaceFunction : Function
    {
        public ReplaceFunction() : base("replace")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new ArgumentError("Too few arguments");
            }

            var text = AuxiliaryMethods.VcIsString(parameters[0]);
            var oldStr = AuxiliaryMethods.VcIsString(parameters[1]);
            var newStr = AuxiliaryMethods.VcIsString(parameters[2]);

            return new ValueContainer(text.Replace(oldStr, newStr));
        }
    }
}