using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class SwitchActionExecutor : DefaultBaseActionExecutor
    {
        private readonly IScopeDepthManager _scopeDepthManager;
        private readonly ILogger<SwitchActionExecutor> _logger;
        private readonly IExpressionEngine _expressionEngine;

        public SwitchActionExecutor(
            IScopeDepthManager scopeDepthManager,
            ILogger<SwitchActionExecutor> logger,
            IExpressionEngine expressionEngine)
        {
            _scopeDepthManager = scopeDepthManager ?? throw new ArgumentNullException(nameof(scopeDepthManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        public override Task<ActionResult> Execute()
        {
            _logger.LogInformation("Entered switch...");

            if (Json == null)
            {
                throw new PowerAutomateMockUpException($"Json cannot be null - cannot execute {ActionName}.");
            }

            var expression = (Json.SelectToken("$..expression") ??
                              throw new InvalidOperationException("Json must contain foreach token.")).Value<string>();

            var expressionValue = _expressionEngine.Parse(expression);

            var cases = Json.SelectToken("$..cases")?.Children() ??
                        throw new InvalidOperationException("Json must contain cases token.");

            foreach (var switchCase in cases)
            {
                var caseValue = switchCase.SelectToken("$..case")?.Value<string>() ??
                                throw new InvalidOperationException("Json must contain case token.");

                if (caseValue.Equals(expressionValue))
                {
                    return ExecuteActions(switchCase);
                }
            }

            var defaultActions = Json.SelectToken("$..default");

            return ExecuteActions(defaultActions);
        }

        private Task<ActionResult> ExecuteActions(JToken actions)
        {
            var scopeActionDescriptions = (actions.SelectToken("$..actions") ??
                                           throw new InvalidOperationException(
                                               "Json must contain actions token.")).OfType<JProperty>();

            var actionDescriptions =
                scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();

            if (!actionDescriptions.Any())
            {
                return Task.FromResult(new ActionResult());
            }

            var firstScopeActionName = actionDescriptions.First(ad =>
                !(ad.Value.SelectToken("$.runAfter") ??
                  throw new InvalidOperationException("Json must contain runAfter token.")).Any()).Name;

            _scopeDepthManager.Push(ActionName, actionDescriptions, null);

            return Task.FromResult(new ActionResult {NextAction = firstScopeActionName});
        }
    }
}