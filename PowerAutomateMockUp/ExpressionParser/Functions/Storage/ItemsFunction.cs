using System;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Storage
{
    public class ItemsFunction : Function
    {
        private readonly IItemsRetriever _variableRetriever;

        public ItemsFunction(IItemsRetriever variableRetriever) : base("items")
        {
            _variableRetriever = variableRetriever ?? throw new ArgumentNullException(nameof(variableRetriever));
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentError(parameters.Length > 1 ? "Too many arguments" : "Too few arguments");
            }

            var variableName = parameters[0].GetValue<string>();
            
            var value = _variableRetriever.GetCurrentItem(variableName);

            return value;
        }
    }
}