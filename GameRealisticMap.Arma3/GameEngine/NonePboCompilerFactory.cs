using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class NonePboCompilerFactory : IPboCompilerFactory
    {
        public IPboCompiler Create(IProgressScope task)
        {
            throw new NotSupportedException();
        }
    }
}
