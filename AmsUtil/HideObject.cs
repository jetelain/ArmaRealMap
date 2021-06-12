using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BIS.WRP;

namespace TerrainBuilderUtil
{
    internal class HideObject : IEquatable<HideObject>
    {
        public HideObject(Vector3 worldPos, string model) 
        {
            WorldPos = worldPos;
            Pos2D = new Vector2(WorldPos.X, WorldPos.Z);
            Model = model;
            ModelName = Path.GetFileNameWithoutExtension(model);
        }

        public Vector3 WorldPos { get; }
        public Vector2 Pos2D { get; }
        public string Model { get; }
        public string ModelName { get; }

        public bool HasOneMatch => Matches.Count > 0;

        public bool Equals(HideObject other)
        {
            return string.Equals(other.Model, Model, StringComparison.OrdinalIgnoreCase) && WorldPos.Equals(other.WorldPos);
        }

        public override int GetHashCode()
        {
            return WorldPos.GetHashCode();
        }

        public List<EditableWrpObject> Matches { get; } = new List<EditableWrpObject>();

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