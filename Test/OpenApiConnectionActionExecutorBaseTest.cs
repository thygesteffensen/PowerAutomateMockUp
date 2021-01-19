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
    public class OpenApiConnectionActionExecutorBaseTest
    {
        private static readonly string TestFlowPath = System.IO.Path.GetFullPath(@"FlowSamples");

        [Test]
        public async Task TestFlowFalse()
        {
            var path = @$"{TestFlowPath}\Pure CDS ce.json";

            var services = new ServiceCollection();

            services.AddFlowActionByApiIdAndOperationsName<Trigger>(Trigger.ApiId,
                Trigger.SupportedOperations);

            services.AddFlowActionByApiIdAndOperationsName<OpenApiConnectionAction>(OpenApiConnectionAction.ApiId, OpenApiConnectionAction.OperationIds);

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<FlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            await flowRunner.Trigger();
        }

        private class Trigger : DefaultBaseActionExecutor
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";
            public static readonly string[] SupportedOperations = {"SubscribeWebhookTrigger"};

            private readonly IState _state;

            public Trigger(IState state)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            public override Task<ActionResult> Execute()
            {
                var contact = new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"body/name", new ValueContainer("Alice Bob")},
                    {"body/contactid", new ValueContainer(Guid.NewGuid().ToString())}
                });

                _state.AddTriggerOutputs(contact);

                return Task.FromResult(new ActionResult());
            }
        }

        private class OpenApiConnectionAction : OpenApiConnectionActionExecutorBase
        {
            private readonly ITriggerOutputsRetriever _triggerOutputsRetriever;
            public static readonly string ApiId = "/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps";
            public static readonly string[] OperationIds = {"UpdateRecord"};

            public OpenApiConnectionAction(IExpressionEngine expressionEngine, ITriggerOutputsRetriever triggerOutputsRetriever) : base(expressionEngine)
            {
                _triggerOutputsRetriever = triggerOutputsRetriever ?? throw new ArgumentNullException(nameof(triggerOutputsRetriever));
            }

            public override Task<ActionResult> Execute()
            {
                var outputs =_triggerOutputsRetriever.GetTriggerOutputs()["body"];
                Assert.AreEqual("Alice Bob", outputs["name"].GetValue<string>());

                return Task.FromResult(new ActionResult());
            }
        }
    }
}