using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions;

namespace Test.Expression
{
    public class LogicalFunctionTest
    {
        internal static object[] LogicalFunctionTestInput =
        {
            new object[]
            {
                new AndFunction(),
                "and",
                new[] {new ValueContainer(true), new ValueContainer(true)},
                new ValueContainer(true)
            },
            new object[]
            {
                new AndFunction(),
                "and",
                new[] {new ValueContainer(true), new ValueContainer(true), new ValueContainer(true)},
                new ValueContainer(true)
            },
            new object[]
            {
                new AndFunction(),
                "and",
                new[] {new ValueContainer(true), new ValueContainer(false)},
                new ValueContainer(false)
            },
            new object[]
            {
                new EqualFunction(),
                "equal",
                new[] {new ValueContainer(1), new ValueContainer(1)},
                new ValueContainer(true)
            },
            new object[]
            {
                new EqualFunction(),
                "equal",
                new[] {new ValueContainer(1), new ValueContainer("1")},
                new ValueContainer(false)
            },
            new object[]
            {
                new EqualFunction(),
                "equal",
                new[] {new ValueContainer("string"), new ValueContainer("string")},
                new ValueContainer(true)
            },
            new object[]
            {
                new GreaterFunction(),
                "greater",
                new[] {new ValueContainer(1), new ValueContainer(1)},
                new ValueContainer(false)
            },
            new object[]
            {
                new GreaterFunction(),
                "greater",
                new[] {new ValueContainer("apple"), new ValueContainer("banana")},
                new ValueContainer(false)
            },
            new object[]
            {
                new GreaterFunction(),
                "greater",
                new[] {new ValueContainer(1.2), new ValueContainer(0.2)},
                new ValueContainer(true)
            },
            new object[]
            {
                new GreaterOrEqualsFunction(),
                "greaterOrEquals",
                new[] {new ValueContainer(1), new ValueContainer(1)},
                new ValueContainer(true)
            },
            new object[]
            {
                new GreaterOrEqualsFunction(),
                "greaterOrEquals",
                new[] {new ValueContainer("apple"), new ValueContainer("banana")},
                new ValueContainer(false)
            },
            new object[]
            {
                new GreaterOrEqualsFunction(),
                "greaterOrEquals",
                new[] {new ValueContainer(1.2), new ValueContainer(0.2)},
                new ValueContainer(true)
            },
            new object[]
            {
                new IfFunction(),
                "if",
                new[] {new ValueContainer(true), new ValueContainer("then"), new ValueContainer("else")},
                new ValueContainer("then")
            },
            new object[]
            {
                new IfFunction(),
                "if",
                new[] {new ValueContainer(false), new ValueContainer("then"), new ValueContainer("else")},
                new ValueContainer("else")
            },
            new object[]
            {
                new LessFunction(),
                "less",
                new[] {new ValueContainer(1), new ValueContainer(1)},
                new ValueContainer(false)
            },
            new object[]
            {
                new LessFunction(),
                "less",
                new[] {new ValueContainer("apple"), new ValueContainer("banana")},
                new ValueContainer(true)
            },
            new object[]
            {
                new LessFunction(),
                "less",
                new[] {new ValueContainer(1.2), new ValueContainer(0.2)},
                new ValueContainer(false)
            },
            new object[]
            {
                new LessOrEqualsFunction(),
                "lessOrEquals",
                new[] {new ValueContainer(1), new ValueContainer(1)},
                new ValueContainer(true)
            },
            new object[]
            {
                new LessOrEqualsFunction(),
                "lessOrEquals",
                new[] {new ValueContainer("apple"), new ValueContainer("banana")},
                new ValueContainer(true)
            },
            new object[]
            {
                new LessOrEqualsFunction(),
                "lessOrEquals",
                new[] {new ValueContainer(1.2), new ValueContainer(0.2)},
                new ValueContainer(false)
            },
            new object[]
            {
                new NotFunction(),
                "not",
                new[] {new ValueContainer(true)},
                new ValueContainer(false)
            },
            new object[]
            {
                new NotFunction(),
                "not",
                new[] {new ValueContainer(false)},
                new ValueContainer(true)
            },
            new object[]
            {
                new OrFunction(),
                "or",
                new[] {new ValueContainer(true), new ValueContainer(true)},
                new ValueContainer(true)
            },
            new object[]
            {
                new OrFunction(),
                "or",
                new[] {new ValueContainer(true), new ValueContainer(true), new ValueContainer(true)},
                new ValueContainer(true)
            },
            new object[]
            {
                new OrFunction(),
                "or",
                new[] {new ValueContainer(true), new ValueContainer(false)},
                new ValueContainer(true)
            }
        };
    }
}