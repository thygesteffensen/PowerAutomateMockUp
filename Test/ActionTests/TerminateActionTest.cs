using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations.ControlActions;

namespace Test.ActionTests
{
    [TestFixture]
    public class TerminateActionTest
    {
        [Test]
        public async Task TerminateSucceededTest()
        {
            var expressionMock = new Mock<IExpressionEngine>();
            expressionMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns<string>(input => new ValueContainer(input));

            var action =
                new TerminateActionExecutor(TestLogger.Create<TerminateActionExecutor>(), expressionMock.Object);

            var json =
                "{\"Terminate\": { \"runAfter\": {}, \"type\": \"Terminate\", \"inputs\": { \"runStatus\": \"Succeeded\" } } }";
            action.InitializeActionExecutor("TerminateAction", JToken.Parse(json).First?.First);

            var resp = await action.Execute();

            Assert.AreEqual(false, resp.ContinueExecution);
            Assert.AreEqual(ActionStatus.Succeeded, resp.ActionStatus);
        }

        [Test]
        public void TerminateFailedNoMessageTest()
        {
            var collection = new ServiceCollection();
            collection.AddFlowRunner();

            var sp = collection.BuildServiceProvider();
            var actionExecutorFactory = sp.GetRequiredService<IActionExecutorFactory>();
            var action = actionExecutorFactory.ResolveActionByType("Terminate");

            var json =
                "{\"Terminate\": { \"runAfter\": {}, \"type\": \"Terminate\", \"inputs\": { \"runStatus\": \"Failed\", \"runError\": { \"code\": \"1234\" } } } }";
            action.InitializeActionExecutor("TerminateAction", JToken.Parse(json).First.First);

            var exception = Assert.ThrowsAsync<PowerAutomateMockUpException>(async () => { await action.Execute(); });

            Assert.AreEqual("Terminate action with status: Failed. Error code: 1234.", exception.Message);
        }
    }
}