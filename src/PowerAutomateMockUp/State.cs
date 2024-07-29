using System;
using System.Collections.Generic;
using Parser.ExpressionParser;

namespace Parser
{
    public class State : IState
    {
        private readonly Dictionary<string, ValueContainer> _variables = new Dictionary<string, ValueContainer>();
        private readonly Dictionary<string, ValueContainer> _outputs = new Dictionary<string, ValueContainer>();

        private readonly Dictionary<string, IItemHandler> _itemHandlers = new Dictionary<string, IItemHandler>();

        private ValueContainer _triggerOutputs;

        public void AddVariable(string variableName, IEnumerable<ValueContainer> values)
        {
            _variables[variableName] = new ValueContainer(values);
        }

        public void AddVariable(string variableName, ValueContainer valueContainer)
        {
            _variables[variableName] = new ValueContainer(valueContainer);
        }

        public void AddOutputs(string actionName, ValueContainer valueContainer)
        {
            _outputs[actionName] = new ValueContainer(valueContainer);
        }

        public void AddOutputs(string actionName, IEnumerable<ValueContainer> values)
        {
            _outputs[actionName] = new ValueContainer(values);
        }

        public void AddItemHandler(string actionName, IItemHandler itemHandler)
        {
            _itemHandlers[actionName] = itemHandler;
        }

        public void AddTriggerOutputs(ValueContainer triggerOutputs)
        {
            _triggerOutputs = triggerOutputs ?? throw new ArgumentNullException(nameof(triggerOutputs));
        }

        public ValueContainer GetVariable(string variableName)
        {
            var successfulRetrieve = _variables.TryGetValue(variableName, out var value);
            if (!successfulRetrieve)
                throw new VariableDoesNotExists(
                    $"Variable with variableName: {variableName} was not found. If you're not running a full flow, remember to add variables to the expression engine.");
            return value;
        }

        public ValueContainer GetTriggerOutputs()
        {
            return _triggerOutputs;
        }

        public ValueContainer GetOutputs(string actionName)
        {
            var successfulRetrieve = _outputs.TryGetValue(actionName, out var value);
            return successfulRetrieve ? value : new ValueContainer();
        }

        public ValueContainer GetCurrentItem(string actionName)
        {
            var successfulHandlerRetrieve = _itemHandlers.TryGetValue(actionName, out var handler);
            if (!successfulHandlerRetrieve)
            {
                throw new PowerAutomateMockUpException("Cannot resolve handler for item");
            }

            return handler.GetCurrentItem();
        }
    }
}