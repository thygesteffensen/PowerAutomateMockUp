﻿using System;
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

        private JProperty[] _actionDescriptions;
        private string _firstScopeActionName;

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

            if (Json == null)
            {
                throw new PowerAutomateMockUpException($"Json cannot be null - cannot execute {ActionName}.");
            }

            var runOn = (Json.SelectToken("$..foreach") ??
                         throw new InvalidOperationException("Json must contain foreach token.")).Value<string>();
            var values = _expressionEngine.ParseToValueContainer(runOn);

            if (values.Type() != ValueContainer.ValueType.Array)
                return Task.FromResult(new ActionResult
                {
                    // TODO: Figure out what happens when you apply for each on non array values
                    ActionStatus = ActionStatus.Failed
                });

            SetupForEach(values);

            UpdateScopeAndSetItemValue();

            return Task.FromResult(new ActionResult {NextAction = _firstScopeActionName});
        }

        private void SetupForEach(ValueContainer values)
        {
            var scopeActionDescriptions = (Json.SelectToken("$.actions") ??
                                           throw new InvalidOperationException("Json must contain actions token."))
                .OfType<JProperty>();
            _actionDescriptions = scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();
            _firstScopeActionName = _actionDescriptions.First(ad =>
                !(ad.Value.SelectToken("$.runAfter") ??
                  throw new InvalidOperationException("Json must contain runAfter token.")).Any()).Name;

            // TODO: Add scope relevant storage to store stuff like this, which cannot interfere with the state.
            _items = values.GetValue<IEnumerable<ValueContainer>>().ToList();
        }

        private void UpdateScopeAndSetItemValue()
        {
            _scopeDepthManager.Push(ActionName, _actionDescriptions, this);

            _state.AddOutputs($"item_{ActionName}", _items.First());
            _items = _items.Skip(1).ToList();
        }


        public Task<ActionResult> ExitScope(ActionStatus scopeStatus)
        {
            if (_items.Count > 0)
            {
                _logger.LogInformation("Continuing foreach.");

                UpdateScopeAndSetItemValue();

                return Task.FromResult(new ActionResult {NextAction = _firstScopeActionName});
            }
            else
            {
                _logger.LogInformation("Exited foreach...");
                return Task.FromResult(new ActionResult());
            }
        }
    }
}