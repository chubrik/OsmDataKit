using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class GeoLine : GeoObject
    {
        public IReadOnlyList<long> PointIds { get; }
        public IReadOnlyList<GeoPoint> Points { get; }

        public override bool IsBroken() => PointIds.Count != Points.Count;

        public override GeoCoords AverageCoords() => Points.AverageCoords();

        internal GeoLine(OsmWayData data, IDictionary<long, GeoPoint> allPoints) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Debug.Assert(allPoints != null);

            if (allPoints == null)
                throw new ArgumentNullException(nameof(allPoints));

            Type = GeoType.Line;
            PointIds = data.NodeIds;
            Points = data.NodeIds.Where(allPoints.ContainsKey).Select(i => allPoints[i]).ToList();
        }
    }
}
