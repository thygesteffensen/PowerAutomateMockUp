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
    }
}