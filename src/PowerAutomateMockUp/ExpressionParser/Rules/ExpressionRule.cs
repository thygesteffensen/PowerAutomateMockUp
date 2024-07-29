using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Rules
{
    public class ExpressionRule : IRule
    {
        private readonly IFunction _function;
        private readonly IEnumerable<IRule> _args;
        public string FunctionName { get; }

        
        public ExpressionRule(IEnumerable<IFunction> functions, string functionName, IEnumerable<IRule> args)
        {
            _function = functions.ToList().Find(x => x.FunctionName == functionName);
            if (_function == null)
            {
                throw new FunctionNotKnown(
                    $"Function with name: {functionName}, does not exist.\nAdd the function to the expression engine to test this expression.");
            }

            _args = args;
            FunctionName = functionName;
        }

        public ValueContainer Evaluate()
        {
            var strings = _args?.Select(rule => rule.Evaluate()).ToArray();

            return _function.ExecuteFunction(strings);
        }

        public string PrettyPrint()
        {
            var seed = _args?.First().PrettyPrint();
            var argumentList = _args?.Skip(1).Aggregate(seed, (s, rule) => s + ", " + rule.PrettyPrint());
            return $"{FunctionName}({argumentList})";
        }
    }
}