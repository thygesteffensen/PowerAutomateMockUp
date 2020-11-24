using System.Collections.Generic;
using Parser.ExpressionParser;

namespace Parser
{
    public interface IState : IVariableRetriever, ITriggerOutputsRetriever, IOutputsRetriever
    {
        void AddTriggerOutputs(ValueContainer triggerOutputs);
        void AddVariable(string variableName, IEnumerable<ValueContainer> values);
        void AddVariable(string variableName, ValueContainer valueContainer);
        void AddOutputs(string actionName, ValueContainer valueContainer);
        void AddOutputs(string actionName, IEnumerable<ValueContainer> values);
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
}