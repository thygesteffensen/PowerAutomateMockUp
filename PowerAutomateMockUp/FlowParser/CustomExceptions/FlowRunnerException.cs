using System;

namespace Parser.FlowParser.CustomExceptions
{
    public class FlowRunnerException : Exception
    {
        public FlowRunnerException(string message) : base(message)
        {
        }
    }
}