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
    public class ForEachActionExecutorTest
    {
        private const string ForEachAction =
            "{" +
            "\"foreach\":\"@outputs('List_records')?['body/value']\"," +
            "\"actions\":{" +
            "\"Update_a_record\":{" +
            "\"runAfter\":{}," +
            "\"type\":\"OpenApiConnection\"," +
            "\"inputs\":{" +
            "\"host\":{" +
            "\"connectionName\":\"shared_commondataserviceforapps\"," +
            "\"operationId\":\"UpdateRecord\"," +
            "\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"" +
            "}," +
            "\"parameters\":{" +
            "\"entityName\":\"accounts\"," +
            "\"recordId\":\"@items('Apply_to_each')?['accountid']\"" +
            "}," +
            "\"authentication\":{" +
            "\"type\":\"Raw\"," +
            "\"value\":\"@json(decodeBase64(triggerOutputs().headers['X-MS-APIM-Tokens']))['$ConnectionKey']\"" +
            "}" +
            "}" +
            "}" +
            "}," +
            "\"runAfter\":{" +
            "\"List_records\":[" +
            "\"Succeeded\"" +
            "]" +
            "}," +
            "\"type\":\"Foreach\"" +
            "}";

        [TestCase]
        public async Task Test()
        {
            var loggerMock = TestLogger.Create<ForEachActionExecutor>();

            var initialList = new ValueContainer(new[]
            {
                new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"item1", new ValueContainer("Value1")}
                }),
                new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"item2", new ValueContainer("Value2")}
                }),
                new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"item3", new ValueContainer("Value3")}
                }),
            });

            var stateMock = new Mock<IState>();

            stateMock.Setup(x => x.GetOutputs("")).Returns(initialList);

            var sdmMock = new Mock<IScopeDepthManager>();


            var forEachActionExecutor = new ForEachActionExecutor(stateMock.Object, sdmMock.Object, loggerMock);
            forEachActionExecutor.InitializeActionExecutor("Foreach", JToken.Parse(ForEachAction));

            var response = await forEachActionExecutor.Execute();

            Assert.AreEqual("Update_a_record", response.NextAction);

            stateMock.Verify(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()), Times.Exactly(1));
            sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(),
                It.Is<ForEachActionExecutor>(actionExecutor => 
                    actionExecutor.Equals(forEachActionExecutor))), Times.Exactly(1));

            var exitAnswer1 = await forEachActionExecutor.ExitScope(ActionStatus.Succeeded);
            Assert.AreEqual("Update_a_record", exitAnswer1.NextAction);

            stateMock.Verify(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()), Times.Exactly(2));

            var exitAnswer2 = await forEachActionExecutor.ExitScope(ActionStatus.Succeeded);
            sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(),
                It.Is<ForEachActionExecutor>(actionExecutor => 
                    actionExecutor.Equals(forEachActionExecutor))), Times.Exactly(2));
            
            Assert.AreEqual("Update_a_record", exitAnswer2.NextAction);

            stateMock.Verify(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()), Times.Exactly(3));
            sdmMock.Verify(x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(),
                It.Is<ForEachActionExecutor>(actionExecutor => 
                    actionExecutor.Equals(forEachActionExecutor))), Times.Exactly(3));
            
            var exitAnswer3 = await forEachActionExecutor.ExitScope(ActionStatus.Succeeded);
            Assert.AreEqual(null, exitAnswer3.NextAction);
        }
    }
}