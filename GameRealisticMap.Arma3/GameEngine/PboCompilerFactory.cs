using System.Runtime.Versioning;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    public sealed class PboCompilerFactory : IPboCompilerFactory
    {
        private readonly ModelInfoLibrary modelInfoLibrary;
        private readonly ProjectDrive projectDrive;

        public PboCompilerFactory(ModelInfoLibrary modelInfoLibrary, ProjectDrive projectDrive)
        {
            this.modelInfoLibrary = modelInfoLibrary;
            this.projectDrive = projectDrive;
        }

        public PboCompilerFactory(ProjectDrive projectDrive) 
            : this(new ModelInfoLibrary(projectDrive), projectDrive)
        {
        }

        [SupportedOSPlatform("windows")]
        public IPboCompiler Create(IProgressScope task)
        {
            return new PboCompiler(task, projectDrive, modelInfoLibrary);
        }
    }
}
