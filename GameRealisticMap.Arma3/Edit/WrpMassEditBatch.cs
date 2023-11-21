namespace GameRealisticMap.Arma3.Edit
{
    public class WrpMassEditBatch
    {
        public List<WrpMassReduce> Reduce { get; } = new List<WrpMassReduce>();

        public List<WrpMassReplace> Replace { get; } = new List<WrpMassReplace>();
    }
}