using System;
using System.Numerics;
using System.Windows;
using BIS.WRP;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    public class TerrainObjectVM : ITerrainEnvelope
    {
        private readonly EditableWrpObject obj;
        private readonly IAssetCatalogItem modelInfo;

        public TerrainObjectVM(EditableWrpObject obj, IAssetCatalogItem modelInfo)
        {
            this.obj = obj;
            this.modelInfo = modelInfo;

            Radius = Math.Max(new Vector2(modelInfo.BboxMin.X, modelInfo.BboxMin.Z).Length(), new Vector2(modelInfo.BboxMax.X, modelInfo.BboxMax.Z).Length());

            Rectangle = new Rect(
                modelInfo.BboxMin.X, 
                modelInfo.BboxMin.Z,
                modelInfo.BboxMax.X - modelInfo.BboxMin.X,
                modelInfo.BboxMax.Z - modelInfo.BboxMin.Z); 
        }

        public Matrix4x4 Matrix => obj.Transform.Matrix;

        public TerrainPoint MinPoint => Center - new Vector2(Radius);

        public TerrainPoint MaxPoint => Center + new Vector2(Radius);

        public TerrainPoint Center => new TerrainPoint(obj.Transform.Matrix.M41, obj.Transform.Matrix.M43);

        public float Radius { get; }

        public AssetCatalogCategory Category => modelInfo.Category;

        public Rect Rectangle { get; }

        public string Model => obj.Model;
    }
}
