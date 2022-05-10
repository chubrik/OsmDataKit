namespace OsmDataKit.Internal;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

internal abstract class GeoObjectConverter<T> : JsonConverter<T> where T : GeoObject
{
    protected const string IdPropName = "i";
    protected const string TagsPropName = "g";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Dictionary<string, string>? ReadTagsJson(ref Utf8JsonReader reader)
    {
        reader.Read();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new InvalidOperationException();

        reader.Read();

        if (reader.TokenType == JsonTokenType.EndObject)
            return null;

        var tags = new Dictionary<string, string>();
        string key, value;

        for (; ; )
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new InvalidOperationException();

            key = reader.GetString()!;
            reader.Read();
            value = reader.GetString()!;
            tags.Add(key, value);
            reader.Read();

            if (reader.TokenType == JsonTokenType.EndObject)
                return tags;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void WriteTagsJson(Utf8JsonWriter writer, T value)
    {
        if (value.Tags?.Count > 0)
        {
            writer.WritePropertyName(TagsPropName);
            writer.WriteStartObject();

            foreach (var pair in value.Tags)
                writer.WriteString(pair.Key, pair.Value);

            writer.WriteEndObject();
        }
    }
}
