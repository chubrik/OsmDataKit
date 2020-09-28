using OsmSharp;
using System;

namespace OsmDataKit
{
    public class RelationMemberObject
    {
        public GeoObject Geo { get; }

        public string? Role { get; }

        public OsmGeoType Type => Geo.Type;

        public long Id => Geo.Id;

        public string Url => Geo.Url;

        public RelationMemberObject(GeoObject geo, string? role)
        {
            Geo = geo ?? throw new ArgumentNullException(nameof(geo));
            Role = role;
        }

        public static Func<RelationMemberObject, string> StringFormatter =
            member => member.Role + " - " + member.Geo.ToString();

        public override string ToString() => StringFormatter(this);
    }
}
