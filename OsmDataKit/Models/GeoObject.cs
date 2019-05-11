using Kit;
using Newtonsoft.Json;
using OsmSharp;
using System.Collections.Generic;
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

        protected GeoObject(OsmGeo osmGeo)
        {
            Id = osmGeo.Id.GetValueOrDefault();

            if (osmGeo.Tags.Count > 0)
                _tagsOrNull = osmGeo.Tags.ToDictionary(i => i.Key, i => i.Value);
        }

        [JsonIgnore]
        public string OsmUrl => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        private static readonly List<string> _tagNames = new List<string> { "name:en", "int_name", "name", "name:ru" };

        public override string ToString()
        {
            var attr = Type.ToString()[0] + Id.ToString();

            if (_tagsOrNull != null)
                foreach (var tagName in _tagNames)
                    if (Tags.TryGetValue(tagName, out var title) && !title.IsNullOrWhiteSpace())
                        return attr + " - " + title.Trim();

            return attr;
        }
    }
}
