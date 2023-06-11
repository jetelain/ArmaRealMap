using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameRealisticMap.Studio
{
    public sealed class StudioAppBootstrapper : Gemini.AppBootstrapper
    {
        public override bool IsPublishSingleFileHandled => true;

        protected override IEnumerable<Assembly> PublishSingleFileBypassAssemblies
        {
            get
            {
                yield return typeof(StudioAppBootstrapper).Assembly;
                yield return typeof(Gemini.AppBootstrapper).Assembly;
                yield return typeof(Gemini.Modules.Output.IOutput).Assembly;
            }
        }

        protected override void PopulateAssemblySource()
        {

        }

        protected override bool CheckIfGeminiAppearsPublishedToSingleFile()
        {
            return true;
        }
    }
}
