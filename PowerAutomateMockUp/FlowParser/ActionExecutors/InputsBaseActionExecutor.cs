using System;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class InputsBaseActionExecutor : ActionExecutorBase
    {
        private readonly IExpressionEngine _expressionEngine;

        protected InputsBaseActionExecutor(IExpressionEngine expressionEngine)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        protected override void ProcessJson()
        {
            var inputs = Json.SelectToken("$.inputs");
            if (inputs != null)
            {
                Inputs = new ValueContainer(inputs, _expressionEngine);
            }
        }
    }
}