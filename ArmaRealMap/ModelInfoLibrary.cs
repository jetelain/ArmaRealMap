using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.P3D.ODOL;

namespace ArmaRealMap
{
    public class ModelInfoLibrary
    {
        public List<ModelInfo> Models { get; } = new List<ModelInfo>();

        internal ModelInfo ResolveByName(string name)
        {
            var model = Models.FirstOrDefault(m => string.Equals(m.Name,name,StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                model = new ModelInfo() { Path = "?\\" + name + ".p3d", Name = name };
                Models.Add(model);
            }
            return model;
        }

        internal ModelInfo ResolveByPath(string path)
        {
            var model = Models.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if ( model == null)
            {
                ODOL odol;
                if (P3D.IsODOL(Path.Combine("P:", path)))
                {
                    odol = StreamHelper.Read<ODOL>(Path.Combine("P:", path));
                }
                else
                {
                    odol = StreamHelper.Read<ODOL>(Path.Combine("P:\\temp",path));
                }

                model = new ModelInfo() { Path = path, Name = Path.GetFileNameWithoutExtension(path), BoundingCenter = odol.ModelInfo.BoundingCenter.Vector3 };
                Models.Add(model);
            }
            return model;
        }
    }
}
