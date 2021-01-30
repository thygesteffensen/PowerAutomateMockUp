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
    public class SwitchActionTest
    {
        private string SwitchAction =
            "{\"Switch\": {" +
            "\"runAfter\": {" +
            "\"List_records\": [" +
            "\"Succeeded\"" +
            "]" +
            "}," +
            "\"cases\": {" +
            "\"Case\": {" +
            "\"case\": \"CaseWithoutActions\"," +
            "\"actions\": {}" +
            "}," +
            "\"Case_2\": {" +
            "\"case\": \"CaseWithActions\"," +
            "\"actions\": {" +
            "\"Update_a_record\": {" +
            "\"runAfter\": {}," +
            "\"type\": \"OpenApiConnection\"," +
            "\"inputs\": {" +
            "\"host\": {" +
            "\"connectionName\": \"shared_commondataserviceforapps\"," +
            "\"operationId\": \"UpdateRecord\"," +
            "\"apiId\": \"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"" +
            "}," +
            "\"parameters\": {" +
            "\"entityName\": \"accounts\"," +
            "\"recordId\": \"@customExpressionRecordId\"" +
            "}," +
            "\"authentication\": {" +
            "\"type\": \"Raw\"," +
            "\"value\": \"@json(decodeBase64(triggerOutputs().headers['X-MS-APIM-Tokens']))['$ConnectionKey']\"" +
            "}}}}}}," +
            "\"default\": {" +
            "\"actions\": {}" +
            "}," +
            "\"expression\": \"@customExpression\"," +
            "\"type\": \"Switch\"" +
            "}}";

        [Test]
        public async Task SwitchTestWithNoActions()
        {
            var logger = TestLogger.Create<SwitchActionExecutor>();

            var sdm = new Mock<IScopeDepthManager>();

            var expressionEngine = new Mock<IExpressionEngine>();

            expressionEngine.Setup(x => x.Parse(It.IsAny<string>())).Returns("CaseWithoutActions");

            var switchActionExecutor = new SwitchActionExecutor(sdm.Object, logger, expressionEngine.Object);
            switchActionExecutor.InitializeActionExecutor("ActionName", JToken.Parse(SwitchAction));

            var result = await switchActionExecutor.Execute();

            Assert.IsNull(result.NextAction);
            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);

            sdm.Verify(
                x => x.Push(It.IsAny<string>(),
                    It.IsAny<IEnumerable<JProperty>>()
                    , It.IsAny<IScopeActionExecutor>()),
                Times.Never);

            expressionEngine.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SwitchTestWithActions()
        {
            var logger = TestLogger.Create<SwitchActionExecutor>();

            var sdm = new Mock<IScopeDepthManager>();

            var expressionEngine = new Mock<IExpressionEngine>();

            expressionEngine.Setup(x => x.Parse(It.IsAny<string>())).Returns("CaseWithActions");

            var switchActionExecutor = new SwitchActionExecutor(sdm.Object, logger, expressionEngine.Object);
            switchActionExecutor.InitializeActionExecutor("ActionName", JToken.Parse(SwitchAction));

            var result = await switchActionExecutor.Execute();

            Assert.AreEqual("Update_a_record", result.NextAction);
            Assert.AreEqual(ActionStatus.Succeeded, result.ActionStatus);

            sdm.Verify(
                x => x.Push(It.IsAny<string>(), It.IsAny<IEnumerable<JProperty>>(), It.IsAny<IScopeActionExecutor>()),
                Times.Once);

            expressionEngine.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
        }
    }
}