namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditBatch
    {
        public List<WrpAddObject> Add { get; } = new List<WrpAddObject>();

        public List<WrpRemoveObject> Remove { get; } = new List<WrpRemoveObject>();

        public List<WrpSetElevationGrid> Elevation { get; } = new List<WrpSetElevationGrid>();

        public bool ElevationAdjustObjects { get; set; }

        public string WorldName { get; set; } = string.Empty;

        public float WorldSize { get; set; }

        public int Revision { get; set; }

        public int? PartCount { get; set; }

        public List<int> PartIndexes { get; } = new List<int>();

        public int? PartIndex => PartIndexes.Count == 0 ? null : PartIndexes.Max();

        public bool IsComplete => PartCount == null || PartCount.Value == PartIndexes.Count;
    }
}
