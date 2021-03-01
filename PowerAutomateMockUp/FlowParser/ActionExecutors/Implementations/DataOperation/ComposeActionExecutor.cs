using System;
using System.Threading.Tasks;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations.DataOperation
{
    public class ComposeActionExecutor : DefaultBaseActionExecutor
    {
        private readonly IExpressionEngine _expressionEngine;

        public ComposeActionExecutor(IExpressionEngine expressionEngine)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        public override Task<ActionResult> Execute()
        {
            var param1 = Inputs.GetValue<string>();

            var result = _expressionEngine.ParseToValueContainer(param1);

            return Task.FromResult(new ActionResult
            {
                ActionStatus = ActionStatus.Succeeded,
                ActionOutput = result
            });
        }
    }
}