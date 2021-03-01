using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    public class IfActionExecutorTest
    {
        private Mock<IExpressionEngine> _expressionParserMock;
        private ILogger<IfActionExecutor> _log;
        private Mock<IScopeDepthManager> _sdmMock;
        private IEnumerable<JProperty> _pushedJson;

        private const string TerminateTrue = "TerminateTrue";
        private const string TerminateFalse = "TerminateFalse";

        private readonly string _standardJson =
            $"{{\"actions\":{{\"{TerminateTrue}\":{{\"runAfter\":{{}},\"type\":\"Terminate\",\"inputs\":{{\"runStatus\":\"Succeeded\"}}}}}}," +
            "\"runAfter\":{\"Status_Reason_-_Failed\":[\"Succeeded\"]}," +
            $"\"else\":{{\"actions\":{{\"{TerminateFalse}\":{{\"runAfter\":{{}},\"type\":\"Terminate\",\"inputs\":{{\"runStatus\":\"Succeeded\"}}}}}}}},";

        [SetUp]
        public void Init()
        {
            _expressionParserMock = new Mock<IExpressionEngine>();
            _log = TestLogger.Create<IfActionExecutor>();
            _sdmMock = new Mock<IScopeDepthManager>();

            _sdmMock.Setup(x => x.Push(
                    It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null))
                .Callback<string, IEnumerable<JProperty>, IScopeActionExecutor>((name, json, handler) =>
                {
                    _pushedJson = json;
                });

            _expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) => new ValueContainer(s));
        }

        [Test]
        public async Task BasicIfActionTest()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/name']"))
                .Returns(new ValueContainer());
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@null")).Returns(new ValueContainer());

            var ifJson =
                _standardJson +
                "\"expression\":{\"not\":{\"equals\":[\"@triggerOutputs()?['body/name']\",\"@null\"]}},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateFalse, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateFalse));
        }

        [Test]
        public async Task Complex1IfActionTest()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/telephone1']"))
                .Returns(new ValueContainer("28984323"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/fullname']"))
                .Returns(new ValueContainer("Dwight Schrute"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/firstname']"))
                .Returns(new ValueContainer("Dwight"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/lastname']"))
                .Returns(new ValueContainer("Schrute"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/emailaddress1']"))
                .Returns(new ValueContainer("dwight.schrute@dundermifflininc.com"));

            var ifJson =
                _standardJson +
                "\"expression\":{" +
                "\"or\":[" +
                "{\"equals\":[\"@triggerOutputs()?['body/telephone1']\",\"28984323\"]}," +
                "{\"not\":{\"equals\":[\"@triggerOutputs()?['body/fullname']\",\"Dwight Schrute\"]}}," +
                "{\"and\":[{\"startsWith\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/firstname']\"]},{\"contains\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/lastname']\"]}]}," +
                "{\"and\":[{\"contains\":[\"@triggerOutputs()?['body/fullname']\",\"@triggerOutputs()?['body/firstname']\"]},{\"greater\":[\"@length(triggerOutputs()?['body/fullname'])\",\"@length(triggerOutputs()?['body/firstname'])\"]}]}]}," +
                "\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);
            
            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task ContainsIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"contains\":[\"John Doe\",\"n D\"]},{\"not\":{\"contains\":[\"John Doe\",\"n_D\"]}}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task EqualsIfActionTest()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/telephone1']"))
                .Returns(new ValueContainer("28984323"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/fullname']"))
                .Returns(new ValueContainer("Dwight Schrute"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/firstname']"))
                .Returns(new ValueContainer("Dwight"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/lastname']"))
                .Returns(new ValueContainer("Schrute"));
            _expressionParserMock.Setup(x => x.ParseToValueContainer("@triggerOutputs()?['body/emailaddress1']"))
                .Returns(new ValueContainer("dwight.schrute@dundermifflininc.com"));


            var ifJson =
                _standardJson +
                "\"expression\":{\"or\":[{\"equals\":[\"@triggerOutputs()?['body/telephone1']\",\"28984323\"]}," +
                "{\"not\":{\"equals\":[\"@triggerOutputs()?['body/fullname']\",\"Dwight Schrute\"]}}," +
                "{\"and\":[{\"startsWith\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/firstname']\"]}," +
                "{\"contains\":[\"@triggerOutputs()?['body/emailaddress1']\",\"@triggerOutputs()?['body/lastname']\"]}]}," +
                "{\"and\":[{\"contains\":[\"@triggerOutputs()?['body/fullname']\",\"@triggerOutputs()?['body/firstname']\"]}," +
                "{\"greater\":[\"@length(triggerOutputs()?['body/fullname'])\",\"@length(triggerOutputs()?['body/firstname'])\"]}]}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task GreaterIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"greater\":[10,8]},{\"not\":{\"greater\":[10,12]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task GreaterOrEqualsIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"greaterOrEquals\":[12,12]},{\"greaterOrEquals\":[14,12]},{\"not\":{\"greaterOrEquals\":[10,12]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task LessIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"less\":[10,12]},{\"not\":{\"less\":[10,8]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task LessOrEqualsIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"lessOrEquals\":[10,10]},{\"lessOrEquals\":[10,12]},{\"not\":{\"lessOrEquals\":[10,8]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task StartsWithIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"startsWith\":[\"John Doe\",\"John\"]},{\"not\":{\"startsWith\":[\"John Doe\",\"Doe\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task EndsWithIfActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"endsWith\":[\"Jane Doe\",\"Doe\"]},{\"not\":{\"endsWith\":[\"John Doe\",\"John\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task NoElseTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"endsWith\":[\"Jane Doe\",\"Nope\"]},{\"not\":{\"endsWith\":[\"John Doe\",\"John\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateFalse, result.NextAction);
            
            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateFalse));
        }

        [Test]
        public async Task EmptyActionTest()
        {
            var ifJson =
                _standardJson +
                "\"expression\":{\"and\":[{\"endsWith\":[\"Jane Doe\",\"Doe\"]},{\"not\":{\"endsWith\":[\"John Doe\",\"John\"]},}]},\"type\":\"If\"}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task BooleanSpecialCaseTest()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) =>
                {
                    return s switch
                    {
                        "@true" => new ValueContainer(true),
                        "@false" => new ValueContainer(false),
                        _ => new ValueContainer(s)
                    };
                });
            var ifJson =
                _standardJson +
                "\"expression\":{\"equals\": [\"@true\", \"true\"]}}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task BooleanSpecialCaseTest2()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) =>
                {
                    return s switch
                    {
                        "@true" => new ValueContainer(true),
                        "@false" => new ValueContainer(false),
                        _ => new ValueContainer(s)
                    };
                });
            var ifJson =
                _standardJson +
                "\"expression\":{\"equals\": [\"@false\", \"false\"]}}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateTrue, result.NextAction);

            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateTrue));
        }

        [Test]
        public async Task BooleanSpecialCaseTest3()
        {
            _expressionParserMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>()))
                .Returns((string s) =>
                {
                    return s switch
                    {
                        "@true" => new ValueContainer(true),
                        "@false" => new ValueContainer(false),
                        _ => new ValueContainer(s)
                    };
                });

            var ifJson =
                _standardJson +
                "\"expression\":{\"equals\": [\"true\", \"@false\"]}}";

            var ifAction = new IfActionExecutor(_expressionParserMock.Object, _log, _sdmMock.Object);
            ifAction.InitializeActionExecutor("IfActionTest", JToken.Parse(ifJson));

            var result = await ifAction.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);
            Assert.AreEqual(TerminateFalse, result.NextAction);
            
            _sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), null), Times.Once);
            Assert.NotNull(_pushedJson.FirstOrDefault(x => x.Name == TerminateFalse));
        }
    }
}