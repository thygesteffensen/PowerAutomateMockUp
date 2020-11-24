using System;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Storage
{
    public class VariablesFunction : Function
    {
        private readonly IState _variableRetriever;

        // public Variables(IState variableRetriever) : base("variables")
        public VariablesFunction(IState variableRetriever) : base("variables")
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
            var value = _variableRetriever.GetVariable(variableName);

            return value;
        }
    }
}