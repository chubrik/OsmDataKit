using Kit;
using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;

namespace OsmDataKit.Internal
{
    internal class RelationObjectConverter : GeoObjectConverter<RelationObject>
    {
        private const string MembersPropName = "m";
        private const string MemberTypePropName = "t";
        private const string MemberIdPropName = IdPropName;
        private const string MemberRolePropName = "r";

        public override RelationObject ReadJson(JsonReader reader, Type objectType, RelationObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new InvalidOperationException();

            long relationId = 0;
            Dictionary<string, string> tags = null;
            var memberInfos = new List<RelationMemberInfo>();

            for (; ; )
            {
                reader.Read();

                if (reader.TokenType == JsonToken.EndObject)
                    return new RelationObject(relationId, memberInfos, tags);

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidOperationException();

                switch (reader.Value)
                {
                    case IdPropName:
                        reader.Read();
                        relationId = (long)reader.Value;
                        break;

                    case TagsPropName:
                        tags = ReadTagsJson(reader);
                        break;

                    case MembersPropName:
                        reader.Read();

                        if (reader.TokenType != JsonToken.StartArray)
                            throw new InvalidOperationException();

                        for (; ; )
                        {
                            reader.Read();

                            if (reader.TokenType == JsonToken.EndArray)
                                break;

                            if (reader.TokenType != JsonToken.StartObject)
                                throw new InvalidOperationException();

                            OsmGeoType? memberType = null;
                            long memberId = 0;
                            string memberRole = null;

                            for (; ; )
                            {
                                reader.Read();

                                if (reader.TokenType == JsonToken.EndObject)
                                {
                                    memberInfos.Add(new RelationMemberInfo(memberType.Value, memberId, memberRole));
                                    break;
                                }

                                if (reader.TokenType != JsonToken.PropertyName)
                                    throw new InvalidOperationException();

                                switch (reader.Value)
                                {
                                    case MemberTypePropName:
                                        memberType = (OsmGeoType)reader.ReadAsInt32();
                                        break;

                                    case MemberIdPropName:
                                        reader.Read();
                                        memberId = (long)reader.Value;
                                        break;

                                    case MemberRolePropName:
                                        memberRole = reader.ReadAsString();
                                        break;
                                }
                            }
                        }

                        break;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, RelationObject value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(IdPropName);
            writer.WriteValue(value.Id);
            WriteTagsJson(writer, value);
            writer.WritePropertyName(MembersPropName);
            writer.WriteStartArray();

            foreach (var member in value.MissedMembers)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(MemberTypePropName);
                writer.WriteValue((int)member.Type);
                writer.WritePropertyName(IdPropName);
                writer.WriteValue(member.Id);

                if (!member.Role.IsNullOrWhiteSpace())
                {
                    writer.WritePropertyName(MemberRolePropName);
                    writer.WriteValue(member.Role);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
