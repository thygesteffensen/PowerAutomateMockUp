using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser.ExpressionParser;

namespace Test
{
    [TestFixture]
    public class ValueContainerJTokenConstructorTest
    {
        [TestCaseSource(nameof(_valueContainerConstructorInput))]
        public void ConstructorTest(JToken jToken, ValueContainer.ValueType expectedValueType,
            ValueContainer expectedValue)
        {
            var valueContainer = new ValueContainer(jToken);

            Assert.AreEqual(expectedValueType, valueContainer.Type());
            Assert.AreEqual(expectedValue, valueContainer);
        }

        private static object[] _valueContainerConstructorInput =
        {
            new object[]
            {
                new JValue("Some random string"),
                ValueContainer.ValueType.String,
                new ValueContainer("Some random string")
            },
            new object[]
            {
                new JValue(23),
                ValueContainer.ValueType.Integer,
                new ValueContainer(23)
            },
            new object[]
            {
                new JValue(25.6),
                ValueContainer.ValueType.Float,
                new ValueContainer(25.6)
            },
            new object[]
            {
                new JValue(true),
                ValueContainer.ValueType.Boolean,
                new ValueContainer(true)
            },
            new object[]
            {
                new JValue(new Guid("b4a9b9ee-96c3-49c4-871c-bc74870a134a")),
                ValueContainer.ValueType.String,
                new ValueContainer("b4a9b9ee-96c3-49c4-871c-bc74870a134a")
            },
            new object[]
            {
                new JValue((object) null),
                ValueContainer.ValueType.Null,
                new ValueContainer()
            },
            new object[]
            {
                new JValue(true),
                ValueContainer.ValueType.Boolean,
                new ValueContainer(true)
            },
        };
    }
}