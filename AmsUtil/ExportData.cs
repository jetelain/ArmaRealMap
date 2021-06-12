using System.Collections.Generic;
using BIS.WRP;

namespace TerrainBuilderUtil
{
    internal class ExportData
    {
        public ExportData()
        {
        }

        public List<EditableWrpObject> Add { get; } = new List<EditableWrpObject>();
        public List<HideObject> ToRemove { get; } = new List<HideObject>();
    }
}