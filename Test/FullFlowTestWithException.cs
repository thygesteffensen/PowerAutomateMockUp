using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.Implementations.ConversionFunctions;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;

namespace Test
{
    [TestFixture]
    public class FullFlowTestWithException
    {
        private static readonly string TestFlowPath = System.IO.Path.GetFullPath(@"FlowSamples");

        [Test]
        public async Task TestFlowFalse()
        {
            var path = @$"{TestFlowPath}\PowerAutomateMockUpSampleFlow.json";

            var services = new ServiceCollection();

            services.AddFlowActionByApiIdAndOperationsName<TriggerActionExecutor>(TriggerActionExecutor.ApiId,
                TriggerActionExecutor.SupportedOperations);

            services.AddFlowActionByName<Second>(Second.FlowActionName);
            services.AddFlowActionByApiIdAndOperationsName<Third>(Third.ApiId, Third.SupportedOperations);

            services.Configure<FlowSettings>(x => { x.FailOnUnknownAction = false; });

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<FlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            Assert.ThrowsAsync<PowerAutomateMockUpException>(async () => await flowRunner.Trigger());
        }

        private class TriggerActionExecutor : DefaultBaseActionExecutor
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";
            public static readonly string[] SupportedOperations = {"SubscribeWebhookTrigger"};

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult
                {
                    ActionStatus = ActionStatus.Succeeded, ActionOutput = new ValueContainer(
                        new Dictionary<string, ValueContainer>
                        {
                            {"body/name", new ValueContainer("Alice Bob")},
                            {"body/accountid", new ValueContainer(Guid.NewGuid().ToString())}
                        })
                });
            }
        }

        private class Second : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Update_Account_-_Invalid_Id";

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult
                    {ActionStatus = ActionStatus.Failed, ActionOutput = new ValueContainer(true)});
            }

            public Second(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }

        private class Third : OpenApiConnectionActionExecutorBase
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_flowpush";
            public static readonly string[] SupportedOperations = {"SendEmailNotification"};

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Failed});
            }

            public Third(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }
    }
}