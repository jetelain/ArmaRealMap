using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class NonePboCompilerFactory : IPboCompilerFactory
    {
        public IPboCompiler Create(IProgressTask task)
        {
            throw new NotSupportedException();
        }
    }
}
