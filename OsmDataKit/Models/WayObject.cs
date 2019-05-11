using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    [JsonObject]
    public class WayObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Way;

        [JsonIgnore]
        public IReadOnlyList<NodeObject> Nodes { get; set; }

        [JsonProperty("n")]
        public IReadOnlyList<long> MissedNodeIds { get; set; }

        public WayObject() { }

        internal WayObject(Way way) : base(way)
        {
            MissedNodeIds = way.Nodes;
        }

        internal bool HasMissedNodes => MissedNodeIds?.Count > 0;

        internal void SetNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(Nodes == null);
            Debug.Assert(nodes?.Count > 0);

            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));

            var missedSet = new HashSet<long>(MissedNodeIds);

            foreach (var node in nodes)
                if (!missedSet.Remove(node.Id))
                    throw new InvalidOperationException();

            MissedNodeIds = missedSet.Count > 0 ? missedSet.ToList() : null;
        }
    }
}
