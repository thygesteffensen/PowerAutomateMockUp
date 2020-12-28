using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.FlowParser.ActionExecutors;

namespace Parser
{
    public class ScopeDepthManager : IScopeDepthManager
    {
        private readonly ILogger<IScopeDepthManager> _logger;
        public IEnumerable<JProperty> CurrentActionDescriptions { get; private set; }

        private readonly Stack<string> _scopes;
        private readonly Stack<IScopeActionExecutor> _scopeActionExecutors;
        private readonly Stack<IEnumerable<JProperty>> _actionDescriptions;

        public ScopeDepthManager(ILogger<IScopeDepthManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _actionDescriptions = new Stack<IEnumerable<JProperty>>();
            _scopes = new Stack<string>();
            _scopeActionExecutors = new Stack<IScopeActionExecutor>();
        }

        public void Push(
            string scopeName,
            IEnumerable<JProperty> scopeActionDescriptions,
            IScopeActionExecutor scopeActionExecutor)
        {
            _logger.LogInformation("Entered a scope ({scopeName}).", scopeName);

            var scopeActionDescriptionsAsList =
                scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();

            if (CurrentActionDescriptions == null)
            {
                CurrentActionDescriptions = scopeActionDescriptionsAsList;
            }
            else
            {
                _actionDescriptions.Push(CurrentActionDescriptions);
                CurrentActionDescriptions = scopeActionDescriptionsAsList;

                _scopes.Push(scopeName);

                _scopeActionExecutors.Push(scopeActionExecutor);
            }
        }

        public async Task<ActionResult> TryPopScope(ActionStatus scopeStatus)
        {
            _logger.LogInformation("Exiting scope.");

#if DOT_NET_CORE
            var scopePopSuccessful = _scopes.TryPop(out var scopeName);
#else
            string scopeName;

            var scopePopSuccessful = true;
            try
            {
                scopeName = _scopes.Pop();
            }
            catch (InvalidOperationException)
            {
                scopePopSuccessful = false;
                scopeName = null;
            }
#endif

            // Todo: Finish the State Machine and figure out how this should be handled properly


            if (!scopePopSuccessful)
            {
                return null;
            }

            CurrentActionDescriptions = _actionDescriptions.Pop();

            var t = _scopeActionExecutors.Pop();

            ActionResult exitScopeResult = null;
            if (t == null)
            {
                _logger.LogInformation("Scope action does not have a exit scope action.");
            }
            else
            {
                exitScopeResult = await t.ExitScope(scopeStatus);
            }


            return await Task.FromResult(new ActionResult
            {
                ActionStatus = exitScopeResult?.ActionStatus ?? scopeStatus,
                NextAction = scopeName
            });
        }
    }
}