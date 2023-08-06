namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditBatch
    {
        public List<WrpAddObject> Add { get; } = new List<WrpAddObject>();

        public List<WrpRemoveObject> Remove { get; } = new List<WrpRemoveObject>();

        public string WorldName { get; internal set; } = string.Empty;

        public float WorldSize { get; internal set; }
        public int Revision { get; internal set; }
    }
}
