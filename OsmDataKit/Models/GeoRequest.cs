namespace OsmDataKit;

using System.Collections.Generic;

public sealed class GeoRequest
{
    public IEnumerable<long>? NodeIds { get; set; }
    public IEnumerable<long>? WayIds { get; set; }
    public IEnumerable<long>? RelationIds { get; set; }

    public GeoRequest() { }

    public GeoRequest(IEnumerable<long>? nodeIds, IEnumerable<long>? wayIds, IEnumerable<long>? relationIds)
    {
        NodeIds = nodeIds;
        WayIds = wayIds;
        RelationIds = relationIds;
    }
}
