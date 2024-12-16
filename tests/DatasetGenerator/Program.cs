using System.Runtime.CompilerServices;
using System.Text.Json;
using GameRealisticMap.Arma3;
using GameRealisticMap.Configuration;
using GameRealisticMap.Generic;
using GameRealisticMap.Generic.Profiles;
using GameRealisticMap.Osm;
using Pmad.Cartography;
using OsmSharp;
using OsmSharp.IO.PBF;
using OsmSharp.Streams;
using Pmad.ProgressTracking;
using ProtoBuf;
using ProtoBuf.Meta;

namespace DatasetGenerator
{
    internal class Program
    {
        private static string GetBasePath([CallerFilePath] string thisFilePath = "")
        {
            return Environment.GetEnvironmentVariable("GRM_DATASETS")
                ?? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFilePath)!, "..", "Datasets"));
        }

        static async Task Main(string[] args)
        {
            using var console = new ConsoleProgessRender();


            var path = GetBasePath();

            var files = Directory.GetFiles(path, "*.grma3m");
            foreach (var file in files)
            {
                var rawdata = Path.ChangeExtension(file, ".pbf.zst");
                if (!File.Exists(rawdata))
                {
                    using var scope = console.Root.CreateScope("Process " + Path.GetFileName(file));

                    var osmLoader = new OsmDataOverPassLoader(scope, new DefaultSourceLocations());

                    Console.WriteLine(file);

                    var a3config = await ReadA3Config(file);

                    var gconfig = new GenericMapConfigJson()
                    {
                        Center = a3config.Center,
                        GridCellSize = a3config.GridCellSize,
                        GridSize = a3config.GridSize,
                        PrivateServiceRoadThreshold = a3config.PrivateServiceRoadThreshold,
                        SouthWest = a3config.SouthWest,
                        Resolution = a3config.Resolution,
                        ExportProfileFile = ExportProfile.Default
                    };

                    using (var output = File.Create(Path.ChangeExtension(file, ".grmm")))
                    {
                        await JsonSerializer.SerializeAsync(output, gconfig);
                    }

                    var data = await osmLoader.Load(gconfig.ToMapConfig().TerrainArea);

                    CompressionHelper.Write(rawdata,
                        Compression.ZSTD,
                        stream => WritePbfMonoBlock(data, stream));

                    //CompressionHelper.Write(Path.ChangeExtension(file, ".xml.zst"),
                    //    Compression.ZSTD,
                    //    stream => Copy(data, new XmlOsmStreamTarget(stream)));
                }
            }
        }

        private static void Copy(IOsmDataSource data, OsmStreamTarget writer)
        {
            writer.Initialize();
            foreach (var item in data.All)
            {
                switch (item.Type)
                {
                    case OsmSharp.OsmGeoType.Way:
                        writer.AddWay(Filter((OsmSharp.Way)item));
                        break;
                    case OsmSharp.OsmGeoType.Node:
                        writer.AddNode(Filter((OsmSharp.Node)item));
                        break;
                    case OsmSharp.OsmGeoType.Relation:
                        writer.AddRelation(Filter((OsmSharp.Relation)item));
                        break;
                }
            }
            writer.Flush();
            writer.Close();
        }


        private static void WritePbfMonoBlock(IOsmDataSource data, Stream stream)
        {
            // PBFOsmStreamTarget has an issue with paging, custom code to generate a PBF file

            var runtimeTypeModel = TypeModel.Create();
            runtimeTypeModel.Add(typeof(BlobHeader), true);
            runtimeTypeModel.Add(typeof(Blob), true);
            runtimeTypeModel.Add(typeof(PrimitiveBlock), true);
            runtimeTypeModel.Add(typeof(HeaderBlock), true);

            var headerBlock = new HeaderBlock();
            headerBlock.required_features.Add("OsmSchema-V0.6");

            WriteObject(stream, runtimeTypeModel, Encoder.OSMHeader, headerBlock);

            var primitiveBlock = new PrimitiveBlock();
            primitiveBlock.Encode(new Dictionary<string, int>(), data.All.ToList(), false);

            WriteObject(stream, runtimeTypeModel, Encoder.OSMData, primitiveBlock);
        }

        private static void WriteObject(Stream stream, RuntimeTypeModel runtimeTypeModel, string type, object obj)
        {
            var objBuffer = new MemoryStream();
            runtimeTypeModel.Serialize(objBuffer, obj);
            WriteBlob(stream, runtimeTypeModel, type, objBuffer);
        }

        private static void WriteBlob(Stream stream, RuntimeTypeModel runtimeTypeModel, string type, MemoryStream source)
        {
            var blob = new Blob();
            blob.raw = source.ToArray();
            blob.raw_size = blob.raw.Length;

            var buffer = new MemoryStream();
            runtimeTypeModel.Serialize(buffer, blob);

            var blobHeader = new BlobHeader();
            blobHeader.datasize = (int)buffer.Length;
            blobHeader.indexdata = null;
            blobHeader.type = type;
            runtimeTypeModel.SerializeWithLengthPrefix(stream, blobHeader, typeof(BlobHeader), PrefixStyle.Fixed32BigEndian, 0);

            buffer.Position = 0;
            buffer.CopyTo(stream);
        }

        private static T Filter<T>(T item) where T : OsmGeo
        {
            if (item.Tags != null)
            {
                item.Tags.RemoveKey("source");
                if (item.Tags.Count == 0)
                {
                    item.Tags = null;
                }
            }
            return item;
        }

        private static async Task<Arma3MapConfigJson> ReadA3Config(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                return await JsonSerializer.DeserializeAsync<Arma3MapConfigJson>(stream) ?? new Arma3MapConfigJson();
            }
        }
    }
}
