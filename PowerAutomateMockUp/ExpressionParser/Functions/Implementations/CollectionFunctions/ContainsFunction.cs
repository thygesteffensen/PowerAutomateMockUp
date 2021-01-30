using System;
using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.CollectionFunctions
{
    public class ContainsFunction : Function
    {
        public ContainsFunction() : base("contains")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            var collection = parameters[0];
            var value = parameters[1];

            switch (collection.Type())
            {
                case ValueContainer.ValueType.Array:
                    var array = collection.GetValue<IEnumerable<ValueContainer>>();

                    switch (value.Type())
                    {
                        case ValueContainer.ValueType.Integer:
                        case ValueContainer.ValueType.Float:
                        case ValueContainer.ValueType.String:
                        case ValueContainer.ValueType.Boolean:
                        case ValueContainer.ValueType.Null:
                            return new ValueContainer(array.Contains(value));
                        case ValueContainer.ValueType.Array:
                        case ValueContainer.ValueType.Object:
                            return new ValueContainer(false);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ValueContainer.ValueType.Object:
                    var key = value.GetValue<string>();
                    return new ValueContainer(collection.AsDict().ContainsKey(key));
                case ValueContainer.ValueType.String:
                    var text = collection.GetValue<string>();
                    var substring = value.GetValue<string>();
                    return new ValueContainer(text.Contains(substring));
                default:
                    throw new PowerAutomateMockUpException($"Cannot perform contains on {collection.Type()}.");
            }
        }
    }
}