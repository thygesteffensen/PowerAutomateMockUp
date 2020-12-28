using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Parser.ExpressionParser
{
    [JsonConverter(typeof(ValueContainerConverter))]
    public class ValueContainer
    {
        private readonly dynamic _value;
        private readonly ValueType _type;

        public ValueContainer(string value, bool tryToParse = false)
        {
            if (value == null)
            {
                _value = null;
                _type = ValueType.Null;
                return;
            }

            if (tryToParse)
            {
                if (int.TryParse(value, out var iValue))
                {
                    _value = iValue;
                    _type = ValueType.Integer;
                }
                else if (float.TryParse(value, out var fValue))
                {
                    _value = fValue;
                    _type = ValueType.Float;
                }
                else if (bool.TryParse(value, out var bValue))
                {
                    _value = bValue;
                    _type = ValueType.Boolean;
                }
                else
                {
                    _value = value;
                    _type = ValueType.String;
                }
            }
            else
            {
                _value = value;
                _type = ValueType.String;
            }
        }

        public ValueContainer(string stringValue)
        {
            _value = stringValue;
            _type = ValueType.String;
        }

        public ValueContainer(float floatValue)
        {
            _value = floatValue;
            _type = ValueType.Float;
        }

        public ValueContainer(double floatValue)
        {
            _value = floatValue;
            _type = ValueType.Float;
        }

        public ValueContainer(int intValue)
        {
            _value = intValue;
            _type = ValueType.Integer;
        }

        public ValueContainer(bool boolValue)
        {
            _value = boolValue;
            _type = ValueType.Boolean;
        }

        public ValueContainer(IEnumerable<ValueContainer> arrayValue)
        {
            _value = arrayValue;
            _type = ValueType.Array;
        }

        internal ValueContainer(Dictionary<string, ValueContainer> objectValue, bool normalize)
        {
            _value = normalize ? objectValue.Normalize() : objectValue;

            _type = ValueType.Object;
        }

        public ValueContainer(Dictionary<string, ValueContainer> objectValue)
        {
            _value = objectValue.Normalize();
            _type = ValueType.Object;
        }

        public ValueContainer(ValueContainer valueContainer)
        {
            _type = valueContainer._type;
            _value = valueContainer._value;
        }

        public ValueContainer()
        {
            _type = ValueType.Null;
            _value = null;
        }

        public ValueContainer(JToken json)
        {
            _type = ValueType.Object;
            _value = JsonToValueContainer(json).GetValue<Dictionary<string, ValueContainer>>();
        }

        public ValueType Type()
        {
            return _type;
        }

        public enum ValueType
        {
            Boolean,
            Integer,
            Float,
            String,
            Object,
            Array,
            Null
        }

        public T GetValue<T>()
        {
            return _value;
        }

        public ValueContainer this[int i]
        {
            get
            {
                if (_type != ValueType.Array)
                {
                    throw new InvalidOperationException("Index operations can only be performed on arrays.");
                }

                return ((List<ValueContainer>) _value)[i];
            }
            set
            {
                if (_type != ValueType.Array)
                {
                    throw new InvalidOperationException("Index operations can only be performed on arrays.");
                }

                ((List<ValueContainer>) _value)[i] = value;
            }
        }

        public ValueContainer this[string key]
        {
            get
            {
                if (_type != ValueType.Object)
                {
                    throw new InvalidOperationException("Index operations can only be performed on objects.");
                }

                var keyPath = key.Split('/');

                var current = GetValue<Dictionary<string, ValueContainer>>()[keyPath.First()];
                foreach (var xKey in keyPath.Skip(1))
                {
                    current = current.GetValue<Dictionary<string, ValueContainer>>()[xKey]; // Does not
                }

                return current;
            }
            set
            {
                if (_type != ValueType.Object)
                {
                    throw new InvalidOperationException("Index operations can only be performed on objects.");
                }

                var keyPath = key.Split('/');
                var finalKey = keyPath.Last();

                var current = _value;
                foreach (var xKey in keyPath.Take(keyPath.Length - 1))
                {
                    var dict = GetValue<Dictionary<string, ValueContainer>>();
                    var success = dict.TryGetValue(xKey, out var temp);

                    if (success)
                    {
                        current = temp;
                    }
                    else
                    {
                        dict[xKey] = new ValueContainer(new Dictionary<string, ValueContainer>());
                        current = dict[xKey].GetValue<Dictionary<string, ValueContainer>>();
                    }
                }

                current[finalKey] = value;
            }
        }

        public Dictionary<string, ValueContainer> AsDict()
        {
            if (_type == ValueType.Object)
            {
                return GetValue<Dictionary<string, ValueContainer>>();
            }

            throw new PowerAutomateMockUpException("Can't get none object value container as dict.");
        }

        private ValueContainer JsonToValueContainer(JToken json)
        {
            if (json is JObject jObject)
            {
                var dictionary = json.ToDictionary(pair => ((JProperty) pair).Name, token =>
                {
                    if (token.Children().Count() != 1) return JsonToValueContainer(token.Children().First());

                    var t = token.First;
                    return t.Type switch
                    {
                        JTokenType.String => new ValueContainer(t.Value<string>(), true),
                        JTokenType.Boolean => new ValueContainer(t.Value<bool>()),
                        JTokenType.Integer => new ValueContainer(t.Value<int>()),
                        JTokenType.Float => new ValueContainer(t.Value<float>()),
                        _ => JsonToValueContainer(token.Children().First())
                    };
                });

                return new ValueContainer(dictionary);
            }

            if (json is JArray jArray)
            {
                return jArray.Count > 0 ? new ValueContainer() : JArrayToValueContainer(jArray);
            }

            throw new Exception();
        }

        private ValueContainer JArrayToValueContainer(JArray json)
        {
            var list = new List<ValueContainer>();
            
            foreach (var jToken in json)
            {
                if (jToken.GetType() != typeof(JValue))
                {
                    throw new PowerAutomateMockUpException("Json can only contain arrays of primitive types.");
                }

                var t = (JValue) jToken;
                switch (t.Value)
                {
                    case int i:
                        list.Add(new ValueContainer(i));
                        break;
                    case string s:
                        list.Add(new ValueContainer(s));
                        break;
                    case bool b:
                        list.Add(new ValueContainer(b));
                        break;
                    case double d:
                        list.Add(new ValueContainer(d));
                        break;
                    default:
                        throw new PowerAutomateMockUpException(
                            $"Type {t.Value.GetType()} is not recognized when converting Json to ValueContainer.");
                }
            }

            return new ValueContainer(list);
        }


        public override string ToString()
        {
            /*
             * This is called when debugging to display text in the variable overview.
             * This is also used when parsing.
             */
            return _type switch
            {
                ValueType.Boolean => _value.ToString(),
                ValueType.Integer => _value.ToString(),
                ValueType.Float => _value.ToString(),
                ValueType.String => _value,
                ValueType.Object => "{" + string.Join(",", GetValue<Dictionary<string, ValueContainer>>()
                    .Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}",
                ValueType.Array => "[" + string.Join(", ", GetValue<ValueContainer[]>().ToList()) + "]",
                ValueType.Null => "<null>",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool IsNull()
        {
            return _type == ValueType.Null;
        }
    }

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