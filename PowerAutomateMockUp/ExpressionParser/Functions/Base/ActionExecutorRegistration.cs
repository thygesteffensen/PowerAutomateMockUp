using System;

namespace Parser.ExpressionParser.Functions.Base
{
    public class ActionExecutorRegistration
    {
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public string ActionApiId { get; set; }
        public string[] SupportedOperationNames { get; set; }
        public Type Type { get; set; }
    }
}