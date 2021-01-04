using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace Test.ActionTests
{
    [TestFixture]
    public class DoUntilActionExecutorTest
    {
        string DoUntilAction =
            "{\"Do_until\": {" +
            "\"actions\": {" +
            "\"Increment_variable\": {" +
            "\"runAfter\": {}," +
            "\"type\": \"IncrementVariable\"," +
            "\"inputs\": {" +
            "\"name\": \"Variable\"," +
            "\"value\": 1" +
            "}}}," +
            "\"runAfter\": { \"Initialize_variable\": [ \"Succeeded\" ] }," +
            "\"expression\": \"@greater(variables('Variable'), 10)\"," +
            "\"limit\": {" +
            "\"count\": 3," +
            "\"timeout\": \"PT1H\"" +
            "}," +
            "\"type\": \"Until\"" +
            "}," +
            "\"Initialize_variable\": {" +
            "\"runAfter\": {}," +
            "\"type\": \"InitializeVariable\"," +
            "\"inputs\": {" +
            "\"variables\": [" +
            "{" +
            "\"name\": \"Variable\"," +
            "\"type\": \"integer\"" +
            "}]}}}";

        [Test]
        public async Task TestDoUntilLimit()
        {
            var loggerMock = TestLogger.Create<DoUntilActionExecutor>();

            var sdm = new Mock<IScopeDepthManager>();

            var expr = new Mock<IExpressionEngine>();
            expr.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns(new ValueContainer(true));

            var doUntil = new DoUntilActionExecutor(sdm.Object, loggerMock, expr.Object);
            doUntil.InitializeActionExecutor("DoUntil", JToken.Parse(DoUntilAction));

            var result = await doUntil.Execute();
            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.NotNull(result.NextAction);

            sdm.Verify(
                x => x.Push(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>(),
                    It.IsAny<IScopeActionExecutor>()),
                Times.Once);

            var exit1 = await doUntil.ExitScope(ActionStatus.Succeeded);
            Assert.NotNull(exit1.NextAction);
            sdm.Verify(
                x => x.Push(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>(),
                    It.IsAny<IScopeActionExecutor>()),
                Times.Exactly(2));
            
            
            var exit2 = await doUntil.ExitScope(ActionStatus.Succeeded);
            Assert.NotNull(exit2.NextAction);
            sdm.Verify(
                x => x.Push(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>(),
                    It.IsAny<IScopeActionExecutor>()),
                Times.Exactly(3));
            
            var exit3 = await doUntil.ExitScope(ActionStatus.Succeeded);
            Assert.IsNull(exit3.NextAction);
            sdm.Verify(
                x => x.Push(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>(),
                    It.IsAny<IScopeActionExecutor>()),
                Times.Exactly(3));
        }

        [Test]
        public async Task TestDoUntilNone()
        {
            var loggerMock = TestLogger.Create<DoUntilActionExecutor>();

            var sdm = new Mock<IScopeDepthManager>();

            var expr = new Mock<IExpressionEngine>();
            expr.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns(new ValueContainer(false));

            var doUntil = new DoUntilActionExecutor(sdm.Object, loggerMock, expr.Object);
            doUntil.InitializeActionExecutor("DoUntil", JToken.Parse(DoUntilAction));

            var result = await doUntil.Execute();
            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.IsNull(result.NextAction);

            sdm.Verify(
                x => x.Push(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>(),
                    It.IsAny<IScopeActionExecutor>()),
                Times.Never);
        }
    }
}