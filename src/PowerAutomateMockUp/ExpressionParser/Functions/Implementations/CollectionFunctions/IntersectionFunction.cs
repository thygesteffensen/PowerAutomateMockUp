using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;

namespace Parser.ExpressionParser.Functions.Implementations.CollectionFunctions
{
    public class InterSectionFunction : Function
    {
        public InterSectionFunction() : base("intersection")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            return parameters[0].Type() switch
            {
                ValueContainer.ValueType.Array => IntersectList(parameters),
                ValueContainer.ValueType.Object => IntersectDict(parameters),
                _ => throw new PowerAutomateMockUpException(
                    $"Can only intersect Array and Object, not {parameters[0].Type()}.")
            };
        }

        private ValueContainer IntersectDict(IReadOnlyList<ValueContainer> parameters)
        {
            var first = parameters[0].GetValue<Dictionary<string, ValueContainer>>();

            return new ValueContainer(parameters.Skip(1).Aggregate(first, ToDictionary));
        }

        private Dictionary<string, ValueContainer> ToDictionary(Dictionary<string, ValueContainer> first,
            ValueContainer valueContainer)
        {
            var second = valueContainer.GetValue<Dictionary<string, ValueContainer>>();

            return first.Where(x => second.ContainsKey(x.Key))
                .ToDictionary(x => x.Key, x => second[x.Key]);
        }

        private ValueContainer IntersectList(IReadOnlyList<ValueContainer> parameters)
        {
            var first = parameters[0].GetValue<IEnumerable<ValueContainer>>();

            var intersection = parameters.Skip(1)
                .Select(valueContainer => valueContainer.GetValue<IEnumerable<ValueContainer>>())
                .Aggregate(first, (current, collection) => current.Intersect(collection));

            return new ValueContainer(intersection);
        }
    }
}