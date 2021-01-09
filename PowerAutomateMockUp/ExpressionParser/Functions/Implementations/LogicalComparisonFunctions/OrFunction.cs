using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions
{
    public class OrFunction : Function
    {
        public OrFunction() : base("or")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return new ValueContainer(parameters.Any(x => x.GetValue<bool>()));
        }
    }
}