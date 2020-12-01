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

            services.Configure<FlowSettings>(x => { });

            services.AddFlowActionByApiIdAndOperationsName<TriggerActionExecutor>(TriggerActionExecutor.ApiId,
                TriggerActionExecutor.SupportedOperations);

            services.AddFlowActionByName<Second>(Second.FlowActionName);
            services.AddFlowActionByApiIdAndOperationsName<Third>(Third.ApiId, Third.SupportedOperations);
            services.AddFlowActionByName<Fourth>(Fourth.FlowActionName);
            services.AddFlowActionByName<Fifth>(Fifth.FlowActionName);

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<FlowRunner>();
            // TODO: Interface der kan give Json documentet

            flowRunner.InitializeFlowRunner(path);

            await flowRunner.Trigger();

            var state = sp.GetRequiredService<IState>();

            Assert.IsTrue(state.GetOutputs("SecondOutput").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("ThirdOutput").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("FourthOutput").Type() == ValueContainer.ValueType.Null);

            Assert.IsTrue(state.GetOutputs("SecondOutput").GetValue<bool>(), "Second action wasn't triggered");
            Assert.IsTrue(state.GetOutputs("ThirdOutput").GetValue<bool>(), "Third action wasn't triggered");
        }

        private class TriggerActionExecutor : DefaultBaseActionExecutor
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";
            public static readonly string[] SupportedOperations = {"SubscribeWebhookTrigger"};

            private readonly IState _state;

            public TriggerActionExecutor(IState state)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            public override Task<ActionResult> Execute()
            {
                var b = new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"body/name", new ValueContainer("Alice Bob")},
                    {"body/accountid", new ValueContainer(Guid.NewGuid().ToString())}
                });

                _state.AddTriggerOutputs(b);

                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
            }
        }

        private class Second : OpenApiConnectionActionExecutorBase
        {
            private readonly IState _state;
            public const string FlowActionName = "Update_Account_-_Invalid_Id";

            public override Task<ActionResult> Execute()
            {
                _state.AddOutputs("SecondOutput", new ValueContainer(true));
                
                Assert.AreEqual(FlowActionName, ActionName);

                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Failed});
            }

            public Second(ExpressionEngine expressionEngine, IState state) : base(expressionEngine)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }
        }

        private class Third : OpenApiConnectionActionExecutorBase
        {
            private readonly IState _state;
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_flowpush";
            public static readonly string[] SupportedOperations = {"SendEmailNotification"};

            public override Task<ActionResult> Execute()
            {
                _state.AddOutputs("ThirdOutput", new ValueContainer(true));
                
                Assert.AreEqual("Send_me_an_email_notification", ActionName);

                Console.WriteLine($"Email Title: {Parameters["NotificationEmailDefinition/notificationSubject"]}");
                Console.WriteLine($"Email Content: {Parameters["NotificationEmailDefinition/notificationBody"]}");
                return Task.FromResult(new ActionResult());
            }

            public Third(ExpressionEngine expressionEngine, IState state) : base(expressionEngine)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }
        }

        private class Fourth : OpenApiConnectionActionExecutorBase
        {
            private readonly IState _state;
            public const string FlowActionName = "Get_a_record_-_Valid_Id";

            public Fourth(ExpressionEngine expressionEngine, IState state) : base(expressionEngine)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            public override Task<ActionResult> Execute()
            {
                _state.AddOutputs("ThirdOutput", new ValueContainer(true));
                
                Assert.AreEqual(FlowActionName, ActionName);

                return Task.FromResult(new ActionResult());
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

            public Fifth(ExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }
    }
}