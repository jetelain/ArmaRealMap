namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class ObjectStatsItem
    {
        public ObjectStatsItem(string model, int count)
        {
            Model = model;
            Count = count;
        }

        public string Model { get; }

        public int Count { get; }

        public string Label => $"{Model} ({Count})";
    }
}