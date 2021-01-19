using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Parser.FlowParser.ActionExecutors;

namespace Parser.ExpressionParser.Functions.Base
{
    public class ActionExecutorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ActionExecutorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ActionExecutorBase ResolveActionByKey(string key)
        {
            var axRegistrationList = _serviceProvider.GetRequiredService<IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList.LastOrDefault(x => x.ActionName == key);
            return axRegistration == null
                ? null
                : _serviceProvider.GetRequiredService(axRegistration.Type) as ActionExecutorBase;
        }

        public ActionExecutorBase ResolveActionByType(string actionType)
        {
            var axRegistrationList = _serviceProvider.GetRequiredService<IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList.LastOrDefault(x => x.ActionType == actionType);
            return axRegistration == null
                ? null
                : _serviceProvider.GetRequiredService(axRegistration.Type) as ActionExecutorBase;
        }

        public ActionExecutorBase ResolveActionByApiId(string apiId, string operationName)
        {
            var axRegistrationList = _serviceProvider.GetRequiredService<IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList.LastOrDefault(
                x => x.ActionApiId == apiId && x.SupportedOperationNames.Contains(operationName));
            return axRegistration == null
                ? null
                : _serviceProvider.GetRequiredService(axRegistration.Type) as ActionExecutorBase;
        }
    }
}