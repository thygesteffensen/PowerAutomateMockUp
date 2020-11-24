using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class ScopeActionExecutor : DefaultBaseActionExecutor
    {
        private readonly ScopeDepthManager _scopeDepthManager;

        public ScopeActionExecutor(ScopeDepthManager scopeDepthManager)
        {
            _scopeDepthManager = scopeDepthManager ?? throw new ArgumentNullException(nameof(scopeDepthManager));
        }

        public override Task<ActionResult> Execute()
        {
            var scopeActionJson = Json.Parent;
            var scopeName = ((JProperty) scopeActionJson).Name;

            var scopeActionDescriptions = Json.SelectToken("$.actions").OfType<JProperty>();
            var actionDescriptions = scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();
            
            _scopeDepthManager.Push(scopeName, actionDescriptions);

            var firstScopeAction = actionDescriptions.First(ad => !ad.Value.SelectToken("$.runAfter").Any());

            return Task.FromResult(new ActionResult {NextAction = firstScopeAction.Name});
        }
    }
}