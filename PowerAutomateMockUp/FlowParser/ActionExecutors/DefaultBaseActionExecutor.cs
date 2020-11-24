using System;
using Newtonsoft.Json.Linq;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class DefaultBaseActionExecutor : ActionExecutorBase
    {
        public override void AddJson(JToken json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
        }
    }
}