using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Math;

namespace Test.Expression
{
    public class MathFunctionTests
    {
        internal static object[] MathFunctionTestInput =
        {
            new object[]
            {
                new AddFunction(),
                "add",
                new[] {new ValueContainer(1), new ValueContainer(2)},
                new ValueContainer(3)
            },
        };
    }
}