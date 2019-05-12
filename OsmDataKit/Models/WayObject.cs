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

        public WayObject(
            long id, IReadOnlyList<NodeObject> nodes, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Debug.Assert(nodes != null);
            Debug.Assert(nodes?.Count > 0);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            if (nodes.Count == 0)
                throw new ArgumentException(nameof(nodes));

            Nodes = nodes;
        }

        internal WayObject(Way way) : base(way)
        {
            MissedNodeIds = way.Nodes;
        }

        [JsonIgnore]
        public bool HasMissedNodes => MissedNodeIds?.Count > 0;

        internal void ReplaceNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(Nodes == null);
            Debug.Assert(nodes?.Count > 0);

            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            MissedNodeIds = null;
        }

        internal void FillNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(Nodes == null);
            Debug.Assert(nodes != null);
            Debug.Assert(nodes?.Count > 0);

            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));

            var missedSet = new HashSet<long>(MissedNodeIds);

            foreach (var node in nodes)
                missedSet.Remove(node.Id);

            MissedNodeIds = missedSet.Count > 0 ? missedSet.ToList() : null;
        }
    }
}
