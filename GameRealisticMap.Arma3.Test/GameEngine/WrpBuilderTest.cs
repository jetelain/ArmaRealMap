using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class WrpCompilerTest
    {
        [Fact]
        public void Materials_Gossi()
        {
            var builder = new WrpCompiler(new NoProgressSystem(), new GameFileSystemMock());
            var wrp = WrpCompiler.InitWrp(Arma3MapConfigMock.Gossi, 512);
            builder.SetMaterialAndIndexes(new ImageryTiler(Arma3MapConfigMock.Gossi), wrp, "prefix");
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers1.txt").ToList());
        }

        [Fact]
        public void Materials_Taunus()
        {
            var builder = new WrpCompiler(new NoProgressSystem(), new GameFileSystemMock());
            var wrp = WrpCompiler.InitWrp(Arma3MapConfigMock.Taunus, 512);
            builder.SetMaterialAndIndexes(new ImageryTiler(Arma3MapConfigMock.Taunus), wrp, "prefix");
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers2.txt").ToList());
        }

        [Fact]
        public void Materials_Belfort()
        {
            var builder = new WrpCompiler(new NoProgressSystem(), new GameFileSystemMock());
            var wrp = WrpCompiler.InitWrp(Arma3MapConfigMock.Belfort, 512);
            builder.SetMaterialAndIndexes(new ImageryTiler(Arma3MapConfigMock.Belfort), wrp, "prefix");
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers2.txt").ToList());
        }

        private IEnumerable<string> Read(string v)
        {
            using (var reader = new StreamReader(typeof(WrpCompilerTest).Assembly.GetManifestResourceStream("GameRealisticMap.Arma3.Test.GameEngine." + v)!))
            {
                string? l;
                while( (l = reader.ReadLine()) != null )
                {
                    yield return l;
                }
            }
        }
    }
}
