using System;
using Newtonsoft.Json;

namespace Parser.ExpressionParser
{
    public class ValueContainerConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Ignored - This will not be implemented until it throws an error the first time.
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return new ValueContainer((string) reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ValueContainer);
        }
    }
}