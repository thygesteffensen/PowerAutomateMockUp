using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser
{
    public class ScopeDepthManager
    {
        public IEnumerable<JProperty> CurrentActionDescriptions { get; private set; }

        private readonly Stack<string> _scopes;
        private readonly Stack<IEnumerable<JProperty>> _actionDescriptions;

        public ScopeDepthManager()
        {
            _actionDescriptions = new Stack<IEnumerable<JProperty>>();
            _scopes = new Stack<string>();
        }

        public void Push(string scopeName, IEnumerable<JProperty> scopeActionDescriptions)
        {
            var scopeActionDescriptionsAsList =
                scopeActionDescriptions as JProperty[] ?? scopeActionDescriptions.ToArray();

            if (CurrentActionDescriptions == null)
            {
                CurrentActionDescriptions = scopeActionDescriptionsAsList;
            }
            else
            {
                _actionDescriptions.Push(CurrentActionDescriptions);
                CurrentActionDescriptions = scopeActionDescriptionsAsList;

                _scopes.Push(scopeName);
            }
        }

        public bool TryPopScope(out string scopeName)
        {
#if DOT_NET_CORE
            var scopePopSuccessful = _scopes.TryPop(out scopeName);
#else
            var scopePopSuccessful = true;
            try
            {
                scopeName = _scopes.Pop();
            }
            catch (InvalidOperationException)
            {
                scopePopSuccessful = false;
                scopeName = null;
            }
#endif

            if (scopePopSuccessful)
            {
                CurrentActionDescriptions = _actionDescriptions.Pop();
            }

            return scopePopSuccessful;
        }
    }
}