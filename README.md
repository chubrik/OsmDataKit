# [![OsmDataKit project](https://raw.githubusercontent.com/chubrik/OsmDataKit/main/icon.png)](#) OsmDataKit
[![NuGet package](https://img.shields.io/nuget/v/OsmDataKit)](https://www.nuget.org/packages/OsmDataKit/)
[![MIT license](https://img.shields.io/github/license/chubrik/OsmDataKit)](https://github.com/chubrik/OsmDataKit/blob/main/LICENSE) 
[![Nuget downloads](https://img.shields.io/nuget/dt/OsmDataKit)](https://www.nuget.org/packages/OsmDataKit/)
[![GitHub stars](https://img.shields.io/github/stars/chubrik/OsmDataKit?style=social)](https://github.com/chubrik/OsmDataKit/stargazers)
<br><br>

- [Install](#install)
- [Usage](#usage)
- [Projects](#projects)
- [License](#license)
<br>

The **OsmDataKit** is a tool for convenient work with the OpenStreetMap database.
Allows loading from a PBF file a set of elementary objects as well as nested tree structures.
It caches load results. Resistant to the integrity violations of the base.
Saves RAM, allows you to work with whole continents.

#### Problems in [OsmSharp](https://github.com/OsmSharp/core) and their solution in OsmDataKit:

1. **Lack of RAM.** OsmSharp uses the principle of “loading the entire database into RAM, and we’ll figure it out”
to build complete objects. Even working with a small country, you can easily use up 16 GB of RAM.
But what if you need the whole world?
    - **Solution.** OsmDataKit makes three loads from the database sequentially to build complete objects.
    In uses only the necessary minimum of information, such as coordinates, tags and relationships between elements.

2. **Crash on integrity violation.** There are inevitably some integrity violations in the OSM database.
For example, one Way may lose one Node. In this situation, OsmSharp will crash and you won’t get anything.
    - **Solution.** If there is no element in the database, OsmDataKit will still construct a complete object,
    convenient for use in your project. Information on all missed elements will be available in the load results.

3. **Complex and inconvenient requests.**
When using OsmSharp, you cannot just make a conceptual query and get what you need.
It is required to specify in the request absolutely all elements
that *potentially* may be part of complete objects. Any mistake you make will result in a crash.
    - **Solution.** When using OsmDataKit, you need to specify the minimum information in the request.
    All dependent elements upon loading will be automatically found and assembled into complete objects.
<br><br><br>

## <a name="install"></a>Install
    PM> Install-Package OsmDataKit
<br>

## <a name="usage"></a>Usage
```csharp
using OsmDataKit;

// Sample queries by filter and by ID’s
bool requestByFilter(OsmGeo geo) => geo.Tags.ContainsKey("place");
var requestByIds = new GeoRequest(nodeIds, wayIds, relationIds);

// Loading elementary geo objects
GeoObjectSet geosByFilter = OsmService.LoadObjects(pathToPbf, requestByFilter);
GeoObjectSet geosByIds = OsmService.LoadObjects(pathToPbf, requestByIds);

// Loading complete geo objects
CompleteGeoObjects completeByFilter = OsmService.LoadCompleteObjects(pathToPbf, requestByFilter);
CompleteGeoObjects completeByIds = OsmService.LoadCompleteObjects(pathToPbf, requestByIds);
```
<br>

The result of loading elementary geo objects will be the class `GeoObjectSet`.
It contains flat lists of all the geo objects found by your request.
Also contains the ID’s of objects not found in the database.
```csharp
class GeoObjectSet
{
  NodeObject[] Nodes
  WayObject[] Ways
  RelationObject[] Relations
  long[] MissedNodeIds
  long[] MissedWayIds
  long[] MissedRelationIds
}
```
<br>

The result of loading complete geo objects will be the class `CompleteGeoObjects`.
It contains top-level geo objects, and all other elements are nested in a tree, according to their conceptual hierarchy.
Also contains the ID’s of objects not found in the database.
```csharp
class CompleteGeoObjects
{
  NodeObject[] RootNodes
  WayObject[] RootWays
  RelationObject[] RootRelations
  long[] MissedNodeIds
  long[] MissedWayIds
  long[] MissedRelationIds
}
```

### Caching
Loading from the database takes significant time, for continents it can be hours.
OsmDataKit allows you to cache the result of any request to a file. To do this, specify the argument `cacheName`.
```csharp
OsmService.CacheDirectory = "osm-cache";

var geosByFilter = OsmService.LoadObjects(pathToPbf, cacheName: "geosByFilter", requestByFilter);
var geosByIds = OsmService.LoadObjects(pathToPbf, cacheName: "geosByIds", requestByIds);
var completeByFilter = OsmService.LoadCompleteObjects(pathToPbf, cacheName: "completeByFilter", requestByFilter);
var completeByIds = OsmService.LoadCompleteObjects(pathToPbf, cacheName: "completeByIds", requestByIds);
```
<br>

## <a name="projects"></a>Projects
The OsmDataKit was designed for toponymic science projects —
[Toponim.by](https://github.com/chubrik/Toponym) and [Toponym.org](https://toponym.org/).
<br><br><br>

## <a name="license"></a>License
The OsmDataKit is licensed under the [MIT license](https://github.com/chubrik/OsmDataKit/blob/main/LICENSE).
<br><br>
