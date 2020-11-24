using System;

namespace Parser.ExpressionParser.Functions.Base
{
    public class ActionExecutorRegistration
    {
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public string ActionApiId { get; set; }
        // TODO: Discuss with PKS whether this makes sense or not
        public string[] SupportedOperationNames { get; set; }
        public Type Type { get; set; }
    }
}