using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// Email Triage Flow Tests
    /// Tests for automated email routing based on keywords
    /// </summary>
    [TestClass]
    public class EmailTriageFlowTests
    {
        #region Action Executors

        /// <summary>
        /// Email Trigger Executor - Simulates receiving an email
        /// </summary>
        private class EmailTriggerExecutor : DefaultBaseActionExecutor
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_office365";
            public static readonly string[] SupportedOperations = { "OnNewEmailV3" };

            private readonly string _subject;
            private readonly string _bodyPreview;
            private readonly string _from;

            public EmailTriggerExecutor(string subject = "Test Email", string bodyPreview = "Test body", string from = "test@example.com")
            {
                _subject = subject;
                _bodyPreview = bodyPreview;
                _from = from;
            }

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult
                {
                    ActionStatus = ActionStatus.Succeeded,
                    ActionOutput = new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"body/id", new ValueContainer("msg-" + Guid.NewGuid().ToString())},
                        {"body/subject", new ValueContainer(_subject)},
                        {"body/bodyPreview", new ValueContainer(_bodyPreview)},
                        {"body/from", new ValueContainer(_from)},
                        {"body/receivedDateTime", new ValueContainer(DateTime.UtcNow.ToString("o"))}
                    })
                });
            }
        }

        /// <summary>
        /// Teams Post Message Executor - Simulates posting to Teams channel
        /// </summary>
        private class TeamsPostMessageExecutor : OpenApiConnectionActionExecutorBase
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_teams";
            public static readonly string[] SupportedOperations = { "PostMessageToChannel" };

            // Static properties to capture the last call for testing
            public static string LastTeamId { get; private set; }
            public static string LastChannelId { get; private set; }
            public static string LastMessage { get; private set; }

            public TeamsPostMessageExecutor(IExpressionEngine expressionEngine)
                : base(expressionEngine) { }

            public override Task<ActionResult> Execute()
            {
                LastTeamId = Parameters["teamId"].GetValue<string>();
                LastChannelId = Parameters["channelId"].GetValue<string>();

                // Extract message content - could be string or complex object
                var messageParam = Parameters["message"];
                if (messageParam.Type == ValueContainer.ValueType.Object)
                {
                    // Complex message with body/content structure
                    LastMessage = messageParam["body/content"]?.GetValue<string>() ?? "No content";
                }
                else
                {
                    // Simple string message
                    LastMessage = messageParam.GetValue<string>();
                }

                Console.WriteLine($"[TEAMS] Posted to Team: {LastTeamId}, Channel: {LastChannelId}");
                Console.WriteLine($"[TEAMS] Message: {LastMessage}");

                return Task.FromResult(new ActionResult
                {
                    ActionStatus = ActionStatus.Succeeded,
                    ActionOutput = new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"body/id", new ValueContainer(Guid.NewGuid().ToString())},
                        {"body/createdDateTime", new ValueContainer(DateTime.UtcNow.ToString("o"))}
                    })
                });
            }

            public static void Reset()
            {
                LastTeamId = null;
                LastChannelId = null;
                LastMessage = null;
            }
        }

        /// <summary>
        /// Forward Email Executor - Simulates forwarding an email
        /// </summary>
        private class ForwardEmailExecutor : OpenApiConnectionActionExecutorBase
        {
            public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_office365";
            public static readonly string[] SupportedOperations = { "ForwardEmail" };

            // Static properties to capture the last call for testing
            public static string LastMessageId { get; private set; }
            public static string LastRecipient { get; private set; }
            public static string LastComment { get; private set; }

            public ForwardEmailExecutor(IExpressionEngine expressionEngine)
                : base(expressionEngine) { }

            public override Task<ActionResult> Execute()
            {
                LastMessageId = Parameters["messageId"].GetValue<string>();

                // Extract recipient - could be direct string or in forwardEmailMessage object
                if (Parameters.ContainsKey("forwardEmailMessage"))
                {
                    var forwardMsg = Parameters["forwardEmailMessage"];
                    LastRecipient = forwardMsg["To"]?.GetValue<string>() ?? "unknown";
                    LastComment = forwardMsg["Comment"]?.GetValue<string>() ?? "";
                }
                else
                {
                    LastRecipient = Parameters["emailAddress"]?.GetValue<string>() ?? "unknown";
                    LastComment = "";
                }

                Console.WriteLine($"[EMAIL] Forwarded message {LastMessageId} to {LastRecipient}");
                if (!string.IsNullOrEmpty(LastComment))
                {
                    Console.WriteLine($"[EMAIL] Comment: {LastComment}");
                }

                return Task.FromResult(new ActionResult
                {
                    ActionStatus = ActionStatus.Succeeded,
                    ActionOutput = new ValueContainer(true)
                });
            }

            public static void Reset()
            {
                LastMessageId = null;
                LastRecipient = null;
                LastComment = null;
            }
        }

        #endregion

        #region Helper Methods

        private IServiceProvider SetupFlowRunner(string emailSubject, string emailBody)
        {
            var services = new ServiceCollection();

            // Create trigger with custom email content
            var trigger = new EmailTriggerExecutor(emailSubject, emailBody, "customer@example.com");

            // Register action executors
            services.AddSingleton<EmailTriggerExecutor>(trigger);
            services.AddFlowActionByApiIdAndOperationsName<EmailTriggerExecutor>(
                EmailTriggerExecutor.ApiId,
                EmailTriggerExecutor.SupportedOperations);

            services.AddFlowActionByApiIdAndOperationsName<TeamsPostMessageExecutor>(
                TeamsPostMessageExecutor.ApiId,
                TeamsPostMessageExecutor.SupportedOperations);

            services.AddFlowActionByApiIdAndOperationsName<ForwardEmailExecutor>(
                ForwardEmailExecutor.ApiId,
                ForwardEmailExecutor.SupportedOperations);

            // Add flow runner
            services.AddFlowRunner();

            return services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [TestInitialize]
        public void TestInitialize()
        {
            // Reset static state before each test
            TeamsPostMessageExecutor.Reset();
            ForwardEmailExecutor.Reset();
        }

        [TestMethod]
        public async Task TestRFQKeywordDetection()
        {
            // Arrange
            var sp = SetupFlowRunner("RFQ: Need pricing for 1000 units", "We need a quote for our order");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify email content was composed
            var emailContent = state.GetOutputs("Initialize_Email_Content");
            Assert.IsNotNull(emailContent);
            Assert.IsTrue(emailContent.GetValue<string>().Contains("rfq"));

            // Verify Teams message was posted
            var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(teamsOutput, "Teams message should have been posted");
            Assert.AreEqual("sales-team-id", TeamsPostMessageExecutor.LastTeamId);
            Assert.AreEqual("sales-channel-id", TeamsPostMessageExecutor.LastChannelId);
            Assert.IsTrue(TeamsPostMessageExecutor.LastMessage.Contains("RFQ"));

            // Verify email was forwarded
            var forwardOutput = state.GetOutputs("Forward_to_Sales");
            Assert.IsNotNull(forwardOutput, "Email should have been forwarded");
            Assert.AreEqual("sales@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestRFPKeywordDetection()
        {
            // Arrange
            var sp = SetupFlowRunner("RFP - Enterprise Software Solution", "Looking for proposals");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify routing to sales team
            var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(teamsOutput);
            Assert.AreEqual("sales@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestInquiryKeywordDetection()
        {
            // Arrange
            var sp = SetupFlowRunner("Product Inquiry", "I have an inquiry about your services");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify routing to sales team
            var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(teamsOutput);
            Assert.AreEqual("sales@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestPolishKeywordDetection()
        {
            // Arrange
            var sp = SetupFlowRunner("Zapytanie ofertowe", "Prosimy o ofertę");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify routing to international sales (Polish)
            var teamsOutput = state.GetOutputs("Post_to_International_Sales_PL");
            Assert.IsNotNull(teamsOutput, "Polish inquiry should post to international channel");
            Assert.AreEqual("intl-team-id", TeamsPostMessageExecutor.LastTeamId);
            Assert.IsTrue(TeamsPostMessageExecutor.LastMessage.Contains("Zapytanie"));

            var forwardOutput = state.GetOutputs("Forward_to_Sales_PL");
            Assert.IsNotNull(forwardOutput, "Polish inquiry should be forwarded");
            Assert.AreEqual("sales-pl@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestGermanKeywordDetection()
        {
            // Arrange
            var sp = SetupFlowRunner("Anfrage für Produkte", "Wir brauchen ein Angebot");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify routing to international sales (German)
            var teamsOutput = state.GetOutputs("Post_to_International_Sales_DE");
            Assert.IsNotNull(teamsOutput, "German inquiry should post to international channel");
            Assert.AreEqual("intl-team-id", TeamsPostMessageExecutor.LastTeamId);
            Assert.IsTrue(TeamsPostMessageExecutor.LastMessage.Contains("Anfrage"));

            var forwardOutput = state.GetOutputs("Forward_to_Sales_DE");
            Assert.IsNotNull(forwardOutput, "German inquiry should be forwarded");
            Assert.AreEqual("sales-de@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestCaseInsensitiveMatching()
        {
            // Arrange - using uppercase keywords
            var sp = SetupFlowRunner("URGENT RFQ REQUEST", "THIS IS AN RFQ");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify that uppercase keywords are still detected
            var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(teamsOutput, "Should detect keywords regardless of case");
            Assert.AreEqual("sales@company.com", ForwardEmailExecutor.LastRecipient);
        }

        [TestMethod]
        public async Task TestKeywordInBody()
        {
            // Arrange - keyword only in body, not subject
            var sp = SetupFlowRunner("General Question", "I would like to submit an RFQ for your services");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify keyword detected in body
            var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(teamsOutput, "Should detect keywords in email body");
        }

        [TestMethod]
        public async Task TestNoKeywordMatch()
        {
            // Arrange - email without any keywords
            var sp = SetupFlowRunner("General Information", "Just asking for general info");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Verify no routing actions were taken
            var salesTeams = state.GetOutputs("Post_to_Sales_Team");
            var polishTeams = state.GetOutputs("Post_to_International_Sales_PL");
            var germanTeams = state.GetOutputs("Post_to_International_Sales_DE");

            Assert.IsNull(salesTeams, "Should not route to sales for non-matching email");
            Assert.IsNull(polishTeams, "Should not route to Polish channel for non-matching email");
            Assert.IsNull(germanTeams, "Should not route to German channel for non-matching email");
        }

        [TestMethod]
        public async Task TestMultipleKeywords()
        {
            // Arrange - email with multiple keywords (should match first condition)
            var sp = SetupFlowRunner("RFQ and Anfrage combined", "This contains both RFP and zapytanie");
            var flowRunner = sp.GetRequiredService<IFlowRunner>();
            flowRunner.InitializeFlowRunner("FlowSamples/EmailTriageFlow.json");

            // Act
            var flowReport = await flowRunner.Trigger();

            // Assert
            var state = sp.GetRequiredService<IState>();

            // Should match the first condition (RFQ/RFP/Inquiry) and not check others
            var salesTeams = state.GetOutputs("Post_to_Sales_Team");
            Assert.IsNotNull(salesTeams, "Should match first keyword condition");
            Assert.AreEqual("sales@company.com", ForwardEmailExecutor.LastRecipient);
        }

        #endregion
    }
}
