namespace GameRealisticMap.Arma3.Edit
{
    public class WrpMassReduce
    {
        public WrpMassReduce(string model, double removeRatio)
        {
            RemoveRatio = removeRatio;
            Model = model;
        }

        public double RemoveRatio { get; set; }

        public string Model { get; set; }
    }
}