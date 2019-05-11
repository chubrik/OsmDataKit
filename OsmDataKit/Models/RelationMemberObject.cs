using OsmSharp;
using System;
using System.Diagnostics;

namespace OsmDataKit
{
    public class RelationMemberObject
    {
        public string Role { get; }

        public GeoObject Geo { get; }

        public OsmGeoType Type => Geo.Type;

        public long Id => Geo.Id;

        internal RelationMemberObject(string role, GeoObject geo)
        {
            Debug.Assert(role != null);
            Debug.Assert(geo != null);

            Role = role;
            Geo = geo ?? throw new ArgumentNullException(nameof(geo));
        }

        public override string ToString() => Role +  " - " + Geo.ToString();
    }
}
