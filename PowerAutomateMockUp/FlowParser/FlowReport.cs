using System.Collections.Generic;

namespace Parser.FlowParser
{
    public class FlowReport
    {
        public FlowReport()
        {
            ActionStates = new Dictionary<string, ActionReport>();
        }

        public Dictionary<string, ActionReport> ActionStates { get; set; }
        public int NumberOfExecutedActions { get; set; }
    }
}