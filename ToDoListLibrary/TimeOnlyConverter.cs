using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary
{
    internal class TimeOnlyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeOnly))
                return true;
            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            var strTime = reader.Value.ToString();
            if (TimeOnly.TryParseExact(strTime, "HH:mm", out TimeOnly time))
                return time;
            return null;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is TimeOnly time)
                writer.WriteValue(time.ToString());
        }
    }
}
