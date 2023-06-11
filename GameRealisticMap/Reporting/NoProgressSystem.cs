namespace GameRealisticMap.Reporting
{
    public class NoProgressSystem : ProgressSystemBase
    {
        public override IProgressInteger CreateStep(string name, int total)
        {
            return new NoProgress();
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            return new NoProgress();
        }
    }
}
