using System.Collections.Generic;

namespace Parser.FlowParser
{
    public class FlowSettings
    {
        public bool FailOnUnknownAction { get; set; } = true;

        public List<string> IgnoreActions { get; set; } = new List<string>();

        public bool LogActionsStates { get; set; } = true;
    }
}