using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArmaRealMap
{
    public class ModelInfoLibrary
    {
        private readonly List<ModelInfo> models = new List<ModelInfo>();

        internal ModelInfo ResolveByName(string name)
        {
            var model = models.FirstOrDefault(m => string.Equals(m.Name,name,StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                model = new ModelInfo() { Path = "?\\" + name + ".p3d", Name = name };
                models.Add(model);
            }
            return model;
        }

        internal ModelInfo ResolveByPath(string path)
        {
            var model = models.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if ( model == null)
            {
                model = new ModelInfo() { Path = path, Name = Path.GetFileNameWithoutExtension(path) };
                models.Add(model);
            }
            return model;
        }
    }
}
