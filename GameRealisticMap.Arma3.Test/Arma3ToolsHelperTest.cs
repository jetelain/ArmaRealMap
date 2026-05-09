using System.IO;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.IO;
using Xunit;

namespace GameRealisticMap.Arma3.Test
{
    public class Arma3ToolsHelperTest
    {
        [Fact]
        public void GetProjectDrivePath_Returns_Custom_If_Set_And_Exists()
        {
            // Arrange
            var customPath = Path.Combine(Path.GetTempPath(), "Arma3CustomProjectDrive");
            Directory.CreateDirectory(customPath);
            var settings = new WorkspaceSettings { ProjectDriveBasePath = customPath };

            // Act
            var result = Arma3ToolsHelper.GetProjectDrivePath(settings);

            // Assert
            Assert.Equal(customPath, result);
        }

        [Fact]
        public void GetProjectDrivePath_Returns_Default_If_Custom_Set_And_Not_Exists()
        {
            // Arrange
            var settings = new WorkspaceSettings { ProjectDriveBasePath = "NoSuchFileOrDirectory" };

            // Act
            var result = Arma3ToolsHelper.GetProjectDrivePath(settings);

            // Assert
            Assert.Contains("Arma 3 Projects", result);
        }

        [Fact]
        public void GetProjectDrivePath_Returns_Default_If_No_P_And_No_Custom()
        {
            // Arrange
            var settings = new WorkspaceSettings { ProjectDriveBasePath = null };

            // Act
            var result = Arma3ToolsHelper.GetProjectDrivePath(settings);

            // Assert
            Assert.Contains("Arma 3 Projects", result);
        }
    }
}
