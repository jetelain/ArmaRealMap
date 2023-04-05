namespace GameRealisticMap.Reporting
{
    public interface IProgressSystem
    {
        IDisposable CreateScope(string name);

        IProgressInteger CreateStep(string name, int total);
    }
}
