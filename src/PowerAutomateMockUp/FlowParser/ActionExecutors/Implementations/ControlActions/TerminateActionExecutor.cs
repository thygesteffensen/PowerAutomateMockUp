using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations.ControlActions
{
    public class TerminateActionExecutor : InputsBaseActionExecutor
    {
        private readonly ILogger<TerminateActionExecutor> _logger;

        public TerminateActionExecutor(ILogger<TerminateActionExecutor> logger, IExpressionEngine expressionEngine) :
            base(expressionEngine)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<ActionResult> Execute()
        {
            var runStatus = Inputs["runStatus"].GetValue<string>();

            switch (runStatus)
            {
                case "Succeeded":
                case "Cancelled":
                    _logger.LogInformation("Terminate Action {ActionName} reached with status {RunStatus}",
                        ActionName, runStatus);
                    return Task.FromResult(new ActionResult
                    {
                        ActionStatus = ActionStatus.Succeeded,
                        ContinueExecution = false
                    });
                case "Failed":
                    var exceptionMessage = "Terminate action with status: Failed.";

                    if (!Inputs.GetValue<Dictionary<string, ValueContainer>>()
                        .TryGetValue("runError", out var runErrorDict))
                        throw new PowerAutomateMockUpException(exceptionMessage);

                    var dict = runErrorDict.GetValue<Dictionary<string, ValueContainer>>();

                    if (dict.TryGetValue("code", out var code))
                    {
                        exceptionMessage += $" Error code: {code}.";
                    }

                    if (dict.TryGetValue("message", out var message))
                    {
                        exceptionMessage += $" Error message: {message}.";
                    }

                    _logger.LogInformation("Terminate Action {ActionName} reached with status {RunStatus}. " +
                                           "Error code: {Code}. Error message: {Message}. Throwing exception",
                        ActionName, runStatus, code, message);

                    throw new PowerAutomateMockUpException(exceptionMessage);
                default:
                    throw new Exception($"Unknown runStatus: {runStatus}");
            }
        }
    }
}