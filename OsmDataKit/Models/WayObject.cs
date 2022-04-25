using OsmDataKit.Internal;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace OsmDataKit
{
    [JsonConverter(typeof(WayObjectConverter))]
    public class WayObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Way;

        public IReadOnlyList<NodeObject>? Nodes { get; private set; }

        public IReadOnlyList<long>? MissedNodeIds { get; private set; }

        public bool IsComplete => MissedNodeIds == null;

        public WayObject(
            long id, IReadOnlyList<NodeObject> nodes, Dictionary<string, string>? tags = null)
            : base(id, tags)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            if (nodes.Count == 0)
                throw new ArgumentException(nameof(nodes));

            Nodes = nodes;
        }

        public WayObject(
            long id, IReadOnlyList<long> nodeIds, Dictionary<string, string>? tags = null)
            : base(id, tags)
        {
            if (nodeIds == null)
                throw new ArgumentNullException(nameof(nodeIds));

            if (nodeIds.Count == 0)
                throw new ArgumentException(nameof(nodeIds));

            MissedNodeIds = nodeIds;
        }

        public WayObject(Way way) : base(way)
        {
            MissedNodeIds = way.Nodes;
        }

        internal void FillNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(Nodes == null);
            Debug.Assert(nodes.Count > 0);

            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));

            var missedSet = new HashSet<long>(MissedNodeIds);

            foreach (var node in nodes)
                missedSet.Remove(node.Id);

            MissedNodeIds = missedSet.Count > 0 ? missedSet.ToList() : null;
        }
    }
}
