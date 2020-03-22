using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OsmDataKit.Tests
{
    public abstract class TestsBase
    {
        private static readonly string BaseDirectory = Kit.Kit.BaseDirectory;

        protected static readonly string PbfPath = BaseDirectory + @"\App_Data\antarctica.osm.pbf";

        [TestInitialize]
        public void BaseInitialize()
        {
            Kit.Kit.Setup(test: true, baseDirectory: BaseDirectory);
        }

        public static void TestInitialize(string testName)
        {
            var workingDir = @"$work\" + testName;
            Kit.Kit.Setup(workingDirectory: workingDir);

            var nativeWorkingDir = BaseDirectory + @"\" + workingDir;
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
