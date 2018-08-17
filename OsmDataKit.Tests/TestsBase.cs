using Kit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OsmDataKit.Tests
{
    public abstract class TestsBase
    {
        protected const string ProjectNativePath = "../../..";

        [TestInitialize]
        public void BaseInitialize()
        {
            Kit.Kit.Setup(test: true, baseDirectory: ProjectNativePath);
            ConsoleClient.Setup(minLevel: LogLevel.Log);
        }

        public void TestInitialize(string testName)
        {
            var workingDir = "$work/" + testName;
            Kit.Kit.Setup(workingDirectory: workingDir);
            ConsoleClient.Setup(minLevel: LogLevel.Log);

            var nativeWorkingDir = ProjectNativePath + "/" + workingDir;

            if (Directory.Exists(nativeWorkingDir))
            {
                foreach (var file in Directory.GetFiles(nativeWorkingDir))
                    File.Delete(file);

                Assert.IsTrue(Directory.GetFiles(nativeWorkingDir).Length == 0);
            }
        }
    }
}
