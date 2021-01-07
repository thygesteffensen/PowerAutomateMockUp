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

            var intersect =
                ToDictionary(first, parameters[1]);

            if (parameters.Count > 2)
            {
                intersect = parameters.Skip(2).Aggregate(intersect, ToDictionary);
            }

            return new ValueContainer(intersect);
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
            var second = parameters[1].GetValue<IEnumerable<ValueContainer>>();

            var intersection = first.Intersect(second, new ValueContainerComparer());

            if (parameters.Count > 2)
            {
                intersection = parameters.Skip(2)
                    .Select(valueContainer => valueContainer.GetValue<IEnumerable<ValueContainer>>())
                    .Aggregate(intersection, (current, collection) => current.Intersect(collection));
            }

            return new ValueContainer(intersection);
        }
    }
}