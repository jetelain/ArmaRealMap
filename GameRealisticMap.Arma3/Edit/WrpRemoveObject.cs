using System.Numerics;
using System.Text.Json.Serialization;
using BIS.WRP;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpRemoveObject
    {
        public WrpRemoveObject(Matrix4x4 transform, string model, int objectId)
        {
            Transform = transform;
            WorldPos = new Vector3(transform.M41, transform.M42, transform.M43);
            Pos2D = new Vector2(transform.M41, transform.M43);
            Model = model;
            ModelName = Path.GetFileNameWithoutExtension(model);
            ObjectId = objectId;
        }

        public Matrix4x4 Transform { get; }

        [JsonIgnore]
        public Vector3 WorldPos { get; }

        [JsonIgnore]
        public Vector2 Pos2D { get; }

        public string Model { get; }

        [JsonIgnore]
        public string ModelName { get; }

        public int ObjectId { get; }

        internal bool MatchRelaxed(EditableWrpObject h)
        {
            if (ModelName.Contains(Path.GetFileNameWithoutExtension(h.Model), StringComparison.OrdinalIgnoreCase))
            {
                var pos = new Vector2(h.Transform.Matrix.M41, h.Transform.Matrix.M43);
                if ((pos - Pos2D).LengthSquared() < 0.02f)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Match(EditableWrpObject h)
        {
            if (string.Equals(h.Model, Model, StringComparison.OrdinalIgnoreCase))
            {
                var pos = new Vector3(h.Transform.Matrix.M41, h.Transform.Matrix.M42, h.Transform.Matrix.M43);
                if ((pos - WorldPos).LengthSquared() < 0.02f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
