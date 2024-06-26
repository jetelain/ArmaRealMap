﻿using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public sealed class RadiusPlacedLayer<TModelInfo> : SimpleSpacialIndex<RadiusPlacedModel<TModelInfo>>
    {
        public RadiusPlacedLayer(ITerrainArea area)
            : this(new Vector2(area.SizeInMeters))
        {

        }

        public RadiusPlacedLayer(Vector2 size)
            : base(Vector2.Zero, size)
        {

        }

        public void Insert(RadiusPlacedModel<TModelInfo> obj)
        {
            Insert(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public void Remove(RadiusPlacedModel<TModelInfo> obj)
        {
            Remove(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public List<RadiusPlacedModel<TModelInfo>> Search(TerrainPoint start, TerrainPoint end)
        {
            return Search(start.Vector, end.Vector);
        }

        public List<RadiusPlacedModel<TModelInfo>> Search(ITerrainEnvelope area)
        {
            return Search(area.MinPoint.Vector, area.MaxPoint.Vector);
        }

        public bool TryLock(ITerrainEnvelope area, out IDisposable? locker)
        {
            return TryLock(area.MinPoint.Vector, area.MaxPoint.Vector, out locker);
        }
    }
}
