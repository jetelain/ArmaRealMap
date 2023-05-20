using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using Gemini.Framework;
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
            Items = new List<BridgeViewModel>()
            {
                new BridgeViewModel("Single", bridgeDefinition?.Single),
                new BridgeViewModel("Start", bridgeDefinition?.Start),
                new BridgeViewModel("Middle", bridgeDefinition?.Middle),
                new BridgeViewModel("End", bridgeDefinition?.End)
            }; 
            ClearItem = new RelayCommand(item => ((BridgeViewModel)item).Clear());
        }

        public float ClearWidth { get; set; }

        public float Width { get; set; }

        public float TextureWidth { get; set; }

        public string Texture { get; set; }

        public string TextureEnd { get; set; }

        public string Material { get; set; }

        public Color SatelliteColor { get; set; }

        public List<BridgeViewModel> Items { get; }
        public RelayCommand ClearItem { get; }

        public override Arma3RoadTypeInfos ToDefinition()
        {
            return new Arma3RoadTypeInfos(FillId, new Rgb24(SatelliteColor.R, SatelliteColor.G, SatelliteColor.B), TextureWidth, Texture, TextureEnd, Material, Width, ClearWidth);
        }

        public BridgeDefinition? ToBridgeDefinition()
        {
            if (Items.Any(i => !i.Composition.IsEmpty))
            {
                return new BridgeDefinition(
                    Items[0].ToDefinition(),
                    Items[1].ToDefinition(),
                    Items[2].ToDefinition(),
                    Items[3].ToDefinition());
            }
            return null;
        }

        public override void Equilibrate()
        {

        }
    }
}