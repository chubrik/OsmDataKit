using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public class WayObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Way;

        public IReadOnlyList<NodeObject> Nodes { get; private set; }

        public IReadOnlyList<long> MissedNodeIds { get; private set; }

        internal WayObject(Way way) : base(way)
        {
            MissedNodeIds = way.Nodes;
        }

        internal bool HasMissedNodes => MissedNodeIds?.Count > 0;

        //public WayObject(
        //    long id,
        //    IReadOnlyList<NodeObject> nodes,
        //    IReadOnlyDictionary<string, string> tags = null,
        //    Dictionary<string, string> data = null)
        //    : base(id, tags, data)
        //{
        //    Debug.Assert(nodes?.Count > 0);

        //    if (nodes == null)
        //        throw new ArgumentNullException(nameof(nodes));

        //    if (nodes.Count == 0)
        //        throw new ArgumentException(nameof(nodes));

        //    NodeIds = nodes.Select(i => i.Id).ToList();
        //    Nodes = nodes;
        //}

        //public bool IsBroken => NodeIds.Count != Nodes.Count;

        internal void SetNodes(IReadOnlyList<NodeObject> nodes)
        {
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
