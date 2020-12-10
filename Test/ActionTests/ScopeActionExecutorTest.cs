using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace Test.ActionTests
{
    [TestFixture]
    public class ScopeActionExecutorTest
    {
        [Test]
        public async Task TestBasicScopeUseCase()
        {
            var logger = TestLogger.Create<ScopeDepthManager>();

            var scopeDepthManager = new ScopeDepthManager(logger);
            scopeDepthManager.Push("rootScope", new List<JProperty>(), null);

            var t = new Mock<IScopeActionExecutor>();
            t.Setup(x => x.ExitScope(ActionStatus.Succeeded)).Returns(Task.FromResult(new ActionResult()));

            scopeDepthManager.Push("testScope", new List<JProperty>(), t.Object);

            var t1 = await scopeDepthManager.TryPopScope(ActionStatus.Succeeded);

            t.Verify(x => x.ExitScope(ActionStatus.Succeeded));
            
            Assert.AreEqual(t1.ActionStatus, ActionStatus.Succeeded);
            Assert.AreEqual(t1.NextAction, "testScope");
            Assert.AreEqual(t1.ContinueExecution, true);
        }
    }
}