using Kit;
using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public abstract class GeoObject
    {
        [JsonIgnore]
        public abstract OsmGeoType Type { get; }

        [JsonProperty("i")]
        public long Id { get; set; }

        [JsonProperty("g", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> _tagsOrNull { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Tags => _tagsOrNull ?? (_tagsOrNull = new Dictionary<string, string>(0));

        protected GeoObject() { }

        protected GeoObject(long id, Dictionary<string, string> tags)
        {
            Debug.Assert(id > 0);

            if (id <= 0)
                throw new ArgumentException(nameof(id));

            Id = id;
            _tagsOrNull = tags?.Count > 0 ? tags : null;
        }

        protected GeoObject(OsmGeo osmGeo)
        {
            Id = osmGeo.Id.GetValueOrDefault();

            if (osmGeo.Tags.Count > 0)
                _tagsOrNull = osmGeo.Tags.ToDictionary(i => i.Key, i => i.Value);
        }

        [JsonIgnore]
        public string Url => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        private static readonly List<string> _tagNames = new List<string> { "name:en", "int_name", "name", "name:ru" };

        public static Func<GeoObject, string> TitleFormatter =
            geo =>
            {
                if (geo._tagsOrNull != null)
                    foreach (var tagName in _tagNames)
                        if (geo.Tags.TryGetValue(tagName, out var title) && !title.IsNullOrWhiteSpace())
                            return title.Trim();

                return null;
            };

        public override string ToString()
        {
            var attr = Type.ToString()[0] + Id.ToString();
            var title = TitleFormatter(this);
            return title != null ? attr + " - " + title : attr;
        }
    }
}
