using System;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Storage
{
    public class OutputsFunction : Function
    {
        private readonly IOutputsRetriever _variableRetriever;

        public OutputsFunction(IOutputsRetriever variableRetriever) : base("outputs")
        {
            _variableRetriever = variableRetriever ?? throw new ArgumentNullException(nameof(variableRetriever));
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentError(parameters.Length > 1 ? "Too many arguments" : "Too few arguments");
            }

            var actionName = parameters[0].GetValue<string>();

            var value = _variableRetriever.GetOutputs(actionName);

            return value;
        }
    }
}