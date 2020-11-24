using System.Collections.Generic;
using NUnit.Framework;
using Parser;
using Parser.ExpressionParser;

namespace Test
{
    [TestFixture]
    public class StateTest
    {
        [Test]
        public void TestVariableExistent()
        {
            var state = new State();
            var key = "simpleName";
            var value = "String value";

            state.AddVariable(key, new ValueContainer(value));

            var retrievedVariable = state.GetVariable(key);

            Assert.AreEqual(value, retrievedVariable.GetValue<string>());
        }

        [Test]
        public void TestVariableNonExistent()
        {
            var state = new State();
            const string key = "simpleName";

            Assert.Catch<VariableDoesNotExists>(() => { state.GetVariable(key); });
        }

        [Test]
        public void TestOutputExistent()
        {
            var state = new State();
            const string key = "simpleName";
            const int value = int.MaxValue;

            state.AddOutputs(key, new ValueContainer(value));

            var retrievedOutput = state.GetOutputs(key);

            Assert.AreEqual(value, retrievedOutput.GetValue<int>());
        }

        [Test]
        public void TestOutputNonExistent()
        {
            var state = new State();
            const string key = "simpleName";

            var retrievedOutput = state.GetOutputs(key);

            Assert.AreEqual(ValueContainer.ValueType.Null, retrievedOutput.Type());
        }

        [Test]
        public void TestNormalizeValueContainer()
        {
            var valueContainer = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"body/contactid", new ValueContainer("guid")},
                {"body/fullname", new ValueContainer("John Doe")},
                {"body", new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"child/one", new ValueContainer("Child 1")},
                    {"child/two", new ValueContainer("Child 2")}
                })},
                {"body/child/three", new ValueContainer("Child 3")},
            });

            var inner = valueContainer.GetValue<Dictionary<string, ValueContainer>>();
            
            Assert.AreEqual(1, inner.Keys.Count);

            var body = inner["body"].GetValue<Dictionary<string, ValueContainer>>();
            
            Assert.AreEqual(3, body.Keys.Count);
            
            Assert.AreEqual("guid", body["contactid"].GetValue<string>());
            Assert.AreEqual("John Doe", body["fullname"].GetValue<string>());

            var children = body["child"].GetValue<Dictionary<string, ValueContainer>>();
            
            Assert.AreEqual(3, children.Keys.Count);
            
            Assert.AreEqual("Child 1", children["one"].GetValue<string>());
            Assert.AreEqual("Child 2", children["two"].GetValue<string>());
            Assert.AreEqual("Child 3", children["three"].GetValue<string>());
        }
    }
}