using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public class WayObject : GeoObject
    {
        //todo nodeIds?
        public IReadOnlyList<long> NodeIds { get; private set; }
        public IReadOnlyList<NodeObject> Nodes { get; private set; }

        public override OsmGeoType Type => OsmGeoType.Way;
        public override bool IsBroken => NodeIds.Count != Nodes.Count;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = Nodes.AverageCoords());

        public void SetNodes(IReadOnlyList<NodeObject> nodes)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
            _averageCoords = null;
        }

        public WayObject(
            long id,
            IReadOnlyList<NodeObject> nodes,
            IReadOnlyDictionary<string, string> tags = null,
            Dictionary<string, string> data = null)
            : base(id, tags, data)
        {
            Debug.Assert(nodes != null);

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            NodeIds = nodes.Select(i => i.Id).ToList();
            Nodes = nodes;
        }
    }
}
