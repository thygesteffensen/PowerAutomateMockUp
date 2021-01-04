using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;


namespace Test
{
    [TestFixture]
    public class ParserTest
    {
        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            // services.AddFlowRunner(new FlowSettings());
            services.AddFlowRunner();

            return services.BuildServiceProvider();
        }

        #region ErrorHandlingTest

        [TestCaseSource(nameof(_simpleCasesExceptions))]
        public void TestSimpleInputExpectFail(TestInput testInput)
        {
            var sp = BuildServiceProvider();
            var engine = sp.GetRequiredService<IExpressionEngine>();
            var state = sp.GetRequiredService<IState>();

            AddValuesToState(testInput.VariableKey, testInput.ValueContainers, testInput.StorageOption, state);

            var exception = Assert.Throws(Is.InstanceOf(testInput.ExceptionType),
                () => { engine.Parse(testInput.Input); });

            Assert.AreEqual(testInput.ExpectedOutput, exception.Message);
        }

        private static TestInput[] _simpleCasesExceptions =
        {
            new TestInput(
                "@nonExistingFunction(\'input\')",
                "Function with name: nonExistingFunction, does not exist.\nAdd the function to the expression engine to test this expression."
            )
            {
                ExceptionType = typeof(FunctionNotKnown)
            },
            new TestInput(
                "@nonExistingFunction()",
                "Function with name: nonExistingFunction, does not exist.\nAdd the function to the expression engine to test this expression."
            )
            {
                ExceptionType = typeof(FunctionNotKnown)
            },
            new TestInput(
                "@toLower(variables(\'name\'))",
                "Variable with variableName: name was not found. If you're not running a full flow, remember to add variables to the expression engine."
            )
            {
                ExceptionType = typeof(VariableDoesNotExists)
            },
            new TestInput(
                "@variables('array')['key']",
                "InvalidTemplate. Unable to process template language expressions in " +
                "action 'variables' inputs at line 'x' and column 'y': " +
                "'The template language expression 'variables('array')['key']' " +
                "cannot be evaluated because property 'key' cannot be selected. " +
                "Array elements can only be selected using an integer index. " +
                "Please see https://aka.ms/logicexpressions for usage details.'."
            )
            {
                ExceptionType = typeof(InvalidTemplateException),
                VariableKey = "array",
                ValueContainers = new[] {new ValueContainer("Value1"), new ValueContainer("Value2")},
                StorageOption = StorageOption.Variables
            }
        };

        #endregion

        #region SimpleCases

        [TestCaseSource(nameof(_simpleCases))]
        public void TestSimpleInput(TestInput testInput)
        {
            var sp = BuildServiceProvider();
            var engine = sp.GetRequiredService<IExpressionEngine>();

            var result = engine.Parse(testInput.Input);

            Assert.AreEqual(testInput.ExpectedOutput, result);
        }

        private static TestInput[] _simpleCases =
        {
            new TestInput("Just a simple string without an exceptions", "Just a simple string without an exceptions"),
            new TestInput("tst@delegate.dk", "tst@delegate.dk"),
            new TestInput(
                "@{toLower(\'user@DOMAIN.dk\')} - Send me an email or call on 8888 8888!",
                "user@domain.dk - Send me an email or call on 8888 8888!"
            ),
            new TestInput("@concat(\'Hi you,\',\' Alice B.\')", "Hi you, Alice B."),
            new TestInput(
                "@concat(\'Hej med dig\', \' Bob A\', \' hvordan \', \'går det?\')",
                "Hej med dig Bob A hvordan går det?"
            ),
            new TestInput("@concat(\'Hi you,\', toLower(\' ALICE B\'))", "Hi you, alice b"),
            new TestInput("@trim(\' What is up \')", "What is up"),
            new TestInput("@toLower(\'It is John''s car.\')", "it is john's car."),
            new TestInput("Hej med dig @{toUpper(\'charlie\')}", "Hej med dig CHARLIE"),
            new TestInput("@{toLower(\'Hej med dig \')}@{trim(\' Mads \')}", "hej med dig Mads"),
            new TestInput("@@1", "@1"),
            new TestInput("@aes", "aes"),
        };

        #endregion

        #region TriggerOutputs

        [TestCaseSource(nameof(_triggerOutputTest))]
        public void TestTriggerOutputs(TestInput testInput)
        {
            var sp = BuildServiceProvider();
            var engine = sp.GetRequiredService<IExpressionEngine>();
            var state = sp.GetRequiredService<IState>();
            state.AddTriggerOutputs(testInput.ValueContainers.First());

            var result = engine.Parse(testInput.Input);

            Assert.AreEqual(testInput.ExpectedOutput, result);
        }

        private static object[] _triggerOutputTest =
        {
            new TestInput("@triggerOutputs()['body/entrytype']", "OCR payment")
            {
                ValueContainers = new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"body/entrytype", new ValueContainer("OCR payment")}
                    })
                }
            },
            new TestInput("@triggerOutputs()?['body/entrytype']", null)
            {
                ValueContainers = new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"", new ValueContainer("")}
                    })
                }
            },
            new TestInput("@triggerOutputs()['body/contactid']", "00112233-4455-6677-8899-aabbccddeeff")
            {
                ValueContainers = new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {
                            "body",
                            new ValueContainer(new Dictionary<string, ValueContainer>
                                {{"contactid", new ValueContainer("00112233-4455-6677-8899-aabbccddeeff")}})
                        }
                    })
                }
            },
            new TestInput("@triggerOutputs()['body']['contactid']", "00112233-4455-6677-8899-aabbccddeeff")
            {
                ValueContainers = new[]
                {
                    new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {
                            "body",
                            new ValueContainer(new Dictionary<string, ValueContainer>
                                {{"contactid", new ValueContainer("00112233-4455-6677-8899-aabbccddeeff")}})
                        }
                    })
                }
            }
        };

        #endregion

        #region TestStorage

        [TestCaseSource(nameof(_testStorage))]
        public void TestInternalsFlowStorage(TestInput input)
        {
            var sp = BuildServiceProvider();
            var engine = sp.GetRequiredService<IExpressionEngine>();
            var state = sp.GetRequiredService<IState>();

            AddValuesToState(input.VariableKey, input.ValueContainers, input.StorageOption, state);

            var result = engine.Parse(input.Input);

            Assert.AreEqual(input.ExpectedOutput, result);
        }


        private static readonly ValueContainer[] BigValue =
        {
            new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {
                    "firstChildValue", new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"secondChildValue", new ValueContainer("The String Value")}
                    })
                }
            }),
            new ValueContainer("Poul")
        };

        private static TestInput[] _testStorage =
        {
            new TestInput(
                "@variables(\'array\')[0]",
                "Thyge")
            {
                VariableKey = "array",
                ValueContainers = new[] {new ValueContainer("Thyge"), new ValueContainer("Poul")},
                StorageOption = StorageOption.Variables
            },
            new TestInput(
                "@toLower(variables(\'name\'))",
                "alice bob")
            {
                VariableKey = "name",
                ValueContainers = new[] {new ValueContainer("Alice Bob")},
                StorageOption = StorageOption.Variables
            },
            new TestInput(
                "@outputs('Compose_name')",
                "Alice Bob")
            {
                VariableKey = "Compose_name",
                ValueContainers = new[] {new ValueContainer("Alice Bob")},
                StorageOption = StorageOption.Outputs
            },
            new TestInput
            (
                "@variables(\'dictionary\')?[0]?[\'firstChildValue\'][\'secondChildValue\']",
                "The String Value")
            {
                VariableKey = "dictionary",
                ValueContainers = BigValue,
                StorageOption = StorageOption.Variables
            }
        };

        [TestCaseSource(nameof(_testStorageValueContainer))]
        public void TestInternalsFlowStorageValueContainer(TestInput input)
        {
            var sp = BuildServiceProvider();
            var engine = sp.GetRequiredService<IExpressionEngine>();
            var state = sp.GetRequiredService<IState>();

            AddValuesToState(input.VariableKey, input.ValueContainers, input.StorageOption, state);

            var result = engine.ParseToValueContainer(input.Input);

            Assert.AreEqual(input.ExpectedOutputType, result.Type());
        }

        private static TestInput[] _testStorageValueContainer =
        {
            new TestInput(
                "@variables(\'array\')",
                ValueContainer.ValueType.Array)
            {
                VariableKey = "array",
                ValueContainers = new[] {new ValueContainer("Thyge"), new ValueContainer("Poul")},
                StorageOption = StorageOption.Variables
            },
            new TestInput(
                "@toLower(variables(\'name\'))",
                ValueContainer.ValueType.String)
            {
                VariableKey = "name",
                ValueContainers = new[] {new ValueContainer("Alice Bob")},
                StorageOption = StorageOption.Variables
            },
            new TestInput(
                "@outputs('Compose_name')",
                ValueContainer.ValueType.String)
            {
                VariableKey = "Compose_name",
                ValueContainers = new[] {new ValueContainer("Alice Bob")},
                StorageOption = StorageOption.Outputs
            },
            new TestInput
            (
                "@variables(\'dictionary\')?[0]?[\'firstChildValue\'][\'secondChildValue\']",
                ValueContainer.ValueType.String)
            {
                VariableKey = "dictionary",
                ValueContainers = BigValue,
                StorageOption = StorageOption.Variables
            },
            new TestInput
            (
                "@greater(variables('Variable'), 10)",
                ValueContainer.ValueType.Boolean)
            {
                VariableKey = "Variable",
                ValueContainers = new[] {new ValueContainer(11)},
                StorageOption = StorageOption.Variables
            }
        };

        public class TestInput
        {
            public TestInput(string input, string expectedOutput)
            {
                Input = input;
                ExpectedOutput = expectedOutput;
            }

            public TestInput(string input, ValueContainer.ValueType expectedOutputType)
            {
                Input = input;
                ExpectedOutputType = expectedOutputType;
            }

            public string VariableKey { get; set; }
            public ValueContainer[] ValueContainers { get; set; }
            public string Input { get; set; }
            public string ExpectedOutput { get; set; }
            public ValueContainer.ValueType ExpectedOutputType { get; set; }
            public StorageOption StorageOption { get; set; }

            public Type ExceptionType { get; set; }
        }

        public enum StorageOption
        {
            Outputs,
            Variables
        }

        #endregion

        private static void AddValuesToState(string key, ValueContainer[] values, StorageOption storageOption,
            IState state)
        {
            if (values == null)
            {
                return;
            }

            switch (storageOption)
            {
                case StorageOption.Outputs:
                    if (values.Length == 1)
                        state.AddOutputs(key, values[0]);
                    else
                        state.AddOutputs(key, values);
                    break;
                case StorageOption.Variables:
                    if (values.Length == 1)
                        state.AddVariable(key, values[0]);
                    else
                        state.AddVariable(key, values);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(storageOption), storageOption, null);
            }
        }
    }
}