using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations
{
    public class GreaterFunction : Function
    {
        public GreaterFunction() : base("greater")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ArgumentError("Too few arguments");
            }
            
            return new ValueContainer(parameters[0].CompareTo(parameters[1]) > 0);
        }
    }
}