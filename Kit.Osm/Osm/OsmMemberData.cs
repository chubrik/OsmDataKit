using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Diagnostics;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmMemberData
    {
        [JsonProperty("i")]
        public long Id { get; private set; }

        [JsonProperty("t")]
        public GeoType Type { get; private set; }

        [JsonProperty("r")]
        public string Role { get; private set; }

        public OsmMemberData() { }

        public OsmMemberData(RelationMember member)
        {
            Debug.Assert(member.Role != null);

            if (member.Role == null)
                throw new ArgumentException(nameof(member));

            Id = member.Id;
            Type = member.Type.ToGeoType();
            Role = member.Role;
        }
    }
}
