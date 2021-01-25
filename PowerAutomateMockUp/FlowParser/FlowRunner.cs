using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser.ActionExecutors;

namespace Parser.FlowParser
{
    public interface IFlowRunner
    {
        void InitializeFlowRunner(in string path);
        Task Trigger();
    }

    public class FlowRunner : IFlowRunner
    {
        private readonly IState _state;
        private readonly FlowSettings _flowRunnerSettings;
        private readonly IScopeDepthManager _scopeManager;
        private readonly IActionExecutorFactory _actionExecutorFactory;
        private readonly ILogger<FlowRunner> _logger;
        private JProperty _trigger;

        public FlowRunner(
            IState state,
            IScopeDepthManager scopeDepthManager,
            IOptions<FlowSettings> flowRunnerSettings,
            IActionExecutorFactory actionExecutorFactory,
            ILogger<FlowRunner> logger)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _scopeManager = scopeDepthManager;
            _flowRunnerSettings = flowRunnerSettings?.Value;
            _actionExecutorFactory =
                actionExecutorFactory ?? throw new ArgumentNullException(nameof(actionExecutorFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void InitializeFlowRunner(in string path)
        {
            using var streamReader = new StreamReader(path);
            using var jsonTextReader = new JsonTextReader(streamReader);

            var flowJson = JToken.ReadFrom(jsonTextReader);
            var flowDefinition = flowJson.SelectToken("$.*.definition");
            _trigger = flowDefinition.SelectToken("$.triggers").First as JProperty;
            _scopeManager.Push("root", flowDefinition.SelectToken("$.actions").OfType<JProperty>(), null);
        }

        public async Task Trigger()
        {
            var trigger = GetActionExecutor(_trigger);

            trigger.InitializeActionExecutor(_trigger.Name, _trigger.Value);
            var triggerResult = await trigger.Execute();

            if (triggerResult.ActionOutput != null)
            {
                _state.AddTriggerOutputs(triggerResult.ActionOutput);
            }

            await RunFlow();
        }

        private async Task RunFlow()
        {
            var currentAd = _scopeManager.CurrentActionDescriptions.First(
                ad => !ad.Value.SelectToken("$.runAfter").Any());

            while (currentAd != null)
            {
                if (_flowRunnerSettings.IgnoreActions.Contains(currentAd.Name))
                {
                    currentAd = _scopeManager.CurrentActionDescriptions.FirstOrDefault(a =>
                        a.Value.SelectToken("$.runAfter").First?.ToObject<JProperty>().Name == currentAd.Name
                        && a.Value.SelectToken("$.runAfter.*").Values().Any(
                            x => x?.Value<string>() == ActionStatus.Succeeded.ToString()));
                    continue;
                }

                var actionExecutor = GetActionExecutor(currentAd);

                var actionResult = await ExecuteAction(actionExecutor, currentAd);
                if (!(actionResult?.ContinueExecution ?? true))
                {
                    break;
                }

                // If an action failes inside a scope, and a suitable action isn't found inside the given scope, that 
                // actions status is transferred to be the scope status. This isn't the case atm

                var actionDescName = currentAd.Name;
                var nextAction = actionResult?.NextAction;
                var actionResultStatus = actionResult?.ActionStatus ?? ActionStatus.Succeeded;
                while (!DetermineNextAction(nextAction, actionResultStatus, out currentAd, actionDescName))
                {
                    nextAction = null;
                    var t = await _scopeManager.TryPopScope(actionResultStatus);
                    if (t == null)
                    {
                        currentAd = null;
                        break;
                    }

                    actionResultStatus = t.ActionStatus;
                    actionDescName = t.NextAction;
                }

                if (currentAd == null && actionResultStatus == ActionStatus.Failed)
                {
                    _logger.LogError(
                        "No succeeding action found after last action had status: Failed. Throwing error.");
                    throw actionResult?.ActionExecutorException ??
                          new PowerAutomateMockUpException(
                              $"No exception recorded - {actionExecutor.ActionName} ended with status: Failed.");
                }
            }
        }

        private bool DetermineNextAction(string nextAction, ActionStatus actionResultStatus,
            out JProperty currentActionDesc, string adName)
        {
            if (nextAction == null)
            {
                currentActionDesc = _scopeManager.CurrentActionDescriptions.FirstOrDefault(a =>
                    a.Value.SelectToken("$.runAfter").First?.ToObject<JProperty>().Name == adName &&
                    a.Value.SelectToken("$.runAfter.*").Values().Any(
                        x => x?.Value<string>() == actionResultStatus.ToString()));
            }
            else
            {
                currentActionDesc =
                    _scopeManager.CurrentActionDescriptions.First(a => a.Name == nextAction);
            }

            return currentActionDesc != null;
        }

        private async Task<ActionResult> ExecuteAction(ActionExecutorBase actionExecutor,
            JProperty currentAction)
        {
            if (actionExecutor == null) return null;

            actionExecutor.InitializeActionExecutor(currentAction.Name, currentAction.First);
            var executionResult = await actionExecutor.Execute();

            if (executionResult.ActionOutput != null)
            {
                _state.AddOutputs(actionExecutor.ActionName, executionResult.ActionOutput);
            }

            return executionResult;
        }

        private ActionExecutorBase GetActionExecutor(JProperty currentAction)
        {
            var actionTypeFromAd = currentAction.First.SelectToken("$.type");
            var actionType = actionTypeFromAd.Value<string>();

            var host = currentAction.First.SelectToken("$.inputs.host");

            var action = _actionExecutorFactory.ResolveActionByKey(currentAction.Name);

            var extraMessage = "";
            if (action == null && (actionType == "OpenApiConnection" || actionType == "OpenApiConnectionWebhook"))
            {
                var apiId = host.SelectToken("$.apiId").Value<string>();
                var operationId = host.SelectToken("$.operationId").Value<string>();
                action = _actionExecutorFactory.ResolveActionByApiId(apiId, operationId);
                extraMessage = $" , ApiId: {apiId} , OperationId: {operationId}";
            }

            action ??= _actionExecutorFactory.ResolveActionByType(actionTypeFromAd.Value<string>());

            if (action == null && _flowRunnerSettings.FailOnUnknownAction)
            {
                throw new Exception(
                    $"Could not find action to: {currentAction.Name} or by its type: {actionTypeFromAd}{extraMessage}. " +
                    "Register an Action either by Action Name or by its type in order to run this Flow."); // TODO: Create Exception
            }

            return action;
        }
    }
}