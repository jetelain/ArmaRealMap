namespace GameRealisticMap.IO
{
    public interface IPackageWriter
    {
        Stream CreateFile(string filename);
    }
}
