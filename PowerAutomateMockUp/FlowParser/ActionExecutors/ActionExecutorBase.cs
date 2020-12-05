using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class ActionExecutorBase
    {
        protected JToken Json { get; set; }

        public string ActionName { get; private set; }

        public ValueContainer Inputs { get; private set; }

        public void InitializeActionExecutor(string actionName, JToken json)
        {
            ActionName = actionName;
            Json = json;

            var inputs = Json.SelectToken("$.inputs");
            if (inputs != null)
            {
                Inputs = new ValueContainer(inputs);
            }

            ProcessJson();
        }
        protected abstract void ProcessJson();

        public abstract Task<ActionResult> Execute();
    }
}