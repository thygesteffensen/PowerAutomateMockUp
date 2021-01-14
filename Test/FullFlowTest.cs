using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;

namespace Test
{
    [TestFixture]
    public class FullFlowTest
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
            services.AddFlowActionByName<Fourth>(Fourth.FlowActionName);
            services.AddFlowActionByName<Fifth>(Fifth.FlowActionName);

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<FlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            await flowRunner.Trigger();

            var state = sp.GetRequiredService<IState>();

            Assert.IsTrue(state.GetOutputs("Update_Account_-_Invalid_Id").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("Send_me_an_email_notification").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("Update_Account_-_Valid_Id").Type() == ValueContainer.ValueType.Null);

            Assert.IsTrue(state.GetOutputs("Update_Account_-_Invalid_Id").GetValue<bool>(),
                "Second action wasn't triggered");
            Assert.IsTrue(state.GetOutputs("Send_me_an_email_notification").GetValue<bool>(),
                "Third action wasn't triggered");
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
                Assert.AreEqual(FlowActionName, ActionName);

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
                Assert.AreEqual("Send_me_an_email_notification", ActionName);

                Console.WriteLine($"Email Title: {Parameters["NotificationEmailDefinition/notificationSubject"]}");
                Console.WriteLine($"Email Content: {Parameters["NotificationEmailDefinition/notificationBody"]}");
                return Task.FromResult(new ActionResult {ActionOutput = new ValueContainer(true)});
            }

            public Third(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }

        private class Fourth : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Get_a_record_-_Valid_Id";

            public Fourth(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }

            public override Task<ActionResult> Execute()
            {
                Assert.AreEqual(FlowActionName, ActionName);

                Assert.AreEqual("accounts", Parameters["entityName"].GetValue<string>());
                Assert.AreNotEqual(ValueContainer.ValueType.Null, Parameters["recordId"]);

                return Task.FromResult(new ActionResult {ActionOutput = new ValueContainer(true)});
            }
        }

        private class Fifth : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Update_Account_-_Valid_Id";

            public override Task<ActionResult> Execute()
            {
                Assert.AreEqual(FlowActionName, ActionName);

                return Task.FromResult(new ActionResult());
            }

            public Fifth(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }
    }
}