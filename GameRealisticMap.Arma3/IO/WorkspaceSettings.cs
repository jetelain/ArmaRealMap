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

        public bool UsePboProject { get; set; }

        [SupportedOSPlatform("windows")]
        public ProjectDrive CreateProjectDrive()
        {
            var drive = new ProjectDrive(
                Arma3ToolsHelper.GetProjectDrivePath(this), 
                new PboFileSystem(PboFileSystem.GetArma3Paths(Arma3BasePath ?? Arma3ToolsHelper.GetArma3Path()), ModsPaths));
            foreach(var pair in ProjectDriveMountPoints)
            {
                drive.AddMountPoint(pair.Key, pair.Value);
            }
            return drive;
        }

        public ProjectDrive CreateProjectDriveStandalone()
        {
            var drive = new ProjectDrive(Arma3ToolsHelper.GetProjectDrivePath(this));
            foreach (var pair in ProjectDriveMountPoints)
            {
                drive.AddMountPoint(pair.Key, pair.Value);
            }
            return drive;
        }

        public ProjectDrive CreateProjectDriveAutomatic()
        {
            if (OperatingSystem.IsWindows())
            {
                return CreateProjectDrive();
            }
            return CreateProjectDriveStandalone();
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

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DefaultLocation)!);
            Save(DefaultLocation);
        }

        public void Save(string filename)
        {
            using var stream = File.Create(filename);
            JsonSerializer.Serialize(stream, this, new JsonSerializerOptions() { WriteIndented = true });
        }

    }
}
