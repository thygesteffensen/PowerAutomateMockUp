#nullable enable
using System.Collections.Generic;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Parser.FlowParser
{
    public class FlowResult
    {
        public FlowResult()
        {
            ActionStates = new Dictionary<string, ActionState>();
        }

        public Dictionary<string, ActionState> ActionStates { get; set; }
        public int NumberOfExecutedActions { get; set; }
    }

    public class ActionState
    {
#nullable enable
        public ValueContainer? ActionInput { get; set; }

        public ActionResult? ActionOutput { get; set; }
#nullable disable
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public int ActionOrder { get; set; }
    }
}