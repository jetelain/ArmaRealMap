using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using Gemini.Framework;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class RoadViewModel : AssetIdBase<RoadTypeId, Arma3RoadTypeInfos>
    {
        private static readonly DefaultRoadTypeLibrary defaultRoadTypeLibrary = new DefaultRoadTypeLibrary();
        private float clearWidth;
        private float width;
        private float textureWidth;
        private string texture;
        private string textureEnd;
        private string material;
        private Color satelliteColor;

        public RoadViewModel(RoadTypeId id, Arma3RoadTypeInfos? arma3RoadTypeInfos, BridgeDefinition? bridgeDefinition, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            var defaults = defaultRoadTypeLibrary.GetInfo(id);
            clearWidth = arma3RoadTypeInfos?.ClearWidth ?? defaults.ClearWidth;
            width = arma3RoadTypeInfos?.Width ?? defaults.Width;
            texture = arma3RoadTypeInfos?.Texture ?? string.Empty;
            textureEnd = arma3RoadTypeInfos?.TextureEnd ?? string.Empty;
            material = arma3RoadTypeInfos?.Material ?? string.Empty;
            textureWidth = arma3RoadTypeInfos?.TextureWidth ?? Width;
            if (arma3RoadTypeInfos != null)
            {
                satelliteColor = Color.FromRgb(arma3RoadTypeInfos.SatelliteColor.R, arma3RoadTypeInfos.SatelliteColor.G, arma3RoadTypeInfos.SatelliteColor.B);
            }
            else
            {
                satelliteColor = Colors.Gray;
            }
            Items = new List<BridgeViewModel>()
            {
                new BridgeViewModel(Labels.BridgeSingle, bridgeDefinition?.Single),
                new BridgeViewModel(Labels.BridgeStart, bridgeDefinition?.Start),
                new BridgeViewModel(Labels.BridgeMiddle, bridgeDefinition?.Middle),
                new BridgeViewModel(Labels.BridgeEnd, bridgeDefinition?.End)
            };
            ClearItem = new RelayCommand(item => ((BridgeViewModel)item).Clear());
        }

        public override string Icon => "pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        public float ClearWidth { get => clearWidth; set { clearWidth = value; NotifyOfPropertyChange(); } }

        public float Width { get => width; set { width = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(LaneOffset)); } }

        public float TextureWidth { get => textureWidth; set { textureWidth = value; NotifyOfPropertyChange(); } }

        public float LaneOffset
        {
            get
            {
                if (IsTwoLanes)
                {
                    return Width / 2;
                }
                return 0;
            }
        }

        public bool IsTwoLanes
        {
            get
            {
                switch (FillId)
                {
                    case RoadTypeId.TwoLanesMotorway:
                    case RoadTypeId.TwoLanesPrimaryRoad:
                    case RoadTypeId.TwoLanesSecondaryRoad:
                    case RoadTypeId.TwoLanesConcreteRoad:
                        return true;
                }
                return false;
            } 
        }

        public bool IsOneLane => !IsPedestrian;

        public bool IsPedestrian
        {
            get
            {
                switch (FillId)
                {
                    case RoadTypeId.ConcreteFootway:
                    case RoadTypeId.Trail:
                        return true;
                }
                return false;
            }
        }


        public string Texture { get => texture; set { texture = value; NotifyOfPropertyChange(); } }

        public string TextureEnd { get => textureEnd; set { textureEnd = value; NotifyOfPropertyChange(); } }

        public string Material { get => material; set { material = value; NotifyOfPropertyChange(); } }

        public Color SatelliteColor { get => satelliteColor; set { satelliteColor = value; NotifyOfPropertyChange(); } }

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
        public override void AddComposition(Composition model, ObjectPlacementDetectedInfos detected)
        {
            var item = Items.FirstOrDefault(i => i.Composition.IsEmpty);
            if (item != null)
            {
                item.AddComposition(model, detected);
            }
        }
        public override IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path));
        }
    }
}