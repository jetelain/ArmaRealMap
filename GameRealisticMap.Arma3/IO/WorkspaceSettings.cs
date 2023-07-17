using System.Runtime.Versioning;
using System.Text.Json;

namespace GameRealisticMap.Arma3.IO
{
    public class WorkspaceSettings
    {
        public static string DefaultLocation { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap", "Arma3", "workspace.json");

        public string? Arma3BasePath { get; set; }

        public string? ProjectDriveBasePath { get; set; }

        public List<string> ModsPaths { get; set; } = new List<string>();

        public Dictionary<string, string> ProjectDriveMountPoints { get; set; } = new Dictionary<string, string>();

        [SupportedOSPlatform("windows")]
        public ProjectDrive CreateProjectDrive()
        {
            var drive = new ProjectDrive(
                ProjectDriveBasePath ?? Arma3ToolsHelper.GetProjectDrivePath(), 
                new PboFileSystem(PboFileSystem.GetArma3Paths(Arma3BasePath ?? Arma3ToolsHelper.GetArma3Path()), ModsPaths));
            foreach(var pair in ProjectDriveMountPoints)
            {
                drive.AddMountPoint(pair.Key, pair.Value);
            }
            return drive;
        }

        public ProjectDrive CreateProjectDriveStandalone()
        {
            var drive = new ProjectDrive(ProjectDriveBasePath ?? Arma3ToolsHelper.GetProjectDrivePath());
            foreach (var pair in ProjectDriveMountPoints)
            {
                drive.AddMountPoint(pair.Key, pair.Value);
            }
            return drive;
        }

        public static async Task<WorkspaceSettings> Load()
        {
            var filename = DefaultLocation;
            if(File.Exists(filename))
            {
                return await LoadFrom(filename).ConfigureAwait(false);
            }
            return new WorkspaceSettings();
        }

        public static async Task<WorkspaceSettings> LoadFrom(string filename)
        {
            using var stream = File.OpenRead(filename);
            return await LoadFrom(stream).ConfigureAwait(false);
        }

        public static async Task<WorkspaceSettings> LoadFrom(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<WorkspaceSettings>(stream).ConfigureAwait(false) ?? new WorkspaceSettings();
        }

        public async Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DefaultLocation)!);
            await Save(DefaultLocation).ConfigureAwait(false);
        }

        public async Task Save(string filename)
        {
            using var stream = File.Create(filename);
            await JsonSerializer.SerializeAsync(stream, this, new JsonSerializerOptions() { WriteIndented = true }).ConfigureAwait(false);
        }

        public async Task Save(Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, this).ConfigureAwait(false);
        }

    }
}
