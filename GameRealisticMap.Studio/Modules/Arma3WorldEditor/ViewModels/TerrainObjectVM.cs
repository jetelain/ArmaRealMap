using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Windows;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Modules.Inspector;
using Gemini.Modules.Inspector.Inspectors;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    public class TerrainObjectVM : PropertyChangedBase, ITerrainEnvelope, IEditablePointCollection, IInspectableObject
    {
        private bool isRemoved;
        private IEnumerable<IInspector>? inspectors;
        private EditableWrpObject obj;
        private IAssetCatalogItem modelInfo;
        private readonly Arma3WorldMapViewModel owner;

        private readonly List<TerrainPoint> corners = new List<TerrainPoint>();

        internal TerrainObjectVM(Arma3WorldMapViewModel owner, EditableWrpObject obj, IAssetCatalogItem modelInfo)
        {
            this.obj = obj;
            this.modelInfo = modelInfo;
            this.owner = owner;

            Radius = Math.Max(new Vector2(modelInfo.BboxMin.X, modelInfo.BboxMin.Z).Length(), new Vector2(modelInfo.BboxMax.X, modelInfo.BboxMax.Z).Length());

            Rectangle = new Rect(
                modelInfo.BboxMin.X,
                modelInfo.BboxMin.Z,
                modelInfo.BboxMax.X - modelInfo.BboxMin.X,
                modelInfo.BboxMax.Z - modelInfo.BboxMin.Z);
        }

        private void GenerateCorners(EditableWrpObject obj, IAssetCatalogItem modelInfo)
        {
            corners.Add(ToTerrainPoint(Vector3.Transform(new Vector3(modelInfo.BboxMin.X, 0, modelInfo.BboxMin.Z), obj.Transform.Matrix)));
            corners.Add(ToTerrainPoint(Vector3.Transform(new Vector3(modelInfo.BboxMin.X, 0, modelInfo.BboxMax.Z), obj.Transform.Matrix)));
            corners.Add(ToTerrainPoint(Vector3.Transform(new Vector3(modelInfo.BboxMax.X, 0, modelInfo.BboxMax.Z), obj.Transform.Matrix)));
            corners.Add(ToTerrainPoint(Vector3.Transform(new Vector3(modelInfo.BboxMax.X, 0, modelInfo.BboxMin.Z), obj.Transform.Matrix)));
        }

        private TerrainPoint ToTerrainPoint(Vector3 vector3)
        {
            return new TerrainPoint(vector3.X, vector3.Z);
        }

        internal EditableWrpObject WrpObject => obj;

        public Matrix4x4 Matrix => obj.Transform.Matrix;

        public TerrainPoint MinPoint => Center - new Vector2(Radius);

        public TerrainPoint MaxPoint => Center + new Vector2(Radius);

        public TerrainPoint Center => new TerrainPoint(obj.Transform.Matrix.M41, obj.Transform.Matrix.M43);

        public float Radius { get; set; }

        public AssetCatalogCategory Category => modelInfo.Category;

        public Rect Rectangle { get; set; }

        public string Model 
        { 
            get => obj.Model; 
            set { /* Not yet supported */ } 
        }

        public bool IsRemoved 
        {
            get { return isRemoved; } 
            set 
            {
                if (isRemoved != value)
                {
                    isRemoved = value;
                    NotifyOfPropertyChange();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    owner.MakeObjectsDirty();
                }
            }
        }

        public IEnumerable<IInspector> Inspectors 
        {
            get
            {
                if (inspectors == null)
                {
                    inspectors =
                        new InspectableObjectBuilder()
                            .WithEditor(this, r => r.Model, new TextBoxEditorViewModel<string>())
                            .ToInspectableObject()
                            .Inspectors;
                }
                return inspectors;
            }
         }

        public bool CanInsertAtEnds => false;

        public bool CanInsertBetween => false;

        public bool CanSplit => false;

        public int Count => isRemoved ? 0 : 4;

        public bool CanDeletePoint => false;

        public bool IsObjectSquare => true;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Add(TerrainPoint terrainPoint)
        {
            throw new InvalidOperationException();
        }

        public IEnumerator<TerrainPoint> GetEnumerator()
        {
            if (isRemoved)
            {
                return Enumerable.Empty<TerrainPoint>().GetEnumerator();
            }
            if (corners.Count == 0)
            {
                GenerateCorners(obj, modelInfo);
            }
            return corners.GetEnumerator();
        }

        public void Insert(int index, TerrainPoint terrainPoint)
        {
            throw new InvalidOperationException();
        }

        public void PreviewSet(int index, TerrainPoint value)
        {
        }

        public void Remove()
        {
            owner.UndoRedoManager.ExecuteAction(new RemoveObjectAction(owner, this));
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        public void Set(int index, TerrainPoint oldValue, TerrainPoint newValue)
        {
        }

        public void SplitAt(int index)
        {
            throw new InvalidOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void Update(IAssetCatalogItem modelInfo)
        {
            this.modelInfo = modelInfo;

            Radius = Math.Max(new Vector2(modelInfo.BboxMin.X, modelInfo.BboxMin.Z).Length(), new Vector2(modelInfo.BboxMax.X, modelInfo.BboxMax.Z).Length());

            Rectangle = new Rect(
                modelInfo.BboxMin.X,
                modelInfo.BboxMin.Z,
                modelInfo.BboxMax.X - modelInfo.BboxMin.X,
                modelInfo.BboxMax.Z - modelInfo.BboxMin.Z);

            corners.Clear();

            IsRemoved = false;
        }
    }
}
