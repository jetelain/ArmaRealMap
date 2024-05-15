using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Algorithms.Randomizations;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class RandomizationOperationVM : PropertyChangedBase
    {
        private static readonly RandomizationOperation[] TypesWithCenterPoint = new[] { RandomizationOperation.RotateX, RandomizationOperation.RotateY, RandomizationOperation.RotateZ }; 

        private float _x;
        private float _y;
        private float _z;
        private float _min;
        private float _max;
        private readonly CompositionItem parent;
        private readonly RandomizationOperationJson initial;

        public RandomizationOperationVM(RandomizationOperationJson initial, CompositionItem parent)
        {
            this.parent = parent;
            this.initial = initial;
            _min = initial.Min;
            _max = initial.Max;
            HasCenterPoint = TypesWithCenterPoint.Contains(initial.Type);
            if (HasCenterPoint)
            {
                _x = initial.CenterPoint?.X ?? parent.X;
                _y = initial.CenterPoint?.Z ?? parent.Y;
                _z = initial.CenterPoint?.Y ?? parent.Z;
            }
        }

        public RandomizationOperation Type => initial.Type;

        public string TypeLabel =>  Labels.ResourceManager.GetString("Randomization" + initial.Type) ?? initial.Type.ToString();

        public bool HasCenterPoint { get; }

        public float Min { get { return _min; } set { Set(ref _min, value); } }

        public float Max { get { return _max; } set { Set(ref _max, value); } }

        public float X { get { return _x; } set { Set(ref _x, value); } }

        public float Y { get { return _y; } set { Set(ref _y, value); } }

        public float Z { get { return _z; } set { Set(ref _z, value); } }

        public bool WasEdited => (HasCenterPoint ? (_x != initial.CenterPoint?.X
            || _y != initial.CenterPoint?.Z
            || _z != initial.CenterPoint?.Y) : false)
            || _min != initial.Min
            || _max != initial.Max;

        internal static IEnumerable<RandomizationOperationVM> Create(IEnumerable<IRandomizationOperation>? operations, CompositionItem parent)
        {
            if (operations != null)
            {
                return operations.Select(op => new RandomizationOperationVM(RandomizationOperationJson.From(op), parent));
            }
            return Enumerable.Empty<RandomizationOperationVM>();
        }

        internal IRandomizationOperation ToDefinition()
        {
            if (Type == RandomizationOperation.ScaleUniform)
            {
                // Force scale on model center to avoid distorded results
                X = parent.X;
                Y = parent.Y;
                Z = parent.Z;
            }
            return new RandomizationOperationJson(initial.Type, _min, _max, new System.Numerics.Vector3(_x, _z, _y)).ToRandomizationOperation();
        }

        public Task Remove()
        {
            parent.Randomizations.Remove(this);
            return Task.CompletedTask;
        }
    }
}