using System.Threading.Tasks;

namespace Parser.FlowParser.ActionExecutors
{
    public interface IScopeActionExecutor
    {
        public Task<ActionResult> ExitScope(ActionStatus scopeStatus);
    }
}