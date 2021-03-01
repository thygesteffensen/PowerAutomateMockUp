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
        private ValueContainer? _actionInput;
#nullable enable
        public ValueContainer? ActionInput
        {
            get => _actionInput;
            set
            {
                _actionInput = value;
                if (_actionInput == null || _actionInput.Type() != ValueContainer.ValueType.Object) return;
                if (_actionInput.AsDict().ContainsKey("parameters"))
                {
                    ActionParameters = _actionInput.AsDict()["parameters"];
                }
            }
        }

        public ValueContainer? ActionParameters { set; get; }

        public ActionResult? ActionOutput { get; set; }
#nullable disable
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public int ActionOrder { get; set; }
    }
}