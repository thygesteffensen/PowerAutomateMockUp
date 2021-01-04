using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class DoUntilActionExecutor : DefaultBaseActionExecutor, IScopeActionExecutor
    {
        private readonly IScopeDepthManager _scopeDepthManager;
        private readonly ILogger<DoUntilActionExecutor> _logger;
        private readonly IExpressionEngine _expressionEngine;

        private JProperty[] _actionDescriptions;
        private string _firstScopeActionName;
        private string _expressionString;
        private int _countLimit;
        private int _counts = 0;

        public DoUntilActionExecutor(
            IScopeDepthManager scopeDepthManager,
            ILogger<DoUntilActionExecutor> logger,
            IExpressionEngine expressionEngine)
        {
            _scopeDepthManager = scopeDepthManager ?? throw new ArgumentNullException(nameof(scopeDepthManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        public override Task<ActionResult> Execute()
        {
            _logger.LogInformation("Entered do until...");

            if (Json == null)
            {
                throw new PowerAutomateMockUpException($"Json cannot be null - cannot execute {ActionName}.");
            }

            SetupDoUntil();

            if (!EvaluateExpressionResult())
            {
                return Task.FromResult(new ActionResult());
            }

            PushActions();

            return Task.FromResult(new ActionResult {NextAction = _firstScopeActionName});
        }

        private void SetupDoUntil()
        {
            _expressionString = (Json.SelectToken("$..expression") ??
                                 throw new InvalidOperationException("Json must contain expression token."))
                .Value<string>();

            _countLimit = (Json.SelectToken("$..count")
                           ?? throw new InvalidOperationException("Json must contain count token.")
                ).Value<int>();

            var scopeActionDescriptions = (Json.SelectToken("$..actions") ??
                                           throw new InvalidOperationException("Json must contain actions token."))
                .OfType<JProperty>();
            _actionDescriptions = scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();
            _firstScopeActionName = _actionDescriptions.First(ad =>
                !(ad.Value.SelectToken("$.runAfter") ??
                  throw new InvalidOperationException("Json must contain runAfter token.")).Any()).Name;
        }

        private void PushActions()
        {
            _scopeDepthManager.Push(ActionName, _actionDescriptions, this);
        }

        public Task<ActionResult> ExitScope(ActionStatus scopeStatus)
        {
            _counts++;
            if (_counts >= _countLimit)
            {
                _logger.LogInformation($"Exited do until loop due to count limit of {_counts}...");
                return Task.FromResult(new ActionResult());
            }

            if (EvaluateExpressionResult())
            {
                _logger.LogInformation("Continuing do until loop.");

                PushActions();

                return Task.FromResult(new ActionResult {NextAction = _firstScopeActionName});
            }
            else
            {
                _logger.LogInformation("Exited do until loop...");
                return Task.FromResult(new ActionResult());
            }
        }

        private bool EvaluateExpressionResult()
        {
            var expressionResult = _expressionEngine.ParseToValueContainer(_expressionString);
            if (expressionResult.Type() != ValueContainer.ValueType.Boolean)
            {
                throw new PowerAutomateMockUpException("Do until expression did not resolve to boolean type.");
            }

            return expressionResult.GetValue<bool>();
        }
    }
}