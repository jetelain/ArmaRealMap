namespace GameRealisticMap.Reporting
{
    public interface IProgressSystem
    {
        IProgressInteger CreateStep(string name, int total);
    }
}
