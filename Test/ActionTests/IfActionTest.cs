using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace Test.ActionTests
{
    [TestFixture]
    public class IfActionTest
    {
        [Test]
        public async Task BasicIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/name']"))
                .Returns(new ValueContainer());
            expressionParserMock.Setup(x => x.ParseToValueContainer("@null")).Returns(new ValueContainer());

            const string ifJson =
                "{\"actions\":{}," +
                "\"runAfter\":{}," +
                "\"else\":{\"actions\":{\"Terminate\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"not\":{\"equals\":[\"@triggerOutputs()?['body/name']\",\"@null\"]}},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("Terminate", result.NextAction);
        }

        [Test]
        public async Task Complex1IfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/telephone1']"))
                .Returns(new ValueContainer("28984323"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/fullname']"))
                .Returns(new ValueContainer("Dwight Schrute"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/firstname']"))
                .Returns(new ValueContainer("Dwight"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/lastname']"))
                .Returns(new ValueContainer("Schrute"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/emailaddress1']"))
                .Returns(new ValueContainer("dwight.schrute@dundermifflininc.com"));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{" +
                "\"or\":[" +
                "{\"equals\":[\"@triggerOutputs()?['body/telephone1']\",\"28984323\"]}," +
                "{\"not\":{\"equals\":[\"@triggerOutputs()?['body/fullname']\",\"Dwight Schrute\"]}}," +
                "{\"and\":[{\"startsWith\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/firstname']\"]},{\"contains\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/lastname']\"]}]}," +
                "{\"and\":[{\"contains\":[\"@triggerOutputs()?['body/fullname']\",\"@triggerOutputs()?['body/firstname']\"]},{\"greater\":[\"@length(triggerOutputs()?['body/fullname'])\",\"@length(triggerOutputs()?['body/firstname'])\"]}]}]}," +
                "\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task ContainsIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"contains\":[\"John Doe\",\"n D\"]},{\"not\":{\"contains\":[\"John Doe\",\"n_D\"]}}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task EqualsIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/telephone1']"))
                .Returns(new ValueContainer("28984323"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/fullname']"))
                .Returns(new ValueContainer("Dwight Schrute"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/firstname']"))
                .Returns(new ValueContainer("Dwight"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/lastname']"))
                .Returns(new ValueContainer("Schrute"));
            expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/emailaddress1']"))
                .Returns(new ValueContainer("dwight.schrute@dundermifflininc.com"));


            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"or\":[{\"equals\":[\"@triggerOutputs()?['body/telephone1']\",\"28984323\"]}," +
                "{\"not\":{\"equals\":[\"@triggerOutputs()?['body/fullname']\",\"Dwight Schrute\"]}}," +
                "{\"and\":[{\"startsWith\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/firstname']\"]}," +
                "{\"contains\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/lastname']\"]}]}," +
                "{\"and\":[{\"contains\":[\"@triggerOutputs()?['body/fullname']\",\"@triggerOutputs()?['body/firstname']\"]}," +
                "{\"greater\":[\"@length(triggerOutputs()?['body/fullname'])\",\"@length(triggerOutputs()?['body/firstname'])\"]}]}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task GreaterIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s, true));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"greater\":[10,8]},{\"not\":{\"greater\":[10,12]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task GreaterOrEqualsIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s, true));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"greaterOrEquals\":[12,12]},{\"greaterOrEquals\":[14,12]},{\"not\":{\"greaterOrEquals\":[10,12]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task LessIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s, true));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"less\":[10,12]},{\"not\":{\"less\":[10,8]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task LessOrEqualsIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s, true));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"lessOrEquals\":[10,10]},{\"lessOrEquals\":[10,12]},{\"not\":{\"lessOrEquals\":[10,8]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task StartsWithIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"startsWith\":[\"John Doe\",\"John\"]},{\"not\":{\"startsWith\":[\"John Doe\",\"Doe\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }

        [Test]
        public async Task EndsWithIfActionTest()
        {
            var expressionParserMock = new Mock<IExpressionEngine>();
            var log = TestLogger.Create<IfActionExecutor>();

            expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));

            const string ifJson =
                "{\"actions\":{\"TerminateTrue\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}," +
                "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
                "\"else\":{\"actions\":{\"TerminateFalse\":{\"runAfter\":{},\"type\":\"Terminate\",\"inputs\":{\"runStatus\":\"Succeeded\"}}}}," +
                "\"expression\":{\"and\":[{\"endsWith\":[\"Jane Doe\",\"Doe\"]},{\"not\":{\"endsWith\":[\"John Doe\",\"John\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(expressionParserMock.Object, log);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual("TerminateTrue", result.NextAction);
        }
    }
}