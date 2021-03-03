<h1 align="center">Power Automate Mock-Up</h1>
<h3 align="center">Skeleton to run Power Automate flows from JSON flow definition.</h3>
<p align="center">
    <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PowerAutomateMockUp/Release/main?label=main">
        <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PowerAutomateMockUp/Build/dev?label=dev">
    <a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget downloads" src="https://img.shields.io/nuget/dt/PowerAutomateMockUp">
    </a>
    <a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget version" src="https://img.shields.io/nuget/v/PowerAutomateMockUp">
    </a>
    <a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget prerelease version" src="https://img.shields.io/nuget/vpre/PowerAutomateMockUp">
    </a>
</p>
<!-- <p align="center">
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Index">Home</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/GettingStarted">Getting Started</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Technical">Technical</a>
</p> -->

~~Currently there is not a way to unit test Power Automate flows. You have the ability to manually run a flow with static results, but this isn't the same as a unit test. I have during my work implemented business critical functionality in Power Automate using Common Data Service (current environment) connector.~~

There is a way to unit test Power Automate flows!

## Installation

A the [NuGet package](https://www.nuget.org/packages/PowerAutomateMockUp/) to your project.

## How to use

### Introduction
This is a skeleton and itself will not test anything OOB. Instead this is meant as the core, used to implement different connectors. As you probably know, Power Automate uses Connectors to interact with others services, such as Common Data Service. I have implemented [Common Data Service (current environment)](https://github.com/thygesteffensen/PAMU_CDS), go take a look to see how it can be used.

### Getting Started

```c#
var path = "<path to flow definition>";
            
// from Microsoft.Extensions.DependencyInjection
var services = new ServiceCollection();

// Required to set up required dependencies
services.AddFlowRunner(); 

var sp = services.BuildServiceProvider();

var flowRunner = sp.GetRequiredService<FlowRunner>();

flowRunner.InitializeFlowRunner(path);

var flowResult = await flowRunner.Trigger();

// Your flow have now ran
```

### Configuration
This is optional and the settings class has the default values mentioned below.

The settings object is configured this way:
```cs
services.Configure<FlowSettings>(x => { }); // Optional way to add settings
```
The possible values to set is:

 * `x.FailOnUnknownAction` (default: `true`): If an action cannot be found and exception is thrown. This can be avoid and the action is ignored and the status is assumed to be `Succeeded`.
 * `x.IgnoreActions` (default: `empty`): List of action names which are ignored during execution, the action is not executed and the status is assumed to be `Succeeded`.
* `x.LogActionsStates` (default: `true`): Logs JSON, parsed input and generated output for every action executed.

### Asserting action input and output

The FlowReport from triggering the flow can be used to assert the input and output of an action.

This can be used to verify that the expected parameters to an action is present and you can assert the input is as expected.

```c#
var greetingCardItems = flowReport.ActionStates["Create_a_new_row_-_Create_greeting_note"]
                .ActionInput?["parameters"]?["item"];
Assert.IsNotNull(greetingCardItems);
Assert.AreEqual(expectedNoteSubject, greetingCardItems["subject"]);
Assert.AreEqual(expectedNoteText, greetingCardItems["notetext"]);
```

### Adding actions
Actions can be added in three ways

1. Using specific action name
2. Using Connection ApiId and supported OperationIds
3. Using Action type (**Not recommended**)

#### 1. Using specific action name
```c#
services.AddFlowActionByName<GetMsnWeather>("Get_forecast_for_today_(Metric)");
```

When the action named *Get_forecast_for_today_(Metric)* is reached and about to be executed, the class with type GetMsnWeather is retrieved from the ServiceProvider and used to execute the action.

#### 2. Using Connection ApiId and supported OperationIds
```c#
// For OpenApiConnection connectors only
services.AddFlowActionByApiIdAndOperationsName<Notification>(
    "/providers/Microsoft.PowerApps/apis/shared_flowpush", 
    new []{ "SendEmailNotification", "SendNotification" });
```

When an action from the **Notification** connector with one of the supported types is reached in the flow, a action executor instance of type `Notification` is created and used to execute the action.

#### 3. Using Action type (**Not recommended**)
```c#
services.AddFlowActionByFlowType<IfActionExecutor>("If");
```
When the generic action type **If** i reached, an action executor instance of type `IfActionExecutor` is created and used to execute the action.

This is not recommended due to the fact that every OpenApiConnection connector will have the type **OpenApiConnection**. This means that both Common Data Service (current environment) and many others, will use the same action executors, which is not the correct way to do it.

This way of resolving an action executor is only used to resolve actions, where only one Action uses that type. This is **If**, **DoUntil** etc.

### Creating action executors.
Currently there are two classes to extend, one is **DefaultBaseActionExecutor** and the other is **OpenApiConnectionBaseActionExecutor**.

#### DefaultBaseActionExecutor
```c#
private class ActionExecutor : DefaultBaseActionExecutor
{
    private readonly IExpressionEngine _expression;

    // Using dependency injection to get dependencies
    public TriggerActionExecutor(IExpressionEngine expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override Task<ActionResult> Execute()
    {
        var result = new ActionResult();

        try
        {
            // Some dangerous operation
            // ...

            result.ActionOutput = new ValueContainer(new Dictionary<string, ValueContainer>()
            {
                {"Key", new ValueContainer("Value")}
            });
            // Corresponds to: outputs('<action>').Key || outputs('<action>')['Key']

            result.ActionStatus = ActionStatus.Succeeded;
        } 
        catch(EvenMoreDangerousException exp)
        {
            // PAMU handles the exceptions...
            result.ActionStatus = ActionStatus.Failed;
            result.ActionExecutorException = exp;
        }

        return Task.FromResult(result);
    }
}
```

The execute method is called when the action is run.

#### OpenApiConnectionBaseActionExecutor
```c#
private class ActionExecutor : OpenApiConnectionActionExecutorBase
{
    // To easier register the Action Executor
    public const string FlowActionName = "Update_Account_-_Invalid_Id";

    public override Task<ActionResult> Execute()
    {
        // ... Execute action functionality
 
        var parameters = Parameters;

        var entityName = parameters["string"].GetValue<string>();

        // ...
    }
}
```

When using OpenApiConnectionActionExecutorBase, some extra values form the Json definition is parsed and ready to use. These are currently values in the parameter object and host object.

### Dependencies
Power Automate MockUp heavily depends on dependencies and its a great way to easily get different classes, in your own class.
Below is a list of dependencies you might want to use, when executing flows.

#### ExpressionParser
```c#
ValueContainer value = expressionParser.Parse("<expression>");
```
An expression is known from Power Automate such as `"@toLower('Jonh Doe')"` or `"@outputs('Get_contacts')[2]"`. 
Every expression from Power Automate is supported in PowerAutomateMockUp. 

The response is returned wrap in the ValueContainer.

#### IState
IState, and its implementation, is how the state of the execution of a Power Automate flow is handled. It contains the trigger values and can give the value of previous executed actions, by either using one of the interfaces below (for simplicity) or IState itself.

## Tests

Tests are located in the **Tests** project and they are written using Nunit as test framework.


## Contribute

This is my bachelor project and I'm currently not accepting contributions until it have been handed in. Anyway, fell free to drop an issue with a suggestion or improvement.

<!--## Credits-->

## Code style
The code is written using [Riders](https://www.jetbrains.com/help/rider/Settings_Code_Style_CSHARP.html) default C# code style.

Commits are written in [conventional commit](https://www.conventionalcommits.org/en/v1.0.0/) style, the commit messages are used to determine the version and when to release a new version. The pipeline is hosted on Github and [Semantic Release](https://github.com/semantic-release/semantic-release) is used.



## License

MIT
