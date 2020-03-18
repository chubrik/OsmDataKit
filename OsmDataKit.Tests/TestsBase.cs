using Kit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OsmDataKit.Tests
{
    public abstract class TestsBase
    {
        private static readonly string BaseDirectory =
            Environment.GetEnvironmentVariable("VisualStudioDir") != null
                ? PathHelper.Combine(Environment.CurrentDirectory, "../../..")
                : Environment.CurrentDirectory;

        protected static readonly string PbfPath =
            PathHelper.Combine(BaseDirectory, "App_Data/antarctica.osm.pbf");

        [TestInitialize]
        public void BaseInitialize()
        {
            Kit.Kit.Setup(test: true, baseDirectory: BaseDirectory);
            ConsoleClient.Setup(minLevel: LogLevel.Log);
        }

        public static void TestInitialize(string testName)
        {
            var workingDir = "$work/" + testName;
            Kit.Kit.Setup(workingDirectory: workingDir);

            var nativeWorkingDir = BaseDirectory + "/" + workingDir;
            OsmService.CacheDir = nativeWorkingDir + "/$osm-cache";

            if (Directory.Exists(nativeWorkingDir))
            {
                foreach (var file in Directory.GetFiles(nativeWorkingDir))
                    File.Delete(file);

                Assert.IsTrue(Directory.GetFiles(nativeWorkingDir).Length == 0);
            }
        }
    }
}
