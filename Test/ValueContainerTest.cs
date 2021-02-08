using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Parser.ExpressionParser;

namespace Test
{
    [TestFixture]
    public class ValueContainerTest
    {
        [Test]
        public void TestNormalizeValueContainer()
        {
            var valueContainer = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"body/contactid", new ValueContainer("guid")},
                {"body/fullname", new ValueContainer("John Doe")},
                {
                    "body", new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"child/one", new ValueContainer("Child 1")},
                        {"child/two", new ValueContainer("Child 2")}
                    })
                },
                {"body/child/three", new ValueContainer("Child 3")},
            });

            var inner = valueContainer;

            Assert.AreEqual(1, inner.GetValue<Dictionary<string, ValueContainer>>().Keys.Count);

            var body = inner["body"].GetValue<Dictionary<string, ValueContainer>>();

            Assert.AreEqual(3, body.Keys.Count);

            Assert.AreEqual("guid", body["contactid"].GetValue<string>());
            Assert.AreEqual("John Doe", body["fullname"].GetValue<string>());

            var children = body["child"];

            Assert.AreEqual(3, children.GetValue<Dictionary<string, ValueContainer>>().Keys.Count);

            Assert.AreEqual("Child 1", children["one"].GetValue<string>());
            Assert.AreEqual("Child 2", children["two"].GetValue<string>());
            Assert.AreEqual("Child 3", children["three"].GetValue<string>());
        }

        [Test]
        public void TestArrayIndexer()
        {
            var valueContainer = new ValueContainer(new List<ValueContainer>
            {
                new ValueContainer("Item 0"),
                new ValueContainer("Item 1"),
                new ValueContainer("Item 2"),
                new ValueContainer("Item 3"),
                new ValueContainer("Item 4"),
                new ValueContainer("Item 5"),
                new ValueContainer("Item 6"),
                new ValueContainer("Item 7"),
                new ValueContainer("Item 8"),
                new ValueContainer("Item 9")
            });

            Assert.AreEqual("Item 5", valueContainer[5].GetValue<string>());

            var newValue8 = "New item 8";
            valueContainer[8] = new ValueContainer(newValue8);

            Assert.AreEqual(newValue8, valueContainer[8].GetValue<string>());
        }

        [Test]
        public void TestArrayIndexerFailure()
        {
            var valueContainer = new ValueContainer(150f);

            var exception1 = Assert.Throws<InvalidOperationException>(() =>
            {
                var temp = valueContainer[0];
            });
            Assert.AreEqual(exception1.Message, "Index operations can only be performed on arrays.");

            var exception2 = Assert.Throws<InvalidOperationException>(() =>
            {
                valueContainer[0] = new ValueContainer(100f);
            });
            Assert.AreEqual(exception2.Message, "Index operations can only be performed on arrays.");
        }

        [Test]
        public void TestObjectIndexer()
        {
            var valueContainer = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {
                    "body", new ValueContainer(new Dictionary<string, ValueContainer>
                    {
                        {"name", new ValueContainer("James P. \"Sulley\" Sullivan")}
                    })
                }
            });

            Assert.AreEqual(ValueContainer.ValueType.Object, valueContainer["body"].Type());

            Assert.AreEqual("James P. \"Sulley\" Sullivan", valueContainer["body"]["name"].GetValue<string>());

            var guidStr = Guid.NewGuid().ToString();
            valueContainer["body"]["id"] = new ValueContainer(guidStr);

            Assert.AreEqual(guidStr, valueContainer["body"]["id"].GetValue<string>());
        }

        [Test]
        public void TestObjectIndexerFailure()
        {
            var valueContainer = new ValueContainer();

            var exception1 = Assert.Throws<InvalidOperationException>(() =>
            {
                var temp = valueContainer["name"];
            });
            Assert.AreEqual("Index operations can only be performed on objects.", exception1.Message);


            var exception2 = Assert.Throws<InvalidOperationException>(() =>
            {
                valueContainer["name"] = new ValueContainer("Mendel Stromm");
            });

            Assert.AreEqual("Index operations can only be performed on objects.", exception2.Message);
        }

        [Test]
        public void TestObjectIndexerKeyPath()
        {
            var valueContainer = new ValueContainer(new Dictionary<string, ValueContainer>());

            valueContainer["body/name"] = new ValueContainer("John Doe");

            Assert.AreEqual("John Doe", valueContainer["body/name"].GetValue<string>());
        }
        
        [Test]
        public void TestJValueToValueContainer()
        {
            var expectedValue = "Value";
            var jValue = new JValue(expectedValue);

            var valueContainer = new ValueContainer(jValue);

            Assert.IsNotNull(valueContainer);
            Assert.AreEqual(ValueContainer.ValueType.String, valueContainer.Type());
            Assert.AreEqual(expectedValue, valueContainer.GetValue<string>());
        }

        public T GetValue<T>(ValueContainer valueContainer)
        {
            return valueContainer.GetValue<T>();
        }

        [Test]
        public void TestJsonToValueContainerSimple()
        {
            var json = JToken.Parse("{\"Terminate\":{\"Key1\":\"Value1\",\"Key2\":\"Value2\"}}");
            
            var valueContainer = new ValueContainer(json);
            
            Assert.IsTrue(valueContainer.GetValue<Dictionary<string, ValueContainer>>().ContainsKey("Terminate"));
            Assert.AreEqual(1,valueContainer.GetValue<Dictionary<string, ValueContainer>>().Count);
            
            Assert.AreEqual(2, valueContainer["Terminate"].GetValue<Dictionary<string,ValueContainer>>().Count);
        }
        
        [Test]
        public void TestJsonToValueContainerTypicalInputSection()
        {
            var json = JToken.Parse(
                "{\"inputs\": {\"host\": {\"connectionName\": \"shared_commondataserviceforapps\",\"operationId\": \"GetItem\"," +
                "\"apiId\": \"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                "\"parameters\": {\"entityName\": \"accounts\",\"recordId\": \"@triggerOutputs()?['body/accountid']\"}," +
                "\"authentication\": \"@parameters('$authentication')\"}}");
            
            var valueContainer = new ValueContainer(json);
            
            Assert.IsTrue(valueContainer.GetValue<Dictionary<string, ValueContainer>>().ContainsKey("inputs"));
            Assert.AreEqual(1,valueContainer.GetValue<Dictionary<string, ValueContainer>>().Count);

            var inputs = valueContainer["inputs"];
            var inputsDict= valueContainer["inputs"].GetValue<Dictionary<string, ValueContainer>>();
            
            Assert.IsTrue(inputsDict.ContainsKey("host"));
            Assert.IsTrue(inputsDict.ContainsKey("parameters"));
            Assert.IsTrue(inputsDict.ContainsKey("authentication"));

            Assert.AreEqual(3,inputs.GetValue<Dictionary<string,ValueContainer>>().Count);

            var host = inputs["host"];
            Assert.AreEqual(3,host.GetValue<Dictionary<string,ValueContainer>>().Count);
            
            var parameters = inputs["parameters"];
            Assert.AreEqual(2,parameters.GetValue<Dictionary<string,ValueContainer>>().Count);
        }
    }
}