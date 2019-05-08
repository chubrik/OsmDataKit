using Kit;
using System;
using System.Diagnostics;

namespace OsmDataKit
{
#if DEBUG
    [DebuggerDisplay("{" + nameof(DebugInfo) + ",nq}")]
#endif
    public class RelationMemberObject
    {
        public string Role { get; }
        public GeoObject Geo { get; }

        public RelationMemberObject(string role, GeoObject geo)
        {
            Debug.Assert(!role.IsNullOrEmpty());
            Debug.Assert(geo != null);

            if (role.IsNullOrEmpty())
                throw new ArgumentException(nameof(role));

            Role = role;
            Geo = geo ?? throw new ArgumentNullException(nameof(geo));
        }

#if DEBUG
        private string DebugInfo =>
            Geo.Type.ToString()[0] + Geo.Id.ToString() + " - " + Role + " - " + Geo.Title;
#endif
    }
}
