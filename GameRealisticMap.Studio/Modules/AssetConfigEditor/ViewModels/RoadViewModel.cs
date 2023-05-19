using System.Windows.Media;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class RoadViewModel : AssetBase<RoadTypeId, Arma3RoadTypeInfos>
    {
        private static readonly DefaultRoadTypeLibrary defaultRoadTypeLibrary = new DefaultRoadTypeLibrary();

        public RoadViewModel(RoadTypeId id, Arma3RoadTypeInfos? arma3RoadTypeInfos, BridgeDefinition? bridgeDefinition, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            var defaults = defaultRoadTypeLibrary.GetInfo(id);
            ClearWidth = arma3RoadTypeInfos?.ClearWidth ?? defaults.ClearWidth;
            Width = arma3RoadTypeInfos?.Width ?? defaults.Width;
            Texture = arma3RoadTypeInfos?.Texture ?? string.Empty;
            TextureEnd = arma3RoadTypeInfos?.TextureEnd ?? string.Empty;
            Material = arma3RoadTypeInfos?.Material ?? string.Empty;
            TextureWidth = arma3RoadTypeInfos?.TextureWidth ?? Width;
            if (arma3RoadTypeInfos != null)
            {
                SatelliteColor = Color.FromRgb(arma3RoadTypeInfos.SatelliteColor.R, arma3RoadTypeInfos.SatelliteColor.G, arma3RoadTypeInfos.SatelliteColor.B);
            }
            else
            {
                SatelliteColor = Colors.Gray;
            }
            // TODO : Bridge
        }

        public float ClearWidth { get; set; }

        public float Width { get; set; }

        public float TextureWidth { get; set; }

        public string Texture { get; set; }

        public string TextureEnd { get; set; }

        public string Material { get; set; }

        public Color SatelliteColor { get; set; }

        public override Arma3RoadTypeInfos ToDefinition()
        {
            return new Arma3RoadTypeInfos(FillId, new Rgb24(SatelliteColor.R, SatelliteColor.G, SatelliteColor.B), TextureWidth, Texture, TextureEnd, Material, Width, ClearWidth);
        }

        public BridgeDefinition? ToBridgeDefinition()
        {
            throw new System.NotImplementedException(); // TODO
        }
    }
}