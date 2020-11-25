## Value container

Value Container is the internal representation of values in Power Automate Mockup. The representation is used throughout both the FlowRunner and the Expression Parser to ensure consistency.

### Structure
Power Automate supports three upper layers of values; primitive values, arrays and objects.

Primitive values are: boolean, integer, float, string.

Arrays are simply an array of values. However, the values does not have to be the same type, as opposed from 'traditional' programming 😉.

Objects are dictionaries of values, of all types.

### Problem
When using object, values can be retrieved using brackets with indexes, such as:

```
outputs('<key>')['parent/child/value']
```

or

```
outputs('<key>')['parent']['child']['value']
```

or

```
outputs('<key>')['parent/child']['value']
```

Each expression above yield the same result, which raises a problem in the mock, we need to support each case.

If we need to allow each representation in the _engine_, we could easily use a lot for time, just trying to retrieve values, without knowing if the actually exist.

Instead, I have chosen to only support one way of resolving values, which is each key in its own bracket. When parsing a key consisting of '/' I simply parse to be like it was in its own brackets. Thus

```
['parent/child/value']      becomes     ['parent']['child']['value']
['parent/child']['value']   becomes     ['parent']['child']['value']
```  

### Why not just represent it as full path keys?
This is another possible solution.
I have chosen to use the other way because sometimes you want to get a subtree of a object/dictionary, thus the other way is easier. I have chosen to put the overhead when inserting values, instead of when retrieving values.


### On the other hand ...
... maybe it should be when retrieving values. I insert a lot more values than retrieving.

