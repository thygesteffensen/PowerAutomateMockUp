#nullable enable
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Parser.FlowParser
{
    public class ActionReport
    {
#nullable enable
        public ValueContainer? ActionInput { get; set; }

        public ActionResult? ActionOutput { get; set; }
#nullable disable
        public string ActionName { get; set; }
        public int ActionOrder { get; set; }
        public JToken ActionJson { get; set; }
    }
}