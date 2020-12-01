﻿using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Parser.FlowParser.ActionExecutors
{
    public abstract class ActionExecutorBase
    {
        protected JToken Json { get; set; }

        public string ActionName { get; private set; }

        public void InitializeActionExecutor(string actionName)
        {
            ActionName = actionName;
        }

        public abstract void AddJson(JToken json);

        public abstract Task<ActionResult> Execute();
    }
}