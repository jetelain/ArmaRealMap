using GameRealisticMap.Studio.Modules.Main.Commands;

namespace GameRealisticMap.Studio.Test.Modules.Main.Commands
{
    public class RecentFilesCommandHandlerTest
    {

        [Fact]
        public void GetDisplayPath()
        {
            Assert.Equal(@"c:\hello.txt", RecentFilesCommandHandler.GetDisplayPath(@"c:\hello.txt"));
            Assert.Equal(@"c:\...\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt", RecentFilesCommandHandler.GetDisplayPath(@"c:\a\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt"));
            Assert.Equal(@"c:\...\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt", RecentFilesCommandHandler.GetDisplayPath(@"c:\thiscanbeverylongtooifyouaddalotofuselesswords\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt"));
            Assert.Equal(@"c:\...hich\name\will\overflow\the\limit\of\sixty\four\characters.txt", RecentFilesCommandHandler.GetDisplayPath(@"c:\this\is\a\very\long\filename\which\name\will\overflow\the\limit\of\sixty\four\characters.txt"));
            Assert.Equal("", RecentFilesCommandHandler.GetDisplayPath(@""));
            Assert.Equal(@"...\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt", RecentFilesCommandHandler.GetDisplayPath(@"thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters.txt"));
            Assert.Equal(@"c:\...ongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters\", RecentFilesCommandHandler.GetDisplayPath(@"c:\thisisaverylongfilenamewhichnamewilloverflowthelimitofsixtyfourcharacters\"));
        }
    }
}
