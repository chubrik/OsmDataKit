namespace OsmDataKit.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

public abstract class TestsBase
{
    protected static readonly string PbfPath = @"..\..\..\App_Data\antarctica.osm.pbf";

    [TestInitialize]
    public void BaseInitialize()
    {
        OsmService.CacheDirectory = @$"$osm-cache\{DateTimeOffset.Now:yyyy-MM-dd--HH-mm-ss}";
    }
}
