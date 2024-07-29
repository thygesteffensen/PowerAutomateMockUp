using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Parser.FlowParser.ActionExecutors;

namespace Parser
{
    public interface IScopeDepthManager
    {
        Task<ActionResult> TryPopScope(ActionStatus scopeStatus);

        void Push(
            string scopeName,
            IEnumerable<JProperty> scopeActionDescriptions,
            IScopeActionExecutor scopeActionExecutor);

        IEnumerable<JProperty> CurrentActionDescriptions { get; }
    }
}