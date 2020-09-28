using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OsmDataKit.Internal
{
    internal abstract class GeoObjectConverter<T> : JsonConverter<T> where T : GeoObject
    {
        protected const string IdPropName = "i";
        protected const string TagsPropName = "g";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Dictionary<string, string>? ReadTagsJson(JsonReader reader)
        {
            reader.Read();

            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();

            reader.Read();

            if (reader.TokenType == JsonToken.EndObject)
                return null;

            var tags = new Dictionary<string, string>();

            for (; ; )
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidOperationException();

                tags.Add((string)reader.Value!, reader.ReadAsString()!);

                reader.Read();

                if (reader.TokenType == JsonToken.EndObject)
                    return tags;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteTagsJson(JsonWriter writer, T value)
        {
            if (value.Tags?.Count > 0)
            {
                writer.WritePropertyName(TagsPropName);
                writer.WriteStartObject();

                foreach (var pair in value.Tags)
                {
                    writer.WritePropertyName(pair.Key);
                    writer.WriteValue(pair.Value);
                }

                writer.WriteEndObject();
            }
        }
    }
}
