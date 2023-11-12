using System;
using System.Numerics;
using System.Windows;
using BIS.P3D;
using BIS.WRP;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    public class TerrainObjectVM : ITerrainEnvelope
    {
        private readonly EditableWrpObject obj;
        private readonly IModelInfo modelInfo;

        public TerrainObjectVM(EditableWrpObject obj, IModelInfo modelInfo)
        {
            this.obj = obj;
            this.modelInfo = modelInfo;

            Radius = Math.Max(new Vector2(modelInfo.BboxMin.X, modelInfo.BboxMin.Z).Length(), new Vector2(modelInfo.BboxMax.X, modelInfo.BboxMax.Z).Length());

            Category = AssetsCatalogService.DetectModel(modelInfo, obj.Model); // TODO: get info from asset manager

            Rectangle = new Rect(
                modelInfo.BboxMin.X, 
                modelInfo.BboxMin.Z,
                modelInfo.BboxMax.X - modelInfo.BboxMin.X,
                modelInfo.BboxMax.Z - modelInfo.BboxMin.Z); 
        }

        public IModelInfo ModelInfo => modelInfo;

        public Matrix4x4 Matrix => obj.Transform.Matrix;

        public TerrainPoint MinPoint => Center - new Vector2(Radius);

        public TerrainPoint MaxPoint => Center + new Vector2(Radius);

        public TerrainPoint Center => new TerrainPoint(obj.Transform.Matrix.M41, obj.Transform.Matrix.M43);

        public float Radius { get; }

        public AssetCatalogCategory Category { get; }

        public Rect Rectangle { get; }
    }
}
