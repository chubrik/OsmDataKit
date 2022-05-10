namespace OsmDataKit.Internal;

using OsmSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;

internal class RelationObjectConverter : GeoObjectConverter<RelationObject>
{
    private const string _membersPropName = "m";
    private const string _memberTypePropName = "t";
    private const string _memberIdPropName = IdPropName;
    private const string _memberRolePropName = "r";

    public override RelationObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new InvalidOperationException();

        long relationId = 0;
        Dictionary<string, string>? tags = null;
        var memberInfos = new List<RelationMemberInfo>();

        for (; ; )
        {
            reader.Read();

            if (reader.TokenType == JsonTokenType.EndObject)
                return new RelationObject(relationId, memberInfos, tags);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new InvalidOperationException();

            switch (reader.GetString())
            {
                case IdPropName:
                    reader.Read();
                    relationId = reader.GetInt64();
                    break;

                case TagsPropName:
                    tags = ReadTagsJson(ref reader);
                    break;

                case _membersPropName:
                    reader.Read();

                    if (reader.TokenType != JsonTokenType.StartArray)
                        throw new InvalidOperationException();

                    for (; ; )
                    {
                        reader.Read();

                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;

                        if (reader.TokenType != JsonTokenType.StartObject)
                            throw new InvalidOperationException();

                        OsmGeoType? memberType = null;
                        long memberId = 0;
                        string? memberRole = null;

                        for (; ; )
                        {
                            reader.Read();

                            if (reader.TokenType == JsonTokenType.EndObject)
                            {
                                memberInfos.Add(new RelationMemberInfo(memberType!.Value, memberId, memberRole));
                                break;
                            }

                            if (reader.TokenType != JsonTokenType.PropertyName)
                                throw new InvalidOperationException();

                            switch (reader.GetString())
                            {
                                case _memberTypePropName:
                                    reader.Read();
                                    memberType = (OsmGeoType)reader.GetInt32();
                                    break;

                                case _memberIdPropName:
                                    reader.Read();
                                    memberId = reader.GetInt64();
                                    break;

                                case _memberRolePropName:
                                    reader.Read();
                                    memberRole = reader.GetString();
                                    break;

                                default:
                                    throw new InvalidOperationException();
                            }
                        }
                    }

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, RelationObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(IdPropName, value.Id);
        WriteTagsJson(writer, value);
        writer.WritePropertyName(_membersPropName);
        writer.WriteStartArray();

        foreach (var member in value.MissedMembers!)
        {
            writer.WriteStartObject();
            writer.WriteNumber(_memberTypePropName, (int)member.Type);
            writer.WriteNumber(IdPropName, member.Id);

            if (!string.IsNullOrWhiteSpace(member.Role))
                writer.WriteString(_memberRolePropName, member.Role);

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
