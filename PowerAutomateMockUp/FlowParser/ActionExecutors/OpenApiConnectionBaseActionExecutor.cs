using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class OpenApiConnectionActionExecutorBase : ActionExecutorBase
    {
        private readonly IExpressionEngine _expressionEngine;

        protected OpenApiConnectionActionExecutorBase(IExpressionEngine expressionEngine)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        protected override void ProcessJson()
        {
            var type = Json.SelectToken("$.type").Value<string>();

            if (type != "OpenApiConnection")
                throw new InvalidOperationException(
                    "The expected Action Description type was OpenApiConnection, but the " +
                    $"Action Description type was {type}.");

            var inputs = Json.SelectToken("$.inputs");
            var content = inputs.ToObject<ActionInputs>();
            Host = content.HostValues;
            Parameters = new ValueContainer(new Dictionary<string, ValueContainer>());
            foreach (var keyValuePar in content.Parameters)
            {
                Parameters[keyValuePar.Key] = _expressionEngine.ParseToValueContainer(keyValuePar.Value);
            }
        }

        protected HostValues Host { get; set; }

        protected ValueContainer Parameters { get; set; }
    }

    internal class ActionInputs
    {
        [JsonProperty("host")]
        internal HostValues HostValues { get; set; }

        [JsonProperty("parameters")]
        internal Dictionary<string, string> Parameters;
    }

    public class HostValues
    {
        [JsonProperty("apiId")]
        public string ApiId { get; set; }

        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }
    }
}