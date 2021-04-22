using NUnit.Framework;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Implementations.StringFunctions;

namespace Test.Expression
{
    [TestFixture]
    public class StringFunctionTests
    {
        internal static object[] StringFunctionTestInput =
        {
            new object[]
            {
                new ConcatFunction(),
                "concat",
                new[] {new ValueContainer("Hey "), new ValueContainer("you"), new ValueContainer("!")},
                new ValueContainer("Hey you!")
            },
            new object[]
            {
                new EndsWithFunction(),
                "endsWith",
                new[] {new ValueContainer("0123456789"), new ValueContainer("789")},
                new ValueContainer(true)
            },
            new object[]
            {
                new FormatNumberFunction(),
                "formatNumber",
                new[] {new ValueContainer(1234567890), new ValueContainer("0,0.00"), new ValueContainer("en-us")},
                new ValueContainer("1,234,567,890.00")
            },
            new object[]
            {
                new FormatNumberFunction(),
                "formatNumber",
                new[] {new ValueContainer(1234567890), new ValueContainer("0,0.00"), new ValueContainer("is-is")},
                new ValueContainer("1.234.567.890,00")
            },
            new object[]
            {
                new FormatNumberFunction(),
                "formatNumber",
                new[] {new ValueContainer(17.35), new ValueContainer("C2")},
                new ValueContainer("$17.35")
            },
            new object[]
            {
                new FormatNumberFunction(),
                "formatNumber",
                new[] {new ValueContainer(17.35), new ValueContainer("C2"), new ValueContainer("da-dk")},
                new ValueContainer("17,35 kr.")
            },
            new object[]
            {
                new IndexOfFunction(),
                "indexOf",
                new[] {new ValueContainer("0123456789"), new ValueContainer("789")},
                new ValueContainer(7)
            },
            new object[]
            {
                new IndexOfFunction(),
                "indexOf",
                new[] {new ValueContainer("0123456789"), new ValueContainer("abc")},
                new ValueContainer(-1)
            },
            new object[]
            {
                new IndexOfFunction(),
                "indexOf",
                new[] {new ValueContainer("0123456789"), new ValueContainer("")},
                new ValueContainer(0)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer("Hey there"), new ValueContainer("e")},
                new ValueContainer(8)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer(""), new ValueContainer("not empty")},
                new ValueContainer(-1)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer(""), new ValueContainer("")},
                new ValueContainer(0)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer("abc"), new ValueContainer("")},
                new ValueContainer(2)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer("Hey there"), new ValueContainer("e")},
                new ValueContainer(8)
            },
            new object[]
            {
                new LastIndexOfFunction(),
                "lastIndexOf",
                new[] {new ValueContainer("Hey there"), new ValueContainer("")},
                new ValueContainer(8)
            },
            new object[]
            {
                new LengthFunction(),
                "length",
                new[] {new ValueContainer("0123456789")},
                new ValueContainer(10)
            },
            new object[]
            {
                new ReplaceFunction(),
                "replace",
                new[] {new ValueContainer("The old string"), new ValueContainer("old"), new ValueContainer("new")},
                new ValueContainer("The new string")
            },
            new object[]
            {
                new StartsWithFunction(),
                "startsWith",
                new[] {new ValueContainer("The old string"), new ValueContainer("The")},
                new ValueContainer(true)
            },
            new object[]
            {
                new StartsWithFunction(),
                "startsWith",
                new[] {new ValueContainer("The old string"), new ValueContainer("String")},
                new ValueContainer(false)
            },
            new object[]
            {
                new SubstringFunction(),
                "substring",
                new[] {new ValueContainer("The old string"), new ValueContainer(4)},
                new ValueContainer("old string")
            },
            new object[]
            {
                new SubstringFunction(),
                "substring",
                new[] {new ValueContainer("The old string"), new ValueContainer(4), new ValueContainer(3)},
                new ValueContainer("old")
            },
            new object[]
            {
                new ToLowerFunction(),
                "toLower",
                new[] {new ValueContainer("Hey")},
                new ValueContainer("hey")
            },
            new object[]
            {
                new ToUpperFunction(),
                "toUpper",
                new[] {new ValueContainer("Hey")},
                new ValueContainer("HEY")
            },
            new object[]
            {
                new TrimFunction(),
                "trim",
                new[] {new ValueContainer(" Hey  ")},
                new ValueContainer("Hey")
            },
        };

        [Test]
        public void GuidFunctionTest()
        {
            var func = new GuidFunction();

            Assert.AreEqual("guid", func.FunctionName);

            var result1 = func.ExecuteFunction();

            Assert.AreEqual(36, result1.GetValue<string>().Length);

            var result2 = func.ExecuteFunction(new ValueContainer("N"));

            Assert.AreEqual(32, result2.GetValue<string>().Length);

            var result3 = func.ExecuteFunction(new ValueContainer("B"));

            Assert.AreEqual(38, result3.GetValue<string>().Length);
        }
        
        [Test]
        public void SplitFunctionTest()
        {
            var func = new SplitFunction();

            Assert.AreEqual("split", func.FunctionName);

            var result1 = func.ExecuteFunction(new ValueContainer("This is sp a sp splitted sp string"), new ValueContainer("sp"));

            var array = result1.GetValue<ValueContainer[]>();
            
            Assert.AreEqual(5, array.Length);

            Assert.AreEqual("This is ", array[0].GetValue<string>());
            Assert.AreEqual(" a ", array[1].GetValue<string>());
            Assert.AreEqual(" ", array[2].GetValue<string>());
            Assert.AreEqual("litted ", array[3].GetValue<string>());
            Assert.AreEqual(" string", array[4].GetValue<string>());
        }
    }
}