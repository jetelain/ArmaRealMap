using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArmaRealMap.Configuration;
using ICSharpCode.SharpZipLib.Zip;

namespace ArmaRealMap
{
    internal class PackageHelper
    {
        internal static MapConfig Unpack(CookOptions arg)
        {
            MapConfig config;
            var buffer = new byte[4096];
            var done = 0;
            using (var input = File.OpenRead(arg.Pack))
            using (var zipFile = new ZipFile(input))
            {
                var entries = zipFile.Cast<ZipEntry>().ToList();

                var json = entries.FirstOrDefault(z => z.Name == "arm.json");
                using (var zipStream = zipFile.GetInputStream(json))
                {
                    config = JsonSerializer.Deserialize<MapConfig>(zipStream, ConfigLoader.SerializerOptions);
                }
                
                var tracker = new ProgressReport("Unpack", (int)entries.Sum(f => f.Size));

                var target = Path.Combine("P:", config.PboPrefix);

                foreach (var entry in entries)
                {
                    if (entry.IsFile && entry.Name != "arm.json")
                    {
                        var fullZipToPath = Path.Combine(target, entry.Name.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(fullZipToPath));
                        using (var zipStream = zipFile.GetInputStream(entry))
                        {
                            using (Stream output = File.Create(fullZipToPath))
                            {
                                done = CopyStreamWithReport(zipStream, output, buffer, done, tracker);
                            }
                        }
                    }
                }
                tracker.TaskDone();
            }

            return config;
        }

        internal static void Pack(GenerateOptions options, MapConfig config)
        {
            Console.WriteLine("==== Packing ====");
            using (FileStream fsOut = File.Create(options.Pack))
            using (var zipStream = new ZipOutputStream(fsOut))
            {
                AddFile(zipStream, "arm.json", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config.ToCook(), ConfigLoader.SerializerOptions)));
                AddDirectory("Layers", zipStream, Path.Combine(config.Target.Cooked, "data", "layers"), "data/layers/");
                AddDirectory("Roads", zipStream, Path.Combine(config.Target.Cooked, "data", "roads"), "data/roads/");
                AddDirectory("World", zipStream, config.Target.Cooked, "");
            }
        }

        private static void AddFile(ZipOutputStream zipStream, string name, byte[] content)
        {
            var entry = new ZipEntry(ZipEntry.CleanName(name));
            entry.DateTime = DateTime.Now;
            entry.Size = content.Length;
            zipStream.PutNextEntry(entry);
            zipStream.Write(content, 0, content.Length);
            zipStream.CloseEntry();
        }

        private static void AddDirectory(string step, ZipOutputStream zipStream, string physical, string target)
        {
            var filenames = Directory.GetFiles(physical).Where(f => Path.GetExtension(f) != ".paa").Select(f => new FileInfo(f)).ToList();

            var buffer = new byte[4096];
            var done = 0;

            var tracker = new ProgressReport(step, (int)filenames.Sum(f => f.Length));
            foreach (var fi in filenames)
            {
                var entry = new ZipEntry(ZipEntry.CleanName(target + Path.GetFileName(fi.FullName)));
                entry.DateTime = fi.LastWriteTime;
                entry.Size = fi.Length;
                zipStream.PutNextEntry(entry);
                using (Stream inputStream = File.OpenRead(fi.FullName))
                {
                    done = CopyStreamWithReport(inputStream, zipStream, buffer, done, tracker);
                }
                zipStream.CloseEntry();
            }
            tracker.TaskDone();
        }

        private static int CopyStreamWithReport(Stream source, Stream target, byte[] buffer, int done, ProgressReport tracker)
        {
            int numRead;
            while ((numRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                done += numRead;
                target.Write(buffer, 0, numRead);
                tracker.ReportItemsDone(done);
            }
            return done;
        }


    }
}
