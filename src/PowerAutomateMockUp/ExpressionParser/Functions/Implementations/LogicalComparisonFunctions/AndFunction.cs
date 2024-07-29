using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions
{
    public class AndFunction : Function
    {
        public AndFunction() : base("and")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return new ValueContainer(parameters.All(x => x.GetValue<bool>()));
        }
    }
}