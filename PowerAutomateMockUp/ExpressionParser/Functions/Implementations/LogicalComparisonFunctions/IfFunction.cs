using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions
{
    public class IfFunction : Function
    {
        public IfFunction() : base("if")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].GetValue<bool>() ? parameters[1] : parameters[2];
        }
    }
}