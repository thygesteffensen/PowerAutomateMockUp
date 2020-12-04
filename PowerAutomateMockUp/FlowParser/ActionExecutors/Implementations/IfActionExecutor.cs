using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Parser.ExpressionParser;

namespace Parser.FlowParser.ActionExecutors.Implementations
{
    public class IfActionExecutor : DefaultBaseActionExecutor
    {
        private const string Or = "or";
        private const string And = "and";
        private readonly IExpressionEngine _expressionEngine;

        public IfActionExecutor(IExpressionEngine expressionEngine)
        {
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
        }

        // https://en.wikipedia.org/wiki/Short-circuit_evaluation
        // https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html

        public override Task<ActionResult> Execute()
        {
            var expression = (JObject) Json.SelectToken("$.expression");

            var type = expression.Properties().ToList()[0].Name;
            var result = ParseGroup(expression, type);

            if (result)
            {
                var actions = Json.SelectToken("$.actions");
                if (actions.HasValues)
                {
                    return Task.FromResult(
                        new ActionResult
                        {
                            NextAction = ((JProperty) actions.First).Name
                        });
                }
            }
            else
            {
                var elseActions = Json.SelectToken("$.else.actions");
                if (elseActions.HasValues)
                {
                    return Task.FromResult(
                        new ActionResult
                        {
                            NextAction = ((JProperty) elseActions.First).Name
                        });
                }
            }

            // return new Task<ActionResult>(() => new ActionResult());
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

            var firstConditionValue = _expressionEngine.Parse(conditionValues[0].Value<string>());
            var secondConditionValue = _expressionEngine.Parse(conditionValues[1].Value<string>());

            var conditionResult = EvaluateCondition(conditionType, firstConditionValue, secondConditionValue);

            return isNot ? !conditionResult : conditionResult;
        }

        private bool EvaluateCondition(ConditionsTypes conditionsType, string value1, string value2)
        {
            switch (conditionsType)
            {
                case ConditionsTypes.Contains:
                    return value1.Contains(value2);
                case ConditionsTypes.Equals:
                    return value1 == value2;
                case ConditionsTypes.StartsWith:
                    return value1.StartsWith(value2);
                case ConditionsTypes.EndsWith:
                    return value1.EndsWith(value2);
                case ConditionsTypes.Not:
                    break;
            }

            var decimalValue1 = decimal.Parse(value1);
            var decimalValue2 = decimal.Parse(value2);
            switch (conditionsType)
            {
                case ConditionsTypes.Greater:
                    return decimalValue1 > decimalValue2;
                case ConditionsTypes.GreaterOrEquals:
                    return decimalValue1 >= decimalValue2;
                case ConditionsTypes.Less:
                    return decimalValue1 < decimalValue2;
                case ConditionsTypes.LessOrEquals:
                    return decimalValue1 <= decimalValue2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionsType), conditionsType, null);
            }
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