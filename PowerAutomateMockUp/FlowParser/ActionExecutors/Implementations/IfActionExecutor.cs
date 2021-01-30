using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class IfActionExecutor : DefaultBaseActionExecutor
    {
        private const string Or = "or";
        private const string And = "and";
        private readonly IExpressionEngine _expressionEngine;
        private readonly ILogger<IfActionExecutor> _logger;

        public IfActionExecutor(IExpressionEngine expressionEngine, ILogger<IfActionExecutor> logger)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // https://en.wikipedia.org/wiki/Short-circuit_evaluation
        // https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html

        public override Task<ActionResult> Execute()
        {
            var expression = (JObject) Json.SelectToken("$.expression");

            var type = expression.Properties().ToList()[0].Name;
            var result = ParseGroup(expression, type);

            _logger.LogInformation($"Condition action '{ActionName}' evaluated {result}.");

            if (result)
            {
                var actions = Json.SelectToken("$.actions");
                if (actions?.HasValues ?? false)
                {
                    return Task.FromResult(
                        new ActionResult
                        {
                            NextAction = ((JProperty) actions.First)?.Name
                        });
                }
            }
            else
            {
                var elseActions = Json.SelectToken("$.else.actions");
                if (elseActions?.HasValues ?? false)
                {
                    return Task.FromResult(
                        new ActionResult
                        {
                            NextAction = ((JProperty) elseActions.First)?.Name
                        });
                }
            }

            return Task.FromResult(new ActionResult());
        }

        private bool ParseGroup(JObject group, string type)
        {
            var theType = group.Properties().First().Name;

            var condition = false;

            if (theType == Or || theType == And)
            {
                var expressions = group.SelectToken($"$.{theType}");
                var statements = expressions.Children();

                foreach (var statement in statements)
                {
                    condition = EvaluateStatement((JProperty) statement.First);

                    switch (type)
                    {
                        case Or when condition:
                            return true;
                        case And when !condition:
                            return false;
                    }
                }
            }
            else
            {
                var t = group.OfType<JProperty>();
                return EvaluateStatement(t.First());
            }

            return condition;
        }

        private bool EvaluateStatement(JProperty statement)
        {
            if (statement.Name == Or || statement.Name == And)
            {
                return ParseGroup(statement.First as JObject, statement.Name);
            }

            Enum.TryParse<ConditionsTypes>(statement.Name, true, out var conditionType);

            var isNot = false;
            if (conditionType == ConditionsTypes.Not)
            {
                statement = (JProperty) statement.First.First;
                isNot = true;
                Enum.TryParse(statement.Name, true, out conditionType);
            }

            var conditionValues = (JArray) statement.First;

            var firstConditionValue = _expressionEngine.ParseToValueContainer(conditionValues[0].Value<string>());
            var secondConditionValue = _expressionEngine.ParseToValueContainer(conditionValues[1].Value<string>());

            var conditionResult = EvaluateCondition(conditionType, firstConditionValue, secondConditionValue);

            return isNot ? !conditionResult : conditionResult;
        }

        private bool EvaluateCondition(ConditionsTypes conditionsType, ValueContainer value1, ValueContainer value2)
        {
            if (value1.Type() != value2.Type()) return false;

            switch (conditionsType)
            {
                case ConditionsTypes.Contains:
                    if (value2.Type() != ValueContainer.ValueType.String ||
                        value1.Type() != ValueContainer.ValueType.Array ||
                        value1.Type() != ValueContainer.ValueType.String)
                    {
                        if (value1.Type() != ValueContainer.ValueType.String ||
                            value2.Type() != ValueContainer.ValueType.String)
                        {
                            throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(ActionName, "endsWidth", "");
                        }
                    }

                    var value = value2.GetValue<string>();

                    if (value1.Type() == ValueContainer.ValueType.Array)
                    {
                        var collection = value1.GetValue<IEnumerable<ValueContainer>>();
                        // if (!collection.Any())
                        // {
                            // return false;
                        // }

                        return (from valueContainer in collection
                            where valueContainer.Type() == ValueContainer.ValueType.String
                            select valueContainer.GetValue<string>()).Any(str => str.Equals(value));
                    }
                    else
                    {
                        var str = value1.GetValue<string>();
                        return str.Contains(value);
                    }
                case ConditionsTypes.Equals:
                    return value1.Equals(value2);
                case ConditionsTypes.StartsWith:
                    if (value1.Type() != ValueContainer.ValueType.String ||
                        value2.Type() != ValueContainer.ValueType.String)
                    {
                        throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(ActionName, "startsWidth", "");
                    }

                    return value1.GetValue<string>().StartsWith(value2.GetValue<string>());
                case ConditionsTypes.EndsWith:
                    if (value1.Type() != ValueContainer.ValueType.String ||
                        value2.Type() != ValueContainer.ValueType.String)
                    {
                        throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(ActionName, "endsWidth", "");
                    }

                    return value1.GetValue<string>().EndsWith(value2.GetValue<string>());
                case ConditionsTypes.Not:
                    break;
                case ConditionsTypes.Greater:
                    return value1.CompareTo(value2) > 0;
                case ConditionsTypes.GreaterOrEquals:
                    return value1.CompareTo(value2) >= 0;
                case ConditionsTypes.Less:
                    return value1.CompareTo(value2) < 0;
                case ConditionsTypes.LessOrEquals:
                    return value1.CompareTo(value2) <= 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionsType), conditionsType, null);
            }

            return false;
        }

        private enum ConditionsTypes
        {
            Contains,
            Equals,
            Greater,
            GreaterOrEquals,
            Less,
            LessOrEquals,
            StartsWith,
            EndsWith,
            Not
        }
    }
}