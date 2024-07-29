using System;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Storage
{
    public class TriggerOutputsFunctions : Function
    {
        private readonly ITriggerOutputsRetriever _variableRetriever;

        public TriggerOutputsFunctions(ITriggerOutputsRetriever triggerOutputsRetriever) : base("triggerOutputs")
        {
            _variableRetriever = triggerOutputsRetriever ??
                                 throw new ArgumentNullException(nameof(triggerOutputsRetriever));
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                throw new ArgumentError("Too many arguments");
            }

            var value = _variableRetriever.GetTriggerOutputs();

            return value;
        }
    }
}