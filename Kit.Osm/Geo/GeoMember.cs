using System;
using System.Diagnostics;

namespace Kit.Osm
{
    [DebuggerDisplay("{" + nameof(DebugInfo) + "(),nq}")]
    public class GeoMember
    {
        public string Role { get; }
        public GeoObject Geo { get; }

        internal GeoMember(OsmMemberData data, GeoObject geo)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Debug.Assert(geo != null);

            if (geo == null)
                throw new ArgumentNullException(nameof(geo));

            Debug.Assert(geo.Type != data.Type);

            if (geo.Type != data.Type)
                throw new ArgumentException(nameof(geo));

            Role = data.Role;
            Geo = geo;
        }

        private string DebugInfo() =>
            Geo.Type.ToString()[0] + Geo.Id.ToString() + " - " + Role + " - " + Geo.TryGetTitle();
    }
}
