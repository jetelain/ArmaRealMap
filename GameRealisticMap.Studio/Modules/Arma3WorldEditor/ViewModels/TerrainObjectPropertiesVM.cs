using System.ComponentModel;
using BIS.WRP;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class TerrainObjectPropertiesVM
    {
        private readonly TerrainBuilderObject tbObj;

        public TerrainObjectPropertiesVM(EditableWrpObject obj, IModelInfoLibrary library)
        {
            tbObj = new TerrainBuilderObject(obj, library);
        }

        public string Name
        {
            get { return tbObj.Model.Name; }
        }

        public float X
        {
            get { return tbObj.Point.X; }
        }

        public float Y
        {
            get { return tbObj.Point.Y; }
        }

        [DisplayName("Z (absolute)")]
        public float Z
        {
            get { return tbObj.Elevation; }
        }


        public float Pitch
        {
            get { return tbObj.Pitch; }
        }
        public float Yaw
        {
            get { return tbObj.Yaw; }
        }

        public float Roll
        {
            get { return tbObj.Roll; }
        }

        public float Scale
        {
            get { return tbObj.Scale; }
        }
    }
}