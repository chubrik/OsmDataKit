using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    [DebuggerDisplay("{" + nameof(DebugInfo) + "(),nq}")]
    public abstract class OsmObject
    {
        public long Id { get; }
        public string Title { get; }
        public OsmGeoType Type { get; protected set; }
        public IDictionary<string, string> Tags { get; }

        // Was protected
        internal OsmObject(OsmGeoData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Id = data.Id;
            Tags = data.Tags;

            // Title should be after Tags
            Title = OsmHelper.TryGetTitle(this);
        }

        public abstract bool IsBroken();

        public abstract GeoCoords AverageCoords();

        private string DebugInfo() =>
            Type.ToString()[0] + Id.ToString() + " - " + Title;
    }
}
