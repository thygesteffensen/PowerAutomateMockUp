using System.Collections.Generic;
using Parser.ExpressionParser;

namespace Parser
{
    public interface IState : IVariableRetriever, ITriggerOutputsRetriever, IOutputsRetriever, IItemsRetriever
    {
        void AddTriggerOutputs(ValueContainer triggerOutputs);
        void AddVariable(string variableName, IEnumerable<ValueContainer> values);
        void AddVariable(string variableName, ValueContainer valueContainer);
        void AddOutputs(string actionName, ValueContainer valueContainer);
        void AddOutputs(string actionName, IEnumerable<ValueContainer> values);
        void AddItemHandler(string actionName, IItemHandler itemHandler);
    }
    
    public interface IItemHandler
    {
        ValueContainer GetCurrentItem();
    }

    public interface IVariableRetriever
    {
        ValueContainer GetVariable(string variableName);
    }

    public interface ITriggerOutputsRetriever
    {
        ValueContainer GetTriggerOutputs();
    }

    public interface IOutputsRetriever
    {
        ValueContainer GetOutputs(string actionName);
    }

    public interface IItemsRetriever
    {
        ValueContainer GetCurrentItem(string actionName);
    }
}