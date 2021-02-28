using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;

namespace Test
{
    [TestFixture]
    public class TestValueContainerTrigger
    {
        private static readonly string TestFlowPath = System.IO.Path.GetFullPath(@"FlowSamples");

        [Test]
        public async Task TestFlowFalse()
        {
            // Arrange
            var path = @$"{TestFlowPath}\ContactTrigger.json";

            var services = new ServiceCollection();

            services.AddFlowActionByName<UpdateRowActionExecutor>(UpdateRowActionExecutor.FlowActionName);
            services.AddFlowActionByName<SendNotification>(SendNotification.FlowActionName);

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<IFlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            var triggerOutput = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {
                    "body", new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"firstname", new ValueContainer("John")},
                        // {"lastname", new ValueContainer("Doe")}
                    })
                }
            });

            // Act
            await flowRunner.Trigger(triggerOutput);

            // Assert
            var state = sp.GetRequiredService<IState>();

            Assert.IsTrue(state.GetOutputs(SendNotification.FlowActionName).Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs(UpdateRowActionExecutor.FlowActionName).Type() == ValueContainer.ValueType.Null);

            Assert.IsTrue(state.GetOutputs(SendNotification.FlowActionName).GetValue<bool>(),
                "Second action wasn't triggered");
        }
    }

    public class UpdateRowActionExecutor : OpenApiConnectionActionExecutorBase
    {
        public static string FlowActionName = "Create_a_new_row_-_Create_greeting_note";

        public UpdateRowActionExecutor(IExpressionEngine expressionEngine) : base(expressionEngine)
        {
        }

        public override Task<ActionResult> Execute()
        {
            return Task.FromResult(new ActionResult
            {
                ActionOutput = new ValueContainer(true)
            });
        }
    }
    
    public class SendNotification : OpenApiConnectionActionExecutorBase
    {
        public static string FlowActionName = "Send_me_an_email_notification";
        
        public SendNotification(IExpressionEngine expressionEngine) : base(expressionEngine)
        {
        }

        public override Task<ActionResult> Execute()
        {
            return Task.FromResult(new ActionResult
            {
                ActionOutput = new ValueContainer(true)
            });
        }
    }
}