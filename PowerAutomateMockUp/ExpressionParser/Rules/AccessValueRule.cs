using System;
using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Rules
{
    public class AccessValueRule : IRule
    {
        private readonly IRule _func;
        private readonly IEnumerable<IRule> _indexRules;

        public AccessValueRule(IRule func, IEnumerable<IRule> indexRules)
        {
            _func = func;
            var tempIndexRules = new List<IRule>();

            foreach (var indexRule in indexRules)
            {
                var indexObject = indexRule.Evaluate().GetValue<Dictionary<string, ValueContainer>>();
                var index = indexObject["index"];
                var nullConditional = indexObject["nullConditional"].GetValue<bool>();

                if (index.Type() == ValueContainer.ValueType.String)
                {
                    var indexes = index.GetValue<string>().Split('/');

                    tempIndexRules.Add(new IndexRule(new ConstantRule(new ValueContainer(indexes.First())),
                        nullConditional));

                    foreach (var accessor in indexes.Skip(1))
                    {
                        tempIndexRules.Add(new IndexRule(new ConstantRule(new ValueContainer(accessor)), false));
                    }
                }
                else
                {
                    tempIndexRules.Add(indexRule);
                }
            }

            _indexRules = tempIndexRules;
        }

        public ValueContainer Evaluate()
        {
            var currentValue = _func.Evaluate();

            foreach (var indexRule in _indexRules)
            {
                var indexObject = indexRule.Evaluate().GetValue<Dictionary<string, ValueContainer>>();
                var index = indexObject["index"];
                var nullConditional = indexObject["nullConditional"].GetValue<bool>();

                if (currentValue.Type() == ValueContainer.ValueType.Array)
                {
                    var asArray = currentValue.GetValue<ValueContainer[]>();
                    if (index.Type() != ValueContainer.ValueType.Integer)
                    {
                        throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(
                            (_func as ExpressionRule)?.FunctionName,
                            _func.PrettyPrint() + string.Join(", ", _indexRules.Select(x => x.PrettyPrint())),
                            index.GetValue<string>());
                    }

                    var i = index.GetValue<int>();
                    if (i > asArray.Length)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    currentValue = asArray[i];
                }

                else if (currentValue.Type() == ValueContainer.ValueType.Object)
                {
                    var asObject = currentValue.GetValue<Dictionary<string, ValueContainer>>();
                    var key = index.GetValue<string>();

                    if (asObject.ContainsKey(key))
                    {
                        currentValue = asObject[key];
                        continue;
                    }

                    if (nullConditional)
                    {
                        return new ValueContainer();
                    }

                    throw InvalidTemplateException.BuildInvalidTemplateExceptionObject(
                        (_func as ExpressionRule)?.FunctionName,
                        _func.PrettyPrint() + string.Join(", ", _indexRules.Select(x => x.PrettyPrint())),
                        index.GetValue<string>(),
                        asObject.Keys.ToArray());
                }
            }

            return currentValue;
        }

        public string PrettyPrint()
        {
            throw new NotImplementedException();
        }
    }
}