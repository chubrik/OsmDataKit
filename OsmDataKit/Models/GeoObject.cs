using OsmSharp;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    //#if DEBUG
    //    [DebuggerDisplay("{" + nameof(DebugInfo) + ",nq}")]
    //#endif
    public abstract class GeoObject
    {
        public abstract OsmGeoType Type { get; }

        public long Id { get; }

        private Dictionary<string, string> _tags;
        public Dictionary<string, string> Tags => _tags ?? (_tags = new Dictionary<string, string>(0));

        protected GeoObject(OsmGeo osmGeo)
        {
            Id = osmGeo.Id.GetValueOrDefault();

            if (osmGeo.Tags.Count > 0)
                _tags = osmGeo.Tags.ToDictionary(i => i.Key, i => i.Value);
        }

        //protected GeoObject(
        //    long id,
        //    IReadOnlyDictionary<string, string> tags,
        //    Dictionary<string, string> data)
        //{
        //    Debug.Assert(id > 0);

        //    Id = id;

        //    if (tags?.Count > 0)
        //        _tags = tags;

        //    if (data?.Count > 0)
        //        _data = data;
        //}

        //#region Title

        //private static readonly List<string> _tagNames = new List<string> { "name:en", "int_name", "name", "name:ru" };

        //private bool _titleAssigned;
        //private string _title;

        //public string Title
        //{
        //    get
        //    {
        //        if (_titleAssigned)
        //            return _title;

        //        _titleAssigned = true;

        //        if (_tags != null)
        //            foreach (var tagName in _tagNames)
        //                if (Tags.TryGetValue(tagName, out var title) && !title.IsNullOrWhiteSpace())
        //                    return _title = title.Trim();

        //        return null;
        //    }
        //    set
        //    {
        //        _titleAssigned = true;
        //        _title = value;
        //    }
        //}

        //public bool HasTitle => Title != null;

        //#endregion

        public string SourceUrl => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        //#if DEBUG
        //        private string DebugInfo =>
        //            Type.ToString()[0] + Id.ToString() + " - " + Title;
        //#endif
    }
}
