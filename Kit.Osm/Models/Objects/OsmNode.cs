using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    public class OsmNode : OsmObject, IGeoCoords
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public override OsmGeoType Type => OsmGeoType.Node;
        public override bool IsBroken => false;
        public override IGeoCoords AverageCoords => this;

        internal OsmNode(NodeData data) : base(data)
        {
            Debug.Assert(data?.Coords?.Length == 2);

            if (data?.Coords?.Length != 2)
                throw new ArgumentException(nameof(data));

            Latitude = data.Coords[0];
            Longitude = data.Coords[1];
        }

        public OsmNode(
            long id, IReadOnlyDictionary<string, string> tags,
            IGeoCoords coords,
            IReadOnlyDictionary<string, string> data = null)
            : base(id, tags, data)
        {
            Debug.Assert(coords != null);

            if (coords == null)
                throw new ArgumentNullException(nameof(coords));

            Latitude = coords.Latitude;
            Longitude = coords.Longitude;
        }
    }
}
