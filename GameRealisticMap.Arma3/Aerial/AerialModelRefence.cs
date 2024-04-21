using System.Numerics;
using BIS.P3D.ODOL;

namespace GameRealisticMap.Arma3.Aerial
{
    public class AerialModelRefence
    {
        public AerialModelRefence(string path, Vector3 boundingCenter, Vector3 bboxMin, Vector3 bboxMax)
        {
            Path = path;
            BoundingCenter = boundingCenter;
            BboxMin = bboxMin;
            BboxMax = bboxMax;
        }

        public string Path { get; }

        public Vector3 BoundingCenter { get; }

        public Vector3 BboxMin { get; }

        public Vector3 BboxMax { get; }

        public static AerialModelRefence FromODOL(string path, ModelInfo modelInfo)
        {
            return new AerialModelRefence(path, modelInfo.BoundingCenter.Vector3, modelInfo.BboxMin.Vector3, modelInfo.BboxMax.Vector3);
        }
    }
}
