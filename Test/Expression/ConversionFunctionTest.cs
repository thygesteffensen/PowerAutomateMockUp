using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Implementations.ConversionFunctions;

namespace Test.Expression
{
    public class ConversionFunctionTest
    {
        internal static object[] ConversionFunctionTestInput =
        {
            new object[]
            {
                new ArrayFunction(),
                "array",
                new []{new ValueContainer("string")},
                new ValueContainer(new []{new ValueContainer("string")})
            },
            new object[]
            {
                new Base64Function(),
                "base64",
                new []{new ValueContainer("hello")},
                new ValueContainer(new ValueContainer("aGVsbG8="))
            },
            new object[]
            {
                new Base64ToBinaryFunction(),
                "base64ToBinary",
                new []{new ValueContainer("aGVsbG8=")},
                new ValueContainer(new ValueContainer("0110100001100101011011000110110001101111"))
            },
            new object[]
            {
                new Base64ToStringFunction(),
                "base64ToString",
                new []{new ValueContainer("aGVsbG8=")},
                new ValueContainer(new ValueContainer("hello"))
            },
            new object[]
            {
                new BinaryFunction(),
                "binary",
                new []{new ValueContainer("hello")},
                new ValueContainer(new ValueContainer("0110100001100101011011000110110001101111"))
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer("true")},
                new ValueContainer(true)
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer("false")},
                new ValueContainer(false)
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer(0)},
                new ValueContainer(false)
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer(1)},
                new ValueContainer(true)
            },
            
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer(-1)},
                new ValueContainer(true)
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer(true)},
                new ValueContainer(true)
            },
            new object[]
            {
                new BoolFunction(),
                "bool",
                new []{new ValueContainer(false)},
                new ValueContainer(false)
            },
            new object[]
            {
                new DataUriFunction(),
                "dataUri",
                new []{new ValueContainer("hello")},
                new ValueContainer("data:text/plain;charset=utf-8;base64,aGVsbG8=")
            },
        };
    }
}