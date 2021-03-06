<h1 align="center">PowerAutomateMockUp</h1>
<h3 align="center">Skeleton to run Power Automate Flows from Json Flow definition.</h3>
<p align="center">
    <!--<img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PowerAutomateMockUp/Build/main">-->
        <img alt="Build status" src="https://img.shields.io/github/workflow/status/thygesteffensen/PowerAutomateMockUp/Build/dev">
    <a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget downloads" src="https://img.shields.io/nuget/dt/PowerAutomateMockUp">
    </a>
    <a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget version" src="https://img.shields.io/nuget/v/PowerAutomateMockUp">
    </a>
    <!--<a href="https://www.nuget.org/packages/PowerAutomateMockUp/">
        <img alt="Nuget prerelease version" src="https://img.shields.io/nuget/vpre/PowerAutomateMockUp">
    </a>-->
</p>
<p align="center">
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Index">Home</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/GettingStarted">Getting Started</a>
    |
    <a href="https://thygesteffensen.github.io/PowerAutomateMockUp/Technical">Technical</a>
</p>

Currently there is not a way to unit test Power Automate flows. You have the ability to manually run a flow with static results, but this isn't the same as a unit test. I have during my work implemented business critical functionality in Power Automate using Common Data Service (current environment) connector, whiteout being able to test it properly, when developing Flows and Plugins to Dynamics 365. 

## Code style
The code is written using [Riders](https://www.jetbrains.com/help/rider/Settings_Code_Style_CSHARP.html) default C# code style.

Commits are written in [conventional commit](https://www.conventionalcommits.org/en/v1.0.0/) style, the commit messages are used to determine the version and when to release a new version. The pipeline is hosted on Github and [Semantic Release](https://github.com/semantic-release/semantic-release) is used.

## Installation

Currently the project is still in alpha. To find the packages at nuget.com, you have to check 'Prerelease', before the nuget appears.

## Tests

Tests are located in the **Tests** project and they are written using Nunit as test framework.

## How to use

### Introduction
This is a skeleton and itself will not test anything OOB. Instead this is meant as the core, used to implement different connectors. As you probably know, Power Automate uses Connectors to interact with others service, such as Common Data Service. I have implemented [Common Data Service (current environment)](https://github.com/thygesteffensen/PAMU_CDS), go take a look to see how it can be used.

### Getting Started

```c#
var path = "<path to flow definition>";
            
// from Microsoft.Extensions.DependencyInjection
var services = new ServiceCollection();

services.Configure<FlowSettings>(x => { });

// Required to set up required dependencies
services.AddFlowRunner(); 

var sp = services.BuildServiceProvider();

var flowRunner = sp.GetRequiredService<FlowRunner>();

flowRunner.InitializeFlowRunner(path);

await flowRunner.Trigger();

// Your flow have now ran
```

### Adding actions
Actions can added in three ways

1. Using specific action name
2. Using Connection ApiId and supported OperationIds
3. Using Action type (**Not recommended**)

#### 1. Using specific action name
```c#
services.AddFlowActionByName<GetMsnWeather>("Get_forecast_for_today_(Metric)");
```

When the action named *Get_forecast_for_today_(Metric)* is reached and about to be executed, the class with type GetMsnWeather is retrieved from the ServiceProvider and called.

#### 2. Using Connection ApiId and supported OperationIds
```c#
// For OpenApiConnection connectors only
services.AddFlowActionByApiIdAndOperationsName<Notification>(
    "/providers/Microsoft.PowerApps/apis/shared_flowpush", 
    new []{ "SendEmailNotification", "SendNotification" });
```

When an action from the **Notification** connector with one of the supported types is reached in the flow, a action executor instance of type Notification is created and used.

#### 3. Using Action type (**Not recommended**)
```c#
services.AddFlowActionByFlowType<IfActionExecutor>("If");
```
When the generic action type **If** i reached, a action executor instance of type Notification is created and used.

This is not recommended due to the fact that every OpenApiConnection connector will have the type OpenApiConnection. This means that both Common Data Service (current environment) and many others, will use the same action executors, which is not the correct way to do it.

### Creating action executors.
Currently there are two classes to extend, the one is **DefaultBaseActionExecutor** and the other are **OpenApiConnectionBaseActionExecutor**.

#### DefaultBaseActionExecutor
```c#
private class ActionExecutor : DefaultBaseActionExecutor
{
    private readonly IState _state;

    // Using dependency injection to get dependencies
    public TriggerActionExecutor(IState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public override Task<ActionResult> Execute()
    {
        // ... Execute action functionality

        return Task.FromResult(
            new ActionResult {ActionStatus = ActionStatus.Succeeded});
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

        return Task.FromResult(
            new ActionResult {ActionStatus = ActionStatus.Failed});
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

## Contribute

This is my bachelor project and I'm currently not accepting contributions until it have been handed in. Anyway, fell free to drop an issue with a suggestion or improvement.

<!--## Credits-->


## License

© Thyge Skødt Steffensen