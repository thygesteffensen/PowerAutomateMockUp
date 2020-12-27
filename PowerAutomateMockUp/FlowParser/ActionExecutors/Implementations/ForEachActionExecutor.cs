using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class ForEachActionExecutor : DefaultBaseActionExecutor, IScopeActionExecutor
    {
        private readonly IState _state;
        private readonly IScopeDepthManager _scopeDepthManager;
        private readonly ILogger<ForEachActionExecutor> _logger;
        private readonly IExpressionEngine _expressionEngine;

        private List<ValueContainer> _items;

        public ForEachActionExecutor(
            IState state,
            IScopeDepthManager scopeDepthManager,
            ILogger<ForEachActionExecutor> logger,
            IExpressionEngine expressionEngine)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _scopeDepthManager = scopeDepthManager ?? throw new ArgumentNullException(nameof(scopeDepthManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));

            _items = new List<ValueContainer>();
        }

        public override Task<ActionResult> Execute()
        {
            _logger.LogInformation("Entered foreach...");

            var runOn = Json.SelectToken("$..foreach").Value<string>();
            var values = _expressionEngine.ParseToValueContainer(runOn);

            if (values.Type() != ValueContainer.ValueType.Array)
                return Task.FromResult(new ActionResult
                {
                    // TODO: Figure out what happens when you apply for each on non array values
                    ActionStatus = ActionStatus.Failed
                });
            
            var valueList = values.GetValue<IEnumerable<ValueContainer>>().ToArray();
                
            var firstScopeAction = SetupScope();

            // TODO: Add scope relevant storage to store stuff like this, which cannot interfere with the state.
            _state.AddOutputs($"item_{ActionName}", valueList.First());
            _items = valueList.Skip(1).ToList();

            return Task.FromResult(new ActionResult {NextAction = firstScopeAction.Name});
        }

        private JProperty SetupScope()
        {
            var scopeActionDescriptions = Json.SelectToken("$.actions").OfType<JProperty>();
            var actionDescriptions = scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();

            _scopeDepthManager.Push(ActionName, actionDescriptions, this);

            var firstScopeAction = actionDescriptions.First(ad => !ad.Value.SelectToken("$.runAfter").Any());
            return firstScopeAction;
        }

        public Task<ActionResult> ExitScope(ActionStatus scopeStatus)
        {
            if (_items.Count > 0)
            {
                _logger.LogInformation("Continuing foreach.");

                _state.AddOutputs($"item_{ActionName}", _items.First());

                _items = _items.Skip(1).ToList();
                
                var firstScopeAction = SetupScope();

                return Task.FromResult(new ActionResult {NextAction = firstScopeAction.Name});
            }
            else
            {
                _logger.LogInformation("Exited foreach...");
                return Task.FromResult(new ActionResult());
            }
        }
    }
}