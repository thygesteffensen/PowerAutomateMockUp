using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions
{
    public class NotFunction : Function
    {
        public NotFunction() : base("not")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 1)
            {
                throw new ArgumentError("Too few arguments");
            }
            
            return new ValueContainer(!parameters[0].GetValue<bool>());
        }
    }
}