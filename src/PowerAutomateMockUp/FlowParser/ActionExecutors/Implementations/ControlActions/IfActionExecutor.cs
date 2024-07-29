using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.FlowParser.ActionExecutors.Implementations.ControlActions
{
    public class IfActionExecutor : DefaultBaseActionExecutor
    {
        private const string Or = "or";
        private const string And = "and";
        private readonly IExpressionEngine _expressionEngine;
        private readonly ILogger<IfActionExecutor> _logger;
        private readonly IScopeDepthManager _scopeDepthManager;

        public IfActionExecutor(
            IExpressionEngine expressionEngine,
            ILogger<IfActionExecutor> logger,
            IScopeDepthManager scopeDepthManager)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeDepthManager = scopeDepthManager ?? throw new ArgumentNullException(nameof(scopeDepthManager));
        }

        // https://en.wikipedia.org/wiki/Short-circuit_evaluation
        // https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html

        public override Task<ActionResult> Execute()
        {
            var expression = (JObject) Json.SelectToken("$.expression");

            if (expression == null)
            {
                throw new PowerAutomateMockUpException(
                    $"JSON expression should not be null in if condition. Condition action name : {ActionName}.");
            }

            var type = expression.Properties().ToList()[0].Name;
            var result = ParseGroup(expression, type);

            _logger.LogInformation("Condition action '{ActionName}' evaluated {Result}",
                ActionName, result);

            if (result)
            {
                var actions = Json.SelectToken("$.actions");

                if (!(actions?.HasValues ?? false)) return Task.FromResult(new ActionResult());

                _scopeDepthManager.Push(ActionName, actions.OfType<JProperty>(), null);
                return Task.FromResult(
                    new ActionResult
                    {
                        NextAction = ((JProperty) actions.First)?.Name
                    });
            }

            var elseActions = Json.SelectToken("$.else.actions");

            if (!(elseActions?.HasValues ?? false)) return Task.FromResult(new ActionResult());

            _scopeDepthManager.Push(ActionName, elseActions.OfType<JProperty>(), null);
            return Task.FromResult(
                new ActionResult
                {
                    NextAction = ((JProperty) elseActions.First)?.Name
                });
        }

        private bool ParseGroup(JObject group, string type)
        {
            var groupType = group.Properties().First().Name;

            var condition = false;

            if (groupType == Or || groupType == And)
            {
                var expressions = group.SelectToken($"$.{groupType}");
                if (expressions == null)
                {
                    throw new PowerAutomateMockUpException("JSON expression group should exist.");
                }

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
            if (statement?.First == null)
            {
                throw new PowerAutomateMockUpException("Cannot evaluate null statement.");
            }
            
            if (statement.Name == Or || statement.Name == And)
            {
                return ParseGroup(statement.First as JObject, statement.Name);
            }

            Enum.TryParse<ConditionsTypes>(statement.Name, true, out var conditionType);

            var isNot = false;
            if (conditionType == ConditionsTypes.Not)
            {
                statement = (JProperty) statement.First.First;
                if (statement == null)
                {
                    throw new PowerAutomateMockUpException("Not statement must contain new group.");
                }
                isNot = true;
                Enum.TryParse(statement.Name, true, out conditionType);
            }

            var conditionValues = (JArray) statement.First;
            if (conditionValues == null || conditionValues.Count < 2)
            {
                throw new PowerAutomateMockUpException("Condition values must cannot be null and two is expected.");
            }

            var firstConditionValue = new ValueContainer(conditionValues[0]);
            if (firstConditionValue.Type() == ValueContainer.ValueType.String)
            {
                firstConditionValue = _expressionEngine.ParseToValueContainer(firstConditionValue.GetValue<string>());
            }
            
            var secondConditionValue = new ValueContainer(conditionValues[1]);
            if (secondConditionValue.Type() == ValueContainer.ValueType.String)
            {
                secondConditionValue = _expressionEngine.ParseToValueContainer(secondConditionValue.GetValue<string>());
            }

            var conditionResult = EvaluateCondition(conditionType, firstConditionValue, secondConditionValue);

            return isNot ? !conditionResult : conditionResult;
        }


        private bool EvaluateCondition(ConditionsTypes conditionsType, ValueContainer value1, ValueContainer value2)
        {
            if (IsBooleanSpecialCase(value1, value2, conditionsType, out var result)) return result;
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
                            throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(ActionName, "endsWidth",
                                "");
                        }
                    }

                    var value = value2.GetValue<string>();

                    if (value1.Type() == ValueContainer.ValueType.Array)
                    {
                        var collection = value1.GetValue<IEnumerable<ValueContainer>>();

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
                        throw InvalidTemplateException.BuildInvalidTemplateExceptionArray(ActionName, "startsWidth",
                            "");
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

        /**
         * The expression @true or '@{true}' does not evaluate to a TRUE boolean constant, but the string 'true'.
         * equals('true', equals(1,1)) evaluates to false...
         *
         * However, when equals(1,1) is used in an condition and the condition type is equals, then equals(1,1) equals
         * 'true'.
         * This is the only place where this behaviour have been observed. 
         */
        private bool IsBooleanSpecialCase(ValueContainer value1, ValueContainer value2, ConditionsTypes conditionsType,
            out bool result)
        {
            result = false;
            if (conditionsType != ConditionsTypes.Equals) return false;
            if (value1.Type() == value2.Type()) return false;

            if (!(value1.Type() == ValueContainer.ValueType.String || value1.Type() == ValueContainer.ValueType.Boolean)
                &&
                !(value2.Type() == ValueContainer.ValueType.String || value2.Type() == ValueContainer.ValueType.Boolean)
            ) return false;

            var b1 = value1.Type() == ValueContainer.ValueType.String
                ? value1.GetValue<string>().Equals("true")
                : value1.GetValue<bool>();

            var b2 = value2.Type() == ValueContainer.ValueType.String
                ? value2.GetValue<string>().Equals("true")
                : value2.GetValue<bool>();

            result = b1 == b2;
            return true;
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