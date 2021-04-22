using System.Collections.Generic;
using NUnit.Framework;
using Parser.ExpressionParser;

namespace Test
{
    [TestFixture]
    public class ValueContainerContainsKeyTest
    {

        [Test]
        public void TestContainsKey()
        {
            var vc = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"layer0/layer1/layer2", new ValueContainer("String value")}
            });
            
            Assert.IsTrue(vc.ContainsKey("layer0/layer1/layer2"));
            Assert.IsTrue(vc.ContainsKey("layer0/layer1"));
            Assert.IsFalse(vc.ContainsKey("layer1/layer2"));
        }
    }
}