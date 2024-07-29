# Parser

This is the readme for the parser project which is a part of the FlowRunner.


## BNF

This is the BNF, [Backus-Nuar form](https://da.wikipedia.org/wiki/Backus-Naur_form), of the language used in Power Automate / Logic Apps expressions. 
Their JSON definition can be found [here](https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference) and [here](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-workflow-definition-language#functions) and the [json](https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json).

### Examples
`@triggerOutputs()`

`@toLower('TEXT')`

`@toLower(toUpper('text'))`

`@variables('var')`

`@variables('array')[0]`

`@variables('object')['user']`

`@variables('object')[toLower('USER')]`

`@{toLower('USER')}@{toUpper('user')}`

`Name: @{variables('object')[toLower('USER')]}`


```xml
<input>                 ::= <expression> | <joinedString> | <atPrefixedString>
<atPrefixedString>      ::= <string> <!-- NO ( ) -->

<joinedString>          ::= *(*<allowedString> *<enclosedExpression>)
<allowedString>         ::= *([A-Za-z] | [0-9] | [\_]) <!-- Add the correct characters -->

<expression>	        ::= "@" <method>
<enclosedExpression>	::= "@{" <method> "}"

<method>                ::= <function> *<operators>

<function>      	    ::= <string> "(" [ <arguments> ] ")"
<arguments>             ::= <argument> *("," <argument>)
<argument>	            ::= <method> | <string>

<operators>	            ::= *( [ <nullconditional> ] "[" <index> "]") | <dot>
<nullconditional>       ::= "?"
<index>                 ::= <int> | <string> | <method>
<dot>                   ::= "." <string>
```

`string` contains a valid string including characters according to Azure Workflow definition language.


## Special expressions
Some expressions the user can define itself, since it is idempotent and does not use the engine state.

Some expression should not be user dependent, such as

 * compose
 * outputs
 * triggerOutputs
 * variables

These are all expression which interact with data stored in the state.



Eventuelt skriv om hvordan det var sværet bare at gå igang, men efter du læste en artikel og lavede en BNF var det nemt. Dette var måske også fordi du vidste hvordan Sprache virkede og hvad der skete under moteren og aritklen gav dig en dybere forståelse af hvad man kan! :D


## Extra observations
### Null conditional
We'll handle this, of course, and I've created a flow to figure out how to handle this :D

As seen in the currect version of owner grammer, we'll accept null conditional after a function or after a indexer. If the part before the null conditional is null, we'll stop executing and return??

The flow used to test is called: `null condtional` and is located in DG Lab 6; [link to flow](https://emea.flow.microsoft.com/manage/environments/934e6690-2c92-4a37-ab81-31be17f7724a/solutions/86491904-39dd-ea11-a813-000d3ab11761/flows/d40eaddf-55a2-4b98-87e3-c21c4fcfc57a).

Error (with out null condition):
> InvalidTemplate. Unable to process template language expressions in action 'Respond_to_a_PowerApp_or_flow_-_Without_null_conditional' inputs at line '1' and column '2357': 'The template language expression 'variables('NullVariable')['first']' cannot be evaluated because property 'first' cannot be selected. Please see https://aka.ms/logicexpressions for usage details.'.

With null condtion, the answer is null and read as an empty string in some cases?