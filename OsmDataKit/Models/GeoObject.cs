using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public abstract class GeoObject
    {
        public abstract OsmGeoType Type { get; }

        public long Id { get; }

        public Dictionary<string, string>? Tags { get; set; }

        public string Url => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        protected GeoObject(long id, Dictionary<string, string>? tags)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Id = id;
            Tags = tags?.Count > 0 ? tags : null;
        }

        protected GeoObject(OsmGeo osmGeo)
        {
            Id = osmGeo.Id!.Value;

            if (osmGeo.Tags.Count > 0)
                Tags = osmGeo.Tags.ToDictionary(i => i.Key, i => i.Value);
        }

        private static readonly string[] _nameTags = new[] { "name:en", "int_name", "name", "name:ru" };

        public static Func<GeoObject, string> StringFormatter =
            geo =>
            {
                var attr = geo.Type.ToString()[0] + geo.Id.ToString();

                if (geo.Tags != null)
                    foreach (var nameTag in _nameTags)
                        if (geo.Tags.TryGetValue(nameTag, out var name) && !string.IsNullOrWhiteSpace(name))
                            return attr + " - " + name.Trim();

                return attr;
            };

        public override string ToString() => StringFormatter(this);
    }
}
