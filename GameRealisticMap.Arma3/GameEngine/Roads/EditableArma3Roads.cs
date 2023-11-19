namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public sealed class EditableArma3Roads
    {
        public List<EditableArma3RoadTypeInfos> RoadTypeInfos { get; set; } = new List<EditableArma3RoadTypeInfos>();

        public List<EditableArma3Road> Roads { get; set; } = new List<EditableArma3Road>();
    }
}
