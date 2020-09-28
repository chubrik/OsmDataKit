using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OsmDataKit.Internal
{
    internal class NodeObjectConverter : GeoObjectConverter<NodeObject>
    {
        private const string _locationPropName = "l";

        public override NodeObject ReadJson(JsonReader reader, Type objectType, NodeObject? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();

            long id = 0;
            Dictionary<string, string>? tags = null;
            Location? location = null;

            for (; ; )
            {
                reader.Read();

                if (reader.TokenType == JsonToken.EndObject)
                    return new NodeObject(id, location!.Value, tags);

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidOperationException();

                switch (reader.Value)
                {
                    case IdPropName:
                        reader.Read();
                        id = (long)reader.Value;
                        break;

                    case TagsPropName:
                        tags = ReadTagsJson(reader);
                        break;

                    case _locationPropName:
                        reader.Read();

                        if (reader.TokenType != JsonToken.StartArray)
                            throw new InvalidOperationException();

                        location = new Location(reader.ReadAsDouble()!.Value, reader.ReadAsDouble()!.Value);

                        reader.Read();

                        if (reader.TokenType != JsonToken.EndArray)
                            throw new InvalidOperationException();

                        break;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, NodeObject? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(IdPropName);
            writer.WriteValue(value!.Id);
            WriteTagsJson(writer, value);
            writer.WritePropertyName(_locationPropName);
            writer.WriteStartArray();
            writer.WriteValue(value.Latitude);
            writer.WriteValue(value.Longitude);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
