using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Arma3.Test.IO
{
    public class ProjectDriveTest
    {
        [Fact]
        public void GetFullPath()
        {
            if (OperatingSystem.IsWindows())
            {
                var drive = new ProjectDrive("c:\\temp\\pdrive");
                drive.AddMountPoint("m", "c:\\temp\\mount");
                Assert.Equal("c:\\temp\\pdrive\\a\\b\\c", drive.GetFullPath("a\\b\\c"));
                Assert.Equal("c:\\temp\\pdrive\\a", drive.GetFullPath("a"));
                Assert.Equal("c:\\temp\\mount\\b\\c", drive.GetFullPath("m\\b\\c"));
                Assert.Equal("c:\\temp\\mount\\b", drive.GetFullPath("m\\b"));
            }
            else
            {
                var drive = new ProjectDrive("/tmp/pdrive");
                drive.AddMountPoint("m", "/tmp/mount");
                Assert.Equal("/tmp/pdrive/a/b/c", drive.GetFullPath("a\\b\\c"));
                Assert.Equal("/tmp/pdrive/a", drive.GetFullPath("a"));
                Assert.Equal("/tmp/mount/b/c", drive.GetFullPath("m\\b\\c"));
                Assert.Equal("/tmp/mount/b", drive.GetFullPath("m\\b"));
            }
        }
    }
}
