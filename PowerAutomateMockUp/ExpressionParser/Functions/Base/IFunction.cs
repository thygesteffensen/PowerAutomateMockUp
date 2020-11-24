using System;

namespace Parser.ExpressionParser.Functions.Base
{
    public interface IFunction
    {
        string FunctionName { get; }
        ValueContainer ExecuteFunction(ValueContainer[] strings);
    }

    public abstract class Function : IFunction
    {
        public string FunctionName { get; }

        protected Function(string functionName)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        }

        public abstract ValueContainer ExecuteFunction(params ValueContainer[] parameters);
    }
}