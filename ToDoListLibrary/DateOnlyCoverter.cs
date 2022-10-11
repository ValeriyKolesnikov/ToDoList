using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary
{
    public class DateOnlyCoverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateOnly))
                return true;
            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            var strDate = reader.Value.ToString();
            if (DateOnly.TryParseExact(strDate, "dd.MM.yyyy", out DateOnly date))
                return date;
            return null;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is DateOnly date)
                writer.WriteValue(date.ToString());
        }
    }
}
