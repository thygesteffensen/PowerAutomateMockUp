using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Parser.FlowParser.ActionExecutors
{
    public interface IActionExecutorFactory
    {
        ActionExecutorBase ResolveActionByKey(string key);
        ActionExecutorBase ResolveActionByType(string actionType);
        ActionExecutorBase ResolveActionByApiId(string apiId, string operationName);
    }

    public class ActionExecutorFactory : IActionExecutorFactory
    {
        private readonly IServiceProvider _sp;

        public ActionExecutorFactory(IServiceProvider serviceProvider)
        {
            _sp = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ActionExecutorBase ResolveActionByKey(string key)
        {
            var axRegistrationList = _sp.GetRequiredService
                <IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList
                .LastOrDefault(x => x.ActionName == key);
            return axRegistration == null
                ? null
                : _sp.GetRequiredService(axRegistration.Type)
                    as ActionExecutorBase;
        }

        public ActionExecutorBase ResolveActionByType(string actionType)
        {
            var axRegistrationList = _sp.GetRequiredService<IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList.LastOrDefault(x => x.ActionType == actionType);
            return axRegistration == null
                ? null
                : _sp.GetRequiredService(axRegistration.Type) as ActionExecutorBase;
        }

        public ActionExecutorBase ResolveActionByApiId(string apiId, string operationName)
        {
            var axRegistrationList = _sp.GetRequiredService<IEnumerable<ActionExecutorRegistration>>();
            var axRegistration = axRegistrationList.LastOrDefault(
                x => x.ActionApiId == apiId && x.SupportedOperationNames.Contains(operationName));
            return axRegistration == null
                ? null
                : _sp.GetRequiredService(axRegistration.Type) as ActionExecutorBase;
        }
    }
}