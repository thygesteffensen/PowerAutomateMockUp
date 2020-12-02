﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class OpenApiConnectionActionExecutorBase : ActionExecutorBase
    {
        private readonly ExpressionEngine _expressionEngine;

        protected OpenApiConnectionActionExecutorBase(ExpressionEngine expressionEngine)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        public override void AddJson(JToken json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
            var type = json.SelectToken("$.type").Value<string>();

            if (type != "OpenApiConnection")
                throw new InvalidOperationException(
                    "The expected Action Description type was OpenApiConnection, but the " +
                    $"Action Description type was {type}.");

            var inputs = Json.SelectToken("$..inputs");
            var content = inputs.ToObject<ActionInputs>();
            Host = content.HostValues;
            Parameters = new ValueContainer(new Dictionary<string, ValueContainer>());
            foreach (var keyValuePar in content.Parameters)
            {
                // TODO: Here is an use case where the engine could have just returned a Value Container instead!
                Parameters[keyValuePar.Key] = new ValueContainer(_expressionEngine.Parse(keyValuePar.Value));
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