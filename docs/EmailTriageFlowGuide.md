# Email Triage Flow Guide

## Overview

This guide demonstrates how to build and test a Power Automate flow that:
- Monitors incoming emails
- Detects specific keywords (inquiry, RFQ, RFP, zapytanie, Anfrage)
- Routes emails to appropriate Teams channels
- Forwards emails to specific distribution lists

## Flow Architecture

### Trigger
- **When a new email arrives (V3)** - Office 365 Outlook connector

### Actions
1. **Parse Email Subject and Body** - Extract text for keyword detection
2. **Determine Category** - Switch action to categorize based on keywords
3. **Post to Teams Channel** - Microsoft Teams connector
4. **Forward Email** - Office 365 Outlook connector

## Keywords Mapping

| Keywords | Category | Teams Channel | Distribution List |
|----------|----------|---------------|-------------------|
| inquiry, RFQ, RFP | Sales Inquiry | Sales Team Channel | sales@company.com |
| zapytanie (Polish) | Sales Inquiry (PL) | International Sales | sales-pl@company.com |
| Anfrage (German) | Sales Inquiry (DE) | International Sales | sales-de@company.com |

## Flow Definition Structure

```json
{
  "properties": {
    "definition": {
      "$schema": "https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#",
      "contentVersion": "1.0.0.0",
      "triggers": {
        "When_a_new_email_arrives": {
          "type": "OpenApiConnectionWebhook",
          "inputs": {
            "host": {
              "apiId": "/providers/Microsoft.PowerApps/apis/shared_office365",
              "operationId": "OnNewEmailV3"
            },
            "parameters": {
              "folderPath": "Inbox",
              "importance": "Any"
            }
          }
        }
      },
      "actions": {
        "Initialize_Email_Content": {
          "type": "Compose",
          "inputs": "@{toLower(concat(triggerOutputs()?['body/subject'], ' ', triggerOutputs()?['body/bodyPreview']))}",
          "runAfter": {}
        },
        "Categorize_Email": {
          "type": "Switch",
          "expression": "@outputs('Initialize_Email_Content')",
          "cases": {
            "Case_RFQ_RFP": {
              "case": "rfq|rfp|inquiry",
              "actions": {
                "Post_to_Sales_Team": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_teams",
                      "operationId": "PostMessageToChannel"
                    },
                    "parameters": {
                      "teamId": "sales-team-id",
                      "channelId": "sales-channel-id",
                      "message": "New Sales Inquiry: @{triggerOutputs()?['body/subject']}"
                    }
                  },
                  "runAfter": {}
                },
                "Forward_to_Sales": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_office365",
                      "operationId": "ForwardEmail"
                    },
                    "parameters": {
                      "messageId": "@triggerOutputs()?['body/id']",
                      "emailAddress": "sales@company.com"
                    }
                  },
                  "runAfter": {
                    "Post_to_Sales_Team": ["Succeeded"]
                  }
                }
              }
            },
            "Case_Polish": {
              "case": "zapytanie",
              "actions": {
                "Post_to_International_Sales_PL": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_teams",
                      "operationId": "PostMessageToChannel"
                    },
                    "parameters": {
                      "teamId": "intl-team-id",
                      "channelId": "intl-channel-id",
                      "message": "Zapytanie (Polish): @{triggerOutputs()?['body/subject']}"
                    }
                  },
                  "runAfter": {}
                },
                "Forward_to_Sales_PL": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_office365",
                      "operationId": "ForwardEmail"
                    },
                    "parameters": {
                      "messageId": "@triggerOutputs()?['body/id']",
                      "emailAddress": "sales-pl@company.com"
                    }
                  },
                  "runAfter": {
                    "Post_to_International_Sales_PL": ["Succeeded"]
                  }
                }
              }
            },
            "Case_German": {
              "case": "anfrage",
              "actions": {
                "Post_to_International_Sales_DE": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_teams",
                      "operationId": "PostMessageToChannel"
                    },
                    "parameters": {
                      "teamId": "intl-team-id",
                      "channelId": "intl-channel-id",
                      "message": "Anfrage (German): @{triggerOutputs()?['body/subject']}"
                    }
                  },
                  "runAfter": {}
                },
                "Forward_to_Sales_DE": {
                  "type": "OpenApiConnection",
                  "inputs": {
                    "host": {
                      "apiId": "/providers/Microsoft.PowerApps/apis/shared_office365",
                      "operationId": "ForwardEmail"
                    },
                    "parameters": {
                      "messageId": "@triggerOutputs()?['body/id']",
                      "emailAddress": "sales-de@company.com"
                    }
                  },
                  "runAfter": {
                    "Post_to_International_Sales_DE": ["Succeeded"]
                  }
                }
              }
            }
          },
          "default": {
            "actions": {}
          },
          "runAfter": {
            "Initialize_Email_Content": ["Succeeded"]
          }
        }
      }
    }
  }
}
```

## Implementation Steps

### Step 1: Create the Flow in Power Automate

1. Go to Power Automate portal (flow.microsoft.com)
2. Create new automated cloud flow
3. Add **When a new email arrives (V3)** trigger
4. Configure folder path (Inbox)
5. Add **Compose** action to concatenate subject and body
6. Add **Switch** action with cases for each keyword category
7. In each case, add:
   - **Post message to channel** (Teams)
   - **Forward an email** (Outlook)
8. Save and export the flow as JSON

### Step 2: Create Action Executors for Testing

Create custom action executors to mock the connector behaviors:

#### Email Trigger Executor
```csharp
public class EmailTriggerExecutor : DefaultBaseActionExecutor
{
    public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_office365";
    public static readonly string[] SupportedOperations = { "OnNewEmailV3" };

    public override Task<ActionResult> Execute()
    {
        return Task.FromResult(new ActionResult
        {
            ActionStatus = ActionStatus.Succeeded,
            ActionOutput = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"body/id", new ValueContainer("msg-12345")},
                {"body/subject", new ValueContainer("RFQ: Need pricing for 1000 units")},
                {"body/bodyPreview", new ValueContainer("We are looking for a quote...")},
                {"body/from", new ValueContainer("customer@example.com")},
                {"body/receivedDateTime", new ValueContainer(DateTime.UtcNow.ToString())}
            })
        });
    }
}
```

#### Teams Post Message Executor
```csharp
public class TeamsPostMessageExecutor : OpenApiConnectionActionExecutorBase
{
    public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_teams";
    public static readonly string[] SupportedOperations = { "PostMessageToChannel" };

    public TeamsPostMessageExecutor(IExpressionEngine expressionEngine)
        : base(expressionEngine) { }

    public override Task<ActionResult> Execute()
    {
        var teamId = Parameters["teamId"].GetValue<string>();
        var channelId = Parameters["channelId"].GetValue<string>();
        var message = Parameters["message"].GetValue<string>();

        // Log or validate the message
        Console.WriteLine($"Teams Message to {teamId}/{channelId}: {message}");

        return Task.FromResult(new ActionResult
        {
            ActionStatus = ActionStatus.Succeeded,
            ActionOutput = new ValueContainer(new Dictionary<string, ValueContainer>
            {
                {"body/id", new ValueContainer(Guid.NewGuid().ToString())},
                {"body/createdDateTime", new ValueContainer(DateTime.UtcNow.ToString())}
            })
        });
    }
}
```

#### Email Forward Executor
```csharp
public class ForwardEmailExecutor : OpenApiConnectionActionExecutorBase
{
    public const string ApiId = "/providers/Microsoft.PowerApps/apis/shared_office365";
    public static readonly string[] SupportedOperations = { "ForwardEmail" };

    public ForwardEmailExecutor(IExpressionEngine expressionEngine)
        : base(expressionEngine) { }

    public override Task<ActionResult> Execute()
    {
        var messageId = Parameters["messageId"].GetValue<string>();
        var emailAddress = Parameters["emailAddress"].GetValue<string>();

        // Log or validate the forward
        Console.WriteLine($"Forwarding email {messageId} to {emailAddress}");

        return Task.FromResult(new ActionResult
        {
            ActionStatus = ActionStatus.Succeeded,
            ActionOutput = new ValueContainer(true)
        });
    }
}
```

### Step 3: Write Tests

```csharp
[TestClass]
public class EmailTriageFlowTests
{
    private IServiceProvider SetupFlowRunner()
    {
        var services = new ServiceCollection();

        // Register action executors
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

    [TestMethod]
    public async Task TestRFQEmailRouting()
    {
        // Arrange
        var sp = SetupFlowRunner();
        var flowRunner = sp.GetRequiredService<IFlowRunner>();
        flowRunner.InitializeFlowRunner("path/to/EmailTriageFlow.json");

        // Act
        var flowReport = await flowRunner.Trigger();

        // Assert
        var state = sp.GetRequiredService<IState>();

        // Verify Teams message was posted
        var teamsOutput = state.GetOutputs("Post_to_Sales_Team");
        Assert.IsNotNull(teamsOutput);

        // Verify email was forwarded
        var forwardOutput = state.GetOutputs("Forward_to_Sales");
        Assert.IsNotNull(forwardOutput);
    }

    [TestMethod]
    public async Task TestPolishEmailRouting()
    {
        // Test with Polish keyword...
    }

    [TestMethod]
    public async Task TestGermanEmailRouting()
    {
        // Test with German keyword...
    }
}
```

## Advanced: Dynamic Keyword Detection

For better keyword matching, use a more sophisticated approach:

```json
{
  "Initialize_Email_Content": {
    "type": "Compose",
    "inputs": "@{toLower(concat(triggerOutputs()?['body/subject'], ' ', triggerOutputs()?['body/bodyPreview']))}"
  },
  "Check_Contains_RFQ": {
    "type": "If",
    "expression": {
      "or": [
        {"contains": ["@outputs('Initialize_Email_Content')", "rfq"]},
        {"contains": ["@outputs('Initialize_Email_Content')", "rfp"]},
        {"contains": ["@outputs('Initialize_Email_Content')", "inquiry"]}
      ]
    },
    "actions": {
      // Post to Teams and forward
    }
  }
}
```

## Testing Strategy

1. **Unit Test Each Keyword**: Create test cases for each keyword category
2. **Test Case Sensitivity**: Ensure keywords are detected regardless of case
3. **Test Multiple Keywords**: Verify behavior when multiple keywords appear
4. **Test No Match**: Ensure default case handles unmatched emails
5. **Validate Parameters**: Assert that correct Teams channels and distribution lists are used

## Best Practices

1. **Use Subject + Body for Detection**: Check both subject and body text
2. **Case-Insensitive Matching**: Convert to lowercase before checking
3. **Fallback Channel**: Have a default route for unmatched emails
4. **Logging**: Log all routing decisions for audit purposes
5. **Error Handling**: Wrap actions in Scope for try-catch behavior

## Next Steps

1. Customize the keyword list and routing rules
2. Add more sophisticated NLP-based categorization
3. Implement priority handling for urgent requests
4. Add notification escalation for SLA breaches
5. Integrate with CRM for automatic case creation

## Related Files

- Flow Definition: `Test/FlowSamples/EmailTriageFlow.json`
- Action Executors: `Test/EmailTriageFlowTests.cs`
- Test Cases: `Test/EmailTriageFlowTests.cs`
