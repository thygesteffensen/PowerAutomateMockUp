// using FlowTesterVersion1;
// using FlowTesterVersion1.actions;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Newtonsoft.Json.Linq;
// using PowerAutomateMockUp.FlowParser.Actions;
//
// namespace Test.ActionTests
// {
//     [TestClass]
//     public class IfActionTest
//     {
//         private Core core;
//         [TestInitialize]
//         public void TestInitialize()
//         {
//             core = new Core();
//             core.AddTriggerOutputs("body/telephone1", "28984323");
//             core.AddTriggerOutputs("body/fullname", "Thyge Steffensen");
//             core.AddTriggerOutputs("body/emailadress1", "thyge@delegate.dk");
//             core.AddTriggerOutputs("body/firstname", "Thyge");
//             core.AddTriggerOutputs("body/lastname", "Steffensen");
//         }
//
//         [TestMethod]
//         public void CombinedTest()
//         {
//             const string trueAction = "TerminateTrue";
//             var json = $@"
//                         {{
//                             ""If_Status_is_Matched_or_Failure"": {{
//                                 ""actions"": {{
//                                             ""{trueAction}"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""or"": [
//                                         {{
//                                             ""equals"": [
//                                                 ""@triggerOutputs()?['body/telephone1']"",
//                                                 ""28984323""
//                                             ]
//                                         }},
//                                         {{
//                                             ""not"": {{
//                                                 ""equals"": [
//                                                     ""@triggerOutputs()?['body/fullname']"",
//                                                     ""Thyge Steffensen""
//                                                 ]
//                                             }}
//                                         }},
//                                         {{
//                                             ""and"": [
//                                                 {{
//                                                     ""startsWith"": [
//                                                         ""@triggerOutputs()?['body/emailaddress1']"",
//                                                         ""@triggerOutputs()?['body/firstname']""
//                                                     ]
//                                                 }},
//                                                 {{
//                                                     ""contains"": [
//                                                         ""@triggerOutputs()?['body/emailaddress1']"",
//                                                         ""@triggerOutputs()?['body/lastname']""
//                                                     ]
//                                                 }}
//                                             ]
//                                         }},
//                                         {{
//                                             ""and"": [
//                                                 {{
//                                                     ""contains"": [
//                                                         ""@triggerOutputs()?['body/fullname']"",
//                                                         ""@triggerOutputs()?['body/firstname']""
//                                                     ]
//                                                 }},
//                                                 {{
//                                                     ""greater"": [
//                                                         ""@length(triggerOutputs()?['body/fullname'])"",
//                                                         ""@length(triggerOutputs()?['body/firstname'])""
//                                                     ]
//                                                 }}
//                                             ]
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject) JObject.Parse(json).GetValue("If_Status_is_Matched_or_Failure"), core);
//
//             var actionResult = ifTrigger.Execute();
//
//             Assert.AreEqual(trueAction, actionResult.RunAfter);
//         }
//         
//         [TestMethod]
//         public void ContainsIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""contains"": [
//                                                 ""John Doe"",
//                                                 ""n D""
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""contains"": [
//                                                     ""John Doe"",
//                                                     ""n_D""
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void EqualsIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""equals"": [
//                                                 1,
//                                                 1
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""equals"": [
//                                                     ""john doe"",
//                                                     ""@toLower('John Doe')""
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void GreaterIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""greater"": [
//                                                 10,
//                                                 8
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""greater"": [
//                                                     10,
//                                                     12
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void GreaterOrEqualsIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""greaterOrEqual"": [
//                                                 12,
//                                                 12
//                                             ]
//                                         }},
//                                         {{
//                                             ""greaterOrEqual"": [
//                                                 14,
//                                                 12
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""greaterOrEqual"": [
//                                                     10,
//                                                     12
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void LessIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""less"": [
//                                                 10,
//                                                 12
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""less"": [
//                                                     10,
//                                                     8
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void LessOeEqualsIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""lessOrEquals"": [
//                                                 10,
//                                                 10
//                                             ]
//                                         }},
//                                         {{
//                                             ""lessOrEquals"": [
//                                                 10,
//                                                 12
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""lessOrEquals"": [
//                                                     10,
//                                                     8
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void StartsWithIfActionTest()
//         {
//             var json =$@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""startsWith"": [
//                                                 ""John Doe"",
//                                                 ""John""
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""startsWith"": [
//                                                     ""John Doe"",
//                                                     ""Doe""
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//
//         [TestMethod]
//         public void EndsWithIfActionTest()
//         {
//             var json = $@"
//                         {{
//                             ""Test_If_Block"": {{
//                                 ""actions"": {{
//                                             ""TerminateTrue"": {{    
//                                                 ""runAfter"": {{}},
//                                                 ""type"": ""Terminate"",
//                                                 ""inputs"": {{
//                                                     ""runStatus"": ""Succeeded""
//                                                     }}
//                                                 }}
//                                             }},
//                                 ""runAfter"": {{
//                                     ""Status_Reason_-_Failed"": [
//                                         ""Succeeded""
//                                     ]
//                                 }},
//                                 ""else"": {{
//                                     ""actions"": {{
//                                         ""TerminateFalse"": {{
//                                             ""runAfter"": {{}},
//                                             ""type"": ""Terminate"",
//                                             ""inputs"": {{
//                                                 ""runStatus"": ""Succeeded""
//                                             }}
//                                         }}
//                                     }}
//                                 }},
//                                 ""expression"": {{
//                                     ""and"": [
//                                         {{
//                                             ""endsWith"": [
//                                                 ""Jane Doe"",
//                                                 ""Doe""
//                                             ]
//                                         }},
//                                         {{ 
//                                             ""not"": 
//                                             {{
//                                                 ""endsWith"": [
//                                                     ""John Doe"",
//                                                     ""John""
//                                                 ]
//                                             }},
//                                         }}
//                                     ]
//                                 }},
//                                 ""type"": ""If""
//                             }}
//                         }}";
//
//             var ifTrigger = new IfAction(ActionType.If,
//                 (JObject)JObject.Parse(json).GetValue("Test_If_Block"), core);
//
//             var result = ifTrigger.Execute();
//
//             Assert.AreEqual("TerminateTrue", result.RunAfter);
//         }
//     }
// }