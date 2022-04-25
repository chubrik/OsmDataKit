using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OsmDataKit.Internal
{
    internal class NodeObjectConverter : GeoObjectConverter<NodeObject>
    {
        private const string _locationPropName = "l";

        public override NodeObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new InvalidOperationException();

            long id = 0;
            Dictionary<string, string>? tags = null;
            Location? location = null;
            double latitude, longitude;

            for (; ; )
            {
                reader.Read();

                if (reader.TokenType == JsonTokenType.EndObject)
                    return new NodeObject(id, location!.Value, tags);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new InvalidOperationException();

                switch (reader.GetString())
                {
                    case IdPropName:
                        reader.Read();
                        id = reader.GetInt64();
                        break;

                    case TagsPropName:
                        tags = ReadTagsJson(ref reader);
                        break;

                    case _locationPropName:
                        reader.Read();

                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new InvalidOperationException();

                        reader.Read();
                        latitude = reader.GetDouble();
                        reader.Read();
                        longitude = reader.GetDouble();
                        location = new Location(latitude, longitude);
                        reader.Read();

                        if (reader.TokenType != JsonTokenType.EndArray)
                            throw new InvalidOperationException();

                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, NodeObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(IdPropName, value.Id);
            WriteTagsJson(writer, value);
            writer.WritePropertyName(_locationPropName);
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Latitude);
            writer.WriteNumberValue(value.Longitude);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
