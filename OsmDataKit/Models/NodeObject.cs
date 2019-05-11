using OsmSharp;

namespace OsmDataKit
{
    public class NodeObject : GeoObject, IGeoCoords
    {
        public override OsmGeoType Type => OsmGeoType.Node;

        public float Latitude { get; }
        public float Longitude { get; }

        internal NodeObject(Node node) : base(node)
        {
            Latitude = (float)node.Latitude.GetValueOrDefault();
            Longitude = (float)node.Longitude.GetValueOrDefault();
        }

        //public NodeObject(
        //    long id, double latitude, double longitude,
        //    IReadOnlyDictionary<string, string> tags = null,
        //    Dictionary<string, string> data = null)
        //    : base(id, tags, data)
        //{
        //    Debug.Assert(latitude >= -90 && latitude <= 90);
        //    Debug.Assert(longitude >= -180 && longitude <= 180);

        //    Latitude = latitude;
        //    Longitude = longitude;
        //}
    }
}
