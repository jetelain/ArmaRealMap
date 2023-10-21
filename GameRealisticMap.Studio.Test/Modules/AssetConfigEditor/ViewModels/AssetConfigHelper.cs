using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Test.Mocks;

namespace GameRealisticMap.Studio.Test.Modules.AssetConfigEditor.ViewModels
{
    internal static class AssetConfigHelper
    {
        internal static AssetConfigEditorViewModel CreateEditor()
        {
            return new AssetConfigEditorViewModel(new Arma3DataModuleMock(), new ShellMock(), new CompositionToolMock());
        }

        internal static void AddSomeObject(IModelImporterTarget target)
        {
            target.AddComposition(
                new Composition(new CompositionObject(new ModelInfo("SomeModel", "mod\\SomeModel.p3d", Vector3.Zero), Matrix4x4.Identity)), 
                new ObjectPlacementDetectedInfos(
                    new RadiusBasedPlacement(new Vector2(0.5f), 1),
                    new RadiusBasedPlacement(new Vector2(0.5f), 2),
                    new RectangleBasedPlacement(new Vector2(0.5f), new Vector2(1.5f)),
                    new RectangleBasedPlacement(new Vector2(0.5f), new Vector2(2.5f))));
        }
    }
}
