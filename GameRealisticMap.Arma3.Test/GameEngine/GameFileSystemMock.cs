using System.Reflection;
using System.Text;
using GameRealisticMap.Arma3.IO;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    internal class GameFileSystemMock : IGameFileSystemWriter
    {
        public Dictionary<string, string> TextFiles { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MemoryStream> BinaryFiles { get; } = new Dictionary<string, MemoryStream>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> FileNames => TextFiles.Keys.OrderBy(k => k);

        public Stream Create(string path)
        {
            CheckDirectoy(path);
            var stream = new MemoryStream();
            BinaryFiles[path] = stream;
            TextFiles.Remove(path);
            return stream;
        }

        public void CreateDirectory(string path)
        {
            string? p = path;
            while(!string.IsNullOrEmpty(p))
            {
                Directories.Add(p);
                p = Path.GetDirectoryName(p);
            }
        }

        public bool FileExists(string path)
        {
            return TextFiles.ContainsKey(path) || BinaryFiles.ContainsKey(path);
        }

        public IEnumerable<string> FindAll(string pattern)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public Stream? OpenFileIfExists(string path)
        {
            if (TextFiles.TryGetValue(path, out var result))
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(result));
            }
            if (BinaryFiles.TryGetValue(path, out var stream))
            {
                return new MemoryStream(stream.ToArray());
            }
            return null;
        }

        public void WritePngImage(string path, Image image)
        {
            CheckDirectoy(path);
            image.SaveAsPng(Create(path));
        }

        public void WriteTextFile(string path, string text)
        {
            CheckDirectoy(path);
            TextFiles[path] = text;
            BinaryFiles.Remove(path);
        }

        private void CheckDirectoy(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directories.Contains(dir))
            {
                throw new IOException($"Path '{dir}' does not yet exists.");
            }
        }

        internal void AssertFiles<TestClass>(string testcase)
        {
            var assembly = typeof(TestClass).Assembly;
            var prefix = typeof(TestClass).FullName + "." + testcase + ".";

            Assert.Equal(ReadText(assembly, prefix, "index.txt"), string.Join("\r\n", FileNames), false, true);

            foreach (var pair in TextFiles)
            {
                Assert.Equal(ReadText(assembly, prefix, pair.Key), pair.Value, false, true);
            }
        }

        private static string? ReadText(Assembly assembly, string prefix, string key)
        {
            using (var stream = assembly.GetManifestResourceStream(prefix + key.Replace('\\', '.').Replace('/', '.')))
            {
                if (stream == null)
                {
                    return null;
                }
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public string? GetLocationInfoForError(string path)
        {
            return path;
        }
    }
}