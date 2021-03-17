using System;
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
    public class FullFlowTestV2
    {
        private static readonly string TestFlowPath = System.IO.Path.GetFullPath(@"FlowSamples");

        [Test]
        public async Task TestFlowFalse()
        {
            var path = @$"{TestFlowPath}/PowerAutomateMockUpSampleFlow.json";

            var services = new ServiceCollection();
            services.AddFlowRunner();

            const string authentication = "Custom value";
            services.Configure<WorkflowParameters>(x => x.Parameters = new Dictionary<string, ValueContainer>
            {
                {"$authentication", new ValueContainer(authentication)}
            });

            services.AddFlowActionByName<UpdateAccountInvalidId>(UpdateAccountInvalidId.FlowActionName);
            services.AddFlowActionByApiIdAndOperationsName<SendEmailNotification>(SendEmailNotification.ApiId,
                SendEmailNotification.SupportedOperations);
            services.AddFlowActionByName<GetRecordValidId>(GetRecordValidId.FlowActionName);
            services.AddFlowActionByName<UpdateAccountValidId>(UpdateAccountValidId.FlowActionName);
            services.AddFlowActionByName<SendOutWarning>(SendOutWarning.FlowActionName);


            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<IFlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            var flowResult = await flowRunner.Trigger(new ValueContainer(
                new Dictionary<string, ValueContainer>
                {
                    {"body/name", new ValueContainer("Alice Bob")},
                    {"body/accountid", new ValueContainer(Guid.NewGuid().ToString())}
                }));

            Assert.AreEqual(7, flowResult.NumberOfExecutedActions);

            const string actionName = "Send_me_an_email_notification";

            FlowAssert.AssertActionWasTriggered(flowResult, actionName);
            FlowAssert.AssertFlowParameters(flowResult, actionName,
                // TODO: Overwrite ContainsKey to also include children dicts for easier use    
                x => x.ContainsKey("parameters/NotificationEmailDefinition"),
                x => x["parameters/NotificationEmailDefinition/notificationSubject"]
                    .Equals(new ValueContainer("A new Account have been added")),
                x => x["parameters/NotificationEmailDefinition/notificationSubject"].GetValue<string>() ==
                     "A new Account have been added");

            Assert.IsTrue(flowResult.ActionStates.ContainsKey(actionName), "Action is expected to be triggered.");
            Assert.NotNull(flowResult.ActionStates[actionName].ActionInput?["parameters"], "Action input is expected.");
            var actionInput = flowResult.ActionStates[actionName].ActionInput?["parameters"].AsDict();
            Assert.IsTrue(actionInput.ContainsKey("NotificationEmailDefinition"),
                "Action input should contain this object.");
            var notification = actionInput["NotificationEmailDefinition"].AsDict();
            Assert.AreEqual("A new Account have been added", notification["notificationSubject"].GetValue<string>(),
                "Asserting the input");

            Assert.IsFalse(flowResult.ActionStates.ContainsKey(SendOutWarning.FlowActionName),
                "Action is not expected to be triggered.");

            var getRecordValidIdActionAuth =
                flowResult.ActionStates["Get_a_record_-_Valid_Id"].ActionInput?["authentication"];
            Assert.IsNotNull(getRecordValidIdActionAuth);
            Assert.AreEqual(ValueContainer.ValueType.String, getRecordValidIdActionAuth.Type());
            Assert.AreEqual(authentication, getRecordValidIdActionAuth.GetValue<string>());
        }

        private class UpdateAccountInvalidId : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Update_Account_-_Invalid_Id";

            public UpdateAccountInvalidId(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult
                {
                    ActionStatus = ActionStatus.Failed,
                    ActionOutput = new ValueContainer(true)
                });
            }
        }

        private class SendEmailNotification : OpenApiConnectionActionExecutorBase
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_flowpush";
            public static readonly string[] SupportedOperations = {"SendEmailNotification"};

            public SendEmailNotification(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }

            public override Task<ActionResult> Execute()
            {
                Console.WriteLine($"Email Title: {Parameters["NotificationEmailDefinition/notificationSubject"]}");
                Console.WriteLine($"Email Content: {Parameters["NotificationEmailDefinition/notificationBody"]}");

                return Task.FromResult(new ActionResult {ActionOutput = new ValueContainer(true)});
            }
        }

        private class GetRecordValidId : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Get_a_record_-_Valid_Id";

            public GetRecordValidId(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult {ActionOutput = new ValueContainer(true)});
            }
        }

        private class UpdateAccountValidId : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Update_Account_-_Valid_Id";

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult());
            }

            public UpdateAccountValidId(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }
        }

        private class SendOutWarning : OpenApiConnectionActionExecutorBase
        {
            public const string FlowActionName = "Send_an_error_message_to_owner";

            public SendOutWarning(IExpressionEngine expressionEngine) : base(expressionEngine)
            {
            }

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult());
            }
        }
    }
}