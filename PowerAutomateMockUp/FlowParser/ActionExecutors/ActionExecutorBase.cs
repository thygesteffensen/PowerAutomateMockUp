using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class ActionExecutorBase
    {
        protected JToken Json { get; set; }

        public string ActionName { get; private set; }

        public ValueContainer Inputs { get; protected set; }

        public void InitializeActionExecutor(string actionName, JToken json)
        {
            ActionName = actionName;
            Json = json;

            ProcessJson();
        }
        protected abstract void ProcessJson();

        public abstract Task<ActionResult> Execute();
    }
}