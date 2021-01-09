using NUnit.Framework;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;

namespace Test.Expression
{
    [TestFixture]
    public class GenericExceptionTest
    {
        [Test,TestCaseSource(typeof(StringFunctionTests), nameof(StringFunctionTests.StringFunctionTestInput))]
        [TestCaseSource(typeof(CollectionFunctionTests), nameof(CollectionFunctionTests.CollectionFunctionTestInput))]
        [TestCaseSource(typeof(LogicalFunctionTest), nameof(LogicalFunctionTest.LogicalFunctionTestInput))]
        public void TestFunctions(Function func, string name, ValueContainer[] parameters,
            ValueContainer expected)
        {
            Assert.AreEqual(name, func.FunctionName);

            var result = func.ExecuteFunction(parameters);

            Assert.AreEqual(expected, result);
        }
    }
}