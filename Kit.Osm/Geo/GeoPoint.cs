using OsmSharp;
using System;
using System.Diagnostics;

namespace Kit.Osm
{
    public class OsmNode : OsmObject
    {
        public double Latitude { get; }
        public double Longitude { get; }

        internal OsmNode(NodeData data) : base(data)
        {
            Debug.Assert(data?.Coords?.Length == 2);

            if (data?.Coords?.Length != 2)
                throw new ArgumentException(nameof(data));

            Type = OsmGeoType.Node;
            Latitude = data.Coords[0];
            Longitude = data.Coords[1];
        }

        public override bool IsBroken() => false;

        public override GeoCoords AverageCoords() => new GeoCoords(Latitude, Longitude);
    }
}
