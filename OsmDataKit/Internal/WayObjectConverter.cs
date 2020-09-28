using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OsmDataKit.Internal
{
    internal class WayObjectConverter : GeoObjectConverter<WayObject>
    {
        private const string _nodeIdsPropName = "n";

        public override WayObject ReadJson(JsonReader reader, Type objectType, WayObject? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();

            long id = 0;
            Dictionary<string, string>? tags = null;
            var nodeIds = new List<long>();

            for (; ; )
            {
                reader.Read();

                if (reader.TokenType == JsonToken.EndObject)
                    return new WayObject(id, nodeIds, tags);

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

                    case _nodeIdsPropName:
                        reader.Read();

                        if (reader.TokenType != JsonToken.StartArray)
                            throw new InvalidOperationException();

                        NextNodeId:

                        reader.Read();

                        if (reader.TokenType == JsonToken.EndArray)
                            break;

                        nodeIds.Add((long)reader.Value);
                        goto NextNodeId;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, WayObject? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(IdPropName);
            writer.WriteValue(value!.Id);
            WriteTagsJson(writer, value);
            writer.WritePropertyName(_nodeIdsPropName);
            writer.WriteStartArray();

            foreach (var nodeId in value.MissedNodeIds!)
                writer.WriteValue(nodeId);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
