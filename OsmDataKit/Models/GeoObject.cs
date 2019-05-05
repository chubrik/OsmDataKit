using Kit;
using OsmDataKit.Internal;
using OsmSharp;
using System;
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
        public IReadOnlyDictionary<string, string> Tags { get; }
        public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();

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
            get {
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
            set {
                _titleAssigned = true;
                _title = value;
            }
        }

        public bool HasTitle => Title != null;

        #endregion

        // protected
        internal GeoObject(OsmGeoData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Id = data.Id;
            Tags = data.Tags;
        }

        protected GeoObject(
            long id,
            IReadOnlyDictionary<string, string> tags,
            IReadOnlyDictionary<string, string> data)
        {
            Debug.Assert(tags != null);

            Id = id;
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));

            if (data != null)
                Data.AddRange(data);
        }

#if DEBUG
        private string DebugInfo =>
            Type.ToString()[0] + Id.ToString() + " - " + Title;
#endif
    }
}
