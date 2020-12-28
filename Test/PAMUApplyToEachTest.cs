using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PAMUApplyToEachTest
    {
        private static readonly string TestFlowPath = System.IO.Path.GetFullPath(@"FlowSamples");

        [Test]
        public async Task TestForEachFlow()
        {
            var path = @$"{TestFlowPath}\PAMUApplyToEach.json";

            var services = new ServiceCollection();

            services.Configure<FlowSettings>(x => x.FailOnUnknownAction = true);

            services.AddFlowActionByName<ManualTrigger>(ManualTrigger.TriggerName);
            services.AddFlowActionByName<ListRecordsAccounts>(ListRecordsAccounts.FlowActionName);
            services.AddFlowActionByName<UpdateRecord>(UpdateRecord.FlowActionName);

            services.AddFlowRunner();

            var sp = services.BuildServiceProvider();
            var flowRunner = sp.GetRequiredService<FlowRunner>();

            flowRunner.InitializeFlowRunner(path);

            await flowRunner.Trigger();

            var state = sp.GetRequiredService<IState>();

            /*
            Assert.IsTrue(state.GetOutputs("SecondOutput").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("ThirdOutput").Type() != ValueContainer.ValueType.Null);
            Assert.IsTrue(state.GetOutputs("FourthOutput").Type() == ValueContainer.ValueType.Null);

            Assert.IsTrue(state.GetOutputs("SecondOutput").GetValue<bool>(), "Second action wasn't triggered");
            Assert.IsTrue(state.GetOutputs("ThirdOutput").GetValue<bool>(), "Third action wasn't triggered");
        */
        }

        private class ManualTrigger : DefaultBaseActionExecutor
        {
            public const string TriggerName = "manual";

            public override Task<ActionResult> Execute()
            {
                return Task.FromResult(new ActionResult());
            }
        }

        private class ListRecordsAccounts : OpenApiConnectionActionExecutorBase
        {
            private readonly IState _state;
            public const string FlowActionName = "List_records";

            public static readonly Guid[] Guids = {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};

            public override Task<ActionResult> Execute()
            {
                var accountList = new ValueContainer(new[]
                {
                    new ValueContainer(
                        new Dictionary<string, ValueContainer>
                        {
                            {"accountid", new ValueContainer(Guids[0].ToString())}
                        }),
                    new ValueContainer(
                        new Dictionary<string, ValueContainer>
                        {
                            {"accountid", new ValueContainer(Guids[1].ToString())}
                        }),
                    new ValueContainer(
                        new Dictionary<string, ValueContainer>
                        {
                            {"accountid", new ValueContainer(Guids[2].ToString())}
                        })
                });

                _state.AddOutputs("List_records", new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"body/value", accountList}
                }));


                return Task.FromResult(new ActionResult {ActionStatus = ActionStatus.Succeeded});
            }

            public ListRecordsAccounts(IExpressionEngine expressionEngine, IState state) : base(expressionEngine)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }
        }


        private class UpdateRecord : OpenApiConnectionActionExecutorBase
        {
            private readonly IState _state;
            public const string FlowActionName = "Update_a_record";
            private static readonly List<string> ProcessedGuids = new List<string>();

            public UpdateRecord(IExpressionEngine expressionEngine, IState state) : base(expressionEngine)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            public override Task<ActionResult> Execute()
            {
                var paras = Parameters;

                var recordId = paras["recordId"].GetValue<string>();
                Assert.IsTrue(
                    ListRecordsAccounts.Guids.Any(x => x.ToString().Equals(recordId)),
                    "Record ID from flow action parameters does not match expected.");

                Assert.IsTrue(
                    !ProcessedGuids.Any(x => x.ToString().Equals(recordId)),
                    "Record ID from flow action parameters is already processed.");
                
                ProcessedGuids.Add(recordId);
                
                return Task.FromResult(new ActionResult());
            }
        }
    }
}