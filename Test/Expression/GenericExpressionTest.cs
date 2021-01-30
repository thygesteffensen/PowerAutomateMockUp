using NUnit.Framework;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;

namespace Test.Expression
{
    [TestFixture]
    public class GenericExpressionTest
    {
        [Test, TestCaseSource(typeof(StringFunctionTests), nameof(StringFunctionTests.StringFunctionTestInput))]
        [TestCaseSource(typeof(CollectionFunctionTests), nameof(CollectionFunctionTests.CollectionFunctionTestInput))]
        [TestCaseSource(typeof(LogicalFunctionTest), nameof(LogicalFunctionTest.LogicalFunctionTestInput))]
        public void TestFunction(Function func, string name,
            ValueContainer[] parameters, ValueContainer expected)
        {
            var result = func.ExecuteFunction(parameters);

            Assert.AreEqual(name, func.FunctionName);
            Assert.AreEqual(expected, result);
        }
    }
}