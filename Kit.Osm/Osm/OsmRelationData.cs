using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmRelationData : OsmObjectData
    {
        [JsonProperty("m")]
        public List<OsmMemberData> Members { get; private set; }

        public OsmRelationData() { }

        public OsmRelationData(Relation relation) : base(relation)
        {
            Debug.Assert(relation.Members != null);

            if (relation.Members == null)
                throw new ArgumentException(nameof(relation));

            Members = relation.Members.Select(i => new OsmMemberData(i)).ToList();
        }
    }
}
