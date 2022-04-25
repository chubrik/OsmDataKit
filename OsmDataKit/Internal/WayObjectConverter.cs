using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OsmDataKit.Internal
{
    internal class WayObjectConverter : GeoObjectConverter<WayObject>
    {
        private const string _nodeIdsPropName = "n";

        public override WayObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new InvalidOperationException();

            long id = 0;
            Dictionary<string, string>? tags = null;
            var nodeIds = new List<long>();

            for (; ; )
            {
                reader.Read();

                if (reader.TokenType == JsonTokenType.EndObject)
                    return new WayObject(id, nodeIds, tags);

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

                    case _nodeIdsPropName:
                        reader.Read();

                        if (reader.TokenType != JsonTokenType.StartArray)
                            throw new InvalidOperationException();

                        NextNodeId:

                        reader.Read();

                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;

                        nodeIds.Add(reader.GetInt64());
                        goto NextNodeId;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, WayObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(IdPropName, value.Id);
            WriteTagsJson(writer, value);
            writer.WritePropertyName(_nodeIdsPropName);
            writer.WriteStartArray();

            foreach (var nodeId in value.MissedNodeIds!)
                writer.WriteNumberValue(nodeId);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
