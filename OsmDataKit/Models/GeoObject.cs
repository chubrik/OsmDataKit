using Kit;
using OsmSharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
#if DEBUG
    [DebuggerDisplay("{" + nameof(DebugInfo) + ",nq}")]
#endif
    public abstract class GeoObject
    {
        public long Id { get; }

        //todo tags & data
        public IReadOnlyDictionary<string, string> Tags { get; }
        public Dictionary<string, string> Data { get; }

        public abstract OsmGeoType Type { get; }
        public abstract bool IsBroken { get; }
        public abstract IGeoCoords AverageCoords { get; }
        public string Url => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        #region Title

        private static readonly List<string> _tagNames = new List<string> { "name:en", "int_name", "name", "name:ru" };

        private bool _titleAssigned;
        private string _title;

        public string Title
        {
            get
            {
                if (_titleAssigned)
                    return _title;

                _titleAssigned = true;

                if (Tags.Count == 0)
                    return null;

                foreach (var tagName in _tagNames)
                    if (Tags.TryGetValue(tagName, out var title) && !title.IsNullOrWhiteSpace())
                        return _title = title.Trim();

                return null;
            }
            set
            {
                _titleAssigned = true;
                _title = value;
            }
        }

        public bool HasTitle => Title != null;

        #endregion

        protected GeoObject(
            long id,
            IReadOnlyDictionary<string, string> tags,
            Dictionary<string, string> data)
        {
            Id = id;
            Tags = tags;
            Data = data;
        }

#if DEBUG
        private string DebugInfo =>
            Type.ToString()[0] + Id.ToString() + " - " + Title;
#endif
    }
}
