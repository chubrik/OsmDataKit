using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OsmDataKit.Tests
{
    public abstract class TestsBase
    {
        protected static readonly string PbfPath = Kit.Kit.BaseDirectory + @"\App_Data\antarctica.osm.pbf";

        [TestInitialize]
        public void BaseInitialize()
        {
            Kit.Kit.Setup(isTest: true, useFileDiagnostics: true);
        }

        public static void TestInitialize(string testName)
        {
            var workingDir = @"$work\" + testName;
            Kit.Kit.Setup(workingDirectory: workingDir, diagnosticsDirectory: "$diagnostics");

            var nativeWorkingDir = Kit.Kit.BaseDirectory + @"\" + workingDir;
            OsmService.CacheDirectory = nativeWorkingDir + @"\$osm-cache";

            if (Directory.Exists(nativeWorkingDir))
            {
                foreach (var file in Directory.GetFiles(nativeWorkingDir))
                    File.Delete(file);

                Assert.IsTrue(Directory.GetFiles(nativeWorkingDir).Length == 0);
            }
        }
    }
}
