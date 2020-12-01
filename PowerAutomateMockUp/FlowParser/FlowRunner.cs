using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser.ActionExecutors;

namespace Parser.FlowParser
{
    public class FlowRunner
    {
        private readonly IState _state;
        private readonly FlowSettings _flowRunnerSettings;
        private readonly ScopeDepthManager _scopeManager;
        private readonly ActionExecutorFactory _actionExecutorFactory;
        private JProperty _trigger;

        public FlowRunner(
            IState state,
            ScopeDepthManager scopeDepthManager,
            IOptions<FlowSettings> flowRunnerSettings,
            ActionExecutorFactory actionExecutorFactory)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _scopeManager = scopeDepthManager;
            _flowRunnerSettings = flowRunnerSettings?.Value;
            _actionExecutorFactory =
                actionExecutorFactory ?? throw new ArgumentNullException(nameof(actionExecutorFactory));
        }

        public void InitializeFlowRunner(in string path)
        {
            using var streamReader = new StreamReader(path);
            using var jsonTextReader = new JsonTextReader(streamReader);

            var flowJson = JToken.ReadFrom(jsonTextReader);
            var flowDefinition = flowJson.SelectToken("$.*.definition");
            _trigger = flowDefinition.SelectToken("$.triggers").First as JProperty;
            _scopeManager.Push("root", flowDefinition.SelectToken("$.actions").OfType<JProperty>());
        }

        public async Task<object> Trigger(ValueContainer triggerOutputs)
        {
            _state.AddTriggerOutputs(triggerOutputs);
            // TODO: A option to handle action while execution
            await RunFlow();

            return null;
        }

        public async Task Trigger()
        {
            var trigger = GetActionExecutor(_trigger);

            trigger.AddJson(_trigger.Value);
            await trigger.Execute();

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
                        a.Value.SelectToken("$.runAfter").First?.ToObject<JProperty>().Name ==
                        currentAd.Name &&
                        a.Value.SelectToken("$.runAfter.*").Values().Any(
                            x => x?.Value<string>() == ActionStatus.Succeeded.ToString()));
                    continue;
                }

                var actionExecutor = GetActionExecutor(currentAd);

                var actionResult = await ExecuteAction(actionExecutor, currentAd);

                var actionDescName = currentAd.Name;
                while (!DetermineNextAction(actionResult, out currentAd, actionDescName))
                {
                    if (!_scopeManager.TryPopScope(out actionDescName))
                    {
                        break;
                    }
                }
            }
        }

        private bool DetermineNextAction(ActionResult actionResult, out JProperty currentActionDesc, string adName)
        {
            if (actionResult?.NextAction == null)
            {
                currentActionDesc = _scopeManager.CurrentActionDescriptions.FirstOrDefault(a =>
                    a.Value.SelectToken("$.runAfter").First?.ToObject<JProperty>().Name == adName &&
                    a.Value.SelectToken("$.runAfter.*").Values().Any(
                        x => x?.Value<string>() == actionResult?.ActionStatus.ToString()));
            }
            else
            {
                currentActionDesc =
                    _scopeManager.CurrentActionDescriptions.First(a => a.Name == actionResult.NextAction);
            }

            return currentActionDesc != null;
        }

        private static async Task<ActionResult> ExecuteAction(ActionExecutorBase actionExecutor,
            JToken currentAction)
        {
            if (actionExecutor == null) return null;

            actionExecutor.AddJson(currentAction.First);
            return await actionExecutor.Execute();
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

            action?.InitializeActionExecutor(currentAction.Name);
            return action;
        }
    }
}