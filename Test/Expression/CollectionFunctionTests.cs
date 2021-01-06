using System.Collections.Generic;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Implementations.CollectionFunctions;

namespace Test.Expression
{
    public class CollectionFunctionTests
    {
        internal static object[] CollectionFunctionTestInput =
        {
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[] {new ValueContainer("Hello world!"), new ValueContainer("world")},
                new ValueContainer(true)
            },
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[] {new ValueContainer("Hello world!"), new ValueContainer("univers")},
                new ValueContainer(false)
            },
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[]
                {
                    new ValueContainer(new[] {new ValueContainer(1), new ValueContainer(2), new ValueContainer(3)}),
                    new ValueContainer(1)
                },
                new ValueContainer(true)
            },
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[]
                {
                    new ValueContainer(new[] {new ValueContainer(1), new ValueContainer(2), new ValueContainer(3)}),
                    new ValueContainer(4)
                },
                new ValueContainer(false)
            },
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                        {{"key1", new ValueContainer()}, {"key2", new ValueContainer()}}),
                    new ValueContainer("key2")
                },
                new ValueContainer(true)
            },
            new object[]
            {
                new ContainsFunction(),
                "contains",
                new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                        {{"key1", new ValueContainer()}, {"key2", new ValueContainer()}}),
                    new ValueContainer("key3")
                },
                new ValueContainer(false)
            },
        };
    }
}