using System.Collections.Generic;
using System.Linq;

namespace Parser.ExpressionParser
{
    static class ValueContainerExtensions
    {
        public static Dictionary<string, ValueContainer> Normalize(this Dictionary<string, ValueContainer> input)
        {
            var temp = new Dictionary<string, ValueContainer>();

            foreach (var keyValuePair in input)
            {
                var keys = keyValuePair.Key.Split('/');

                BuildNest(keys, keyValuePair.Value, new ValueContainer(temp, false));
            }

            return temp;
        }

        private static ValueContainer BuildNest(string[] keys, ValueContainer value, ValueContainer current)
        {
            if (keys.Length == 1)
            {
                var dict = current.GetValue<Dictionary<string, ValueContainer>>();
                if (dict.ContainsKey(keys[0]) && value.Type() == ValueContainer.ValueType.Object)
                {
                    var innerDict = dict[keys[0]].GetValue<Dictionary<string, ValueContainer>>();
                    var valueDict = value.GetValue<Dictionary<string, ValueContainer>>();
                    foreach (var keyValuePair in valueDict)
                    {
                        innerDict[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
                else
                {
                    dict[keys[0]] = value;
                }

                return new ValueContainer(dict, false);
            }
            else
            {
                var dict = current.GetValue<Dictionary<string, ValueContainer>>();

                if (dict.ContainsKey(keys.First()))
                {
                    var innerDict = dict[keys.First()].GetValue<Dictionary<string, ValueContainer>>();

                    BuildNest(keys.Skip(1).ToArray(), value, new ValueContainer(innerDict, false));
                }
                else
                {
                    var t = BuildNest(keys.Skip(1).ToArray(), value,
                        new ValueContainer(new Dictionary<string, ValueContainer>(), false));
                    dict[keys.First()] = t;
                }

                return new ValueContainer(dict, false);
            }
        }
    }
}