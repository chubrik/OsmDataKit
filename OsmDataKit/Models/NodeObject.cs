using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    [JsonObject]
    public class NodeObject : GeoObject, IGeoPoint
    {
        public override OsmGeoType Type => OsmGeoType.Node;

        [JsonIgnore]
        public float Latitude { get; set; }

        [JsonIgnore]
        public float Longitude { get; set; }

        public NodeObject() { }

        public NodeObject(
            long id, float latitude, float longitude, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Debug.Assert(latitude >= -90 && latitude <= 90);
            Debug.Assert(longitude >= -180 && longitude <= 180);

            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude));

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        internal NodeObject(Node node) : base(node)
        {
            Latitude = (float)node.Latitude.GetValueOrDefault();
            Longitude = (float)node.Longitude.GetValueOrDefault();
        }

        [JsonProperty("p")]
        public float[] _point
        {
            get => new[] { Latitude, Longitude };
            set
            {
                Debug.Assert(value?.Length == 2);
                Latitude = value[0];
                Longitude = value[1];
            }
        }
    }
}
