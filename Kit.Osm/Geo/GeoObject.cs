using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    [DebuggerDisplay("{" + nameof(DebugInfo) + "(),nq}")]
    public abstract class GeoObject
    {
        public long Id { get; protected set; }
        public string Title { get; protected set; }
        public GeoType Type { get; protected set; }
        public IDictionary<string, string> Tags { get; protected set; }

        // was protected
        internal GeoObject(OsmObjectData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Id = data.Id;
            Tags = data.Tags;

            // Title should be after Tags
            Title = this.TryGetTitle();
        }

        public abstract bool IsBroken();

        public bool HasTitle => !Title.IsNullOrWhiteSpace();

        public abstract GeoCoords AverageCoords();

        private string DebugInfo() =>
            Type.ToString()[0] + Id.ToString() + " - " + Title;
    }
}
