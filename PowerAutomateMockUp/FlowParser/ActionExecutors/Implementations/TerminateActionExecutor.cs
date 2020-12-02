using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parser.ExpressionParser;
using Parser.FlowParser.CustomExceptions;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class TerminateActionExecutor : DefaultBaseActionExecutor
    {
        public override Task<ActionResult> Execute()
        {
            var runStatus = Inputs["runStatus"].GetValue<string>();

            switch (runStatus)
            {
                case "Succeeded":
                case "Cancelled":
                    return Task.FromResult(new ActionResult
                    {
                        ActionStatus = ActionStatus.Succeeded,
                        ContinueExecution = false
                    });
                case "Failed":
                    var exceptionMessage = "Terminate action with status: Failed.";
                    
                    if (!Inputs.GetValue<Dictionary<string, ValueContainer>>()
                        .TryGetValue("runError", out var runErrorDict)) throw new FlowRunnerException(exceptionMessage);
                    
                    var dict = runErrorDict.GetValue<Dictionary<string, ValueContainer>>();

                    if (dict.TryGetValue("code", out var code))
                    {
                        exceptionMessage += $" Error code: {code}.";
                    }

                    if (dict.TryGetValue("message", out var message))
                    {
                        exceptionMessage += $" Error code: {message}.";
                    }

                    throw new FlowRunnerException(exceptionMessage);
                default:
                    throw new Exception($"Unknown runStatus: {runStatus}");
            }
        }
    }
}