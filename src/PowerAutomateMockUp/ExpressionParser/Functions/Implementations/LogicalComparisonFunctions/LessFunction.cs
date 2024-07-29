using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions
{
    public class LessFunction : Function
    {
        public LessFunction() : base("less")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ArgumentError("Too few arguments");
            }
            
            return new ValueContainer(parameters[0].CompareTo(parameters[1]) < 0);
        }
    }
}