# Flow Parser aka. the runner

## Actions
Actions are denfined using schema and we'll handle them in the following way. First a lillte specs


Each action is defined by a name and its content:

```json
{
  ...
    "<action-name>": {
       "type": "<action-type>",
       "inputs": { 
          "<input-name>": { "<input-value>" },
          "retryPolicy": "<retry-behavior>" 
       },
       "runAfter": { "<previous-trigger-or-action-status>" },
       "runtimeConfiguration": { "<runtime-config-options>" },
       "operationOptions": "<operation-option>"
    },
  ...
}
```
_https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-workflow-actions-triggers#actions-overview

First we'll look thorugh registered handles by its name.

If an appropiate handle cannot be found by name, we'll look thorugh registered handles by type.

From the type, we'll determine which action to use. The Action class is build and executed. The result from the actions can be the follwong three

1. Succesfull
2. Unsuccesfull

From all of these, they can have impacted the state variables or added a compose variable or generated an output to use.

### Types
All Action have a type and most of the types af defined [here]()

However, I have found a list of types, which isn't listed. This does not make it easier.

* **Changeset**: Found when using the CDS current environment connectior action `Execute a changeset request`. It also have a new attribute called `kind` which have the value _ODataOpenApiConnection_.


### Alternative
Instead of implementing Action to run the action block expected, outputs can be added.

Simple an Action just generates outputs, which the flow will either use in another action or return to an user. Thus, it makes sense to add outputs from the start or simple set up a rule, to add something to outputs, when an action is called.


### Scopes
Scope does not enclose any variables. So Action run inside a scope, could just as well be outside the scope.

Scopes provide two functionalities.
1. Visual overview of flow in editor (block can be collapsed)
1. Emulate `try catch final` pattern. 

The second option is the want we want to support in PowerAutomateMockUp (PAMU).

To support Scopes, I have added a class ScopeDepthManager (can be renamed), which manages the current collection of action descriptions as well as which scope we are in.

When you exit a scope, you cannot re-enter the scope, therefore I have used a LIFO list (C# Stack). So this is the flow of the ScopeDepthManager

First time a list of action description have been added to the ScopeDepth Manager, they are added to the variable `currentActionDescriptions`.

When a Scope action is encountered, the ScopeActionExecutor is executed.

It extracts the Scope name and Scope actions. Then it calls the SDM, which pushes the current list of action descriptions to a stack, adds the newly extracted list of action descriptions as the current list of action descriptions and pushes the scope name to another stack.

The flow is executed as before.

When the FlowRunner tries to find the next action and fails (no preceding actions matches the conditions to be the next action), the SDM is called and a push is attempted.

* The pop fails -> The flow is done executing and the FlowRunner returns the status of the last run action.
* The pop is successful -> The stack of action descriptions is pop and added as the current list of action descriptions. The scope name from the stack is popped and returned, and this is used to find the preceding action. If no action is found, we'll continue to pop the SDM until an action is found   or until the SDM pop fails.
