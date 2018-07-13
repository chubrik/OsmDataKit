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
        public IReadOnlyDictionary<string, string> Tags { get; }
        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public abstract OsmGeoType Type { get; }
        public abstract bool IsBroken { get; }
        public abstract IGeoCoords AverageCoords { get; }

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
        }

        public bool HasTitle => Title != null;

        #endregion

        // Was protected
        internal OsmObject(OsmGeoData data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Id = data.Id;
            Tags = data.Tags;
        }

        protected OsmObject(
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
        private string DebugInfo() =>
            Type.ToString()[0] + Id.ToString() + " - " + Title;
#endif
    }
}
