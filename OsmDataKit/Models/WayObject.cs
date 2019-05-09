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

        //todo nodeIds?
        public IReadOnlyList<long> NodeIds { get; private set; }
        public IReadOnlyList<NodeObject> Nodes { get; private set; }

        public WayObject(
            long id,
            IReadOnlyList<NodeObject> nodes,
            IReadOnlyDictionary<string, string> tags = null,
            Dictionary<string, string> data = null)
            : base(id, tags, data)
        {
            Debug.Assert(nodes?.Count > 0);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            if (nodes.Count == 0)
                throw new ArgumentException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
        }

        public bool IsBroken => NodeIds.Count != Nodes.Count;

        public void SetNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
        }
    }
}
