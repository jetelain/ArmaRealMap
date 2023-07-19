using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class ImageryTilerTest
    {
        [Fact]
        public void ImageryTiler_UVB()
        {
            var gossi = new ImageryTiler(Arma3MapConfigMock.Gossi);
            Assert.Equal(1024, gossi.TileSize);
            Assert.Equal(40960, gossi.FullImageSize.Width);
            Assert.Equal(2, gossi.Resolution);
            Assert.Equal(64, gossi.TileOverlap);

            Assert.Equal(0.03125, gossi.Segments[0, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[0, 0].VB);

            Assert.Equal(0.03125, gossi.Segments[0, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[0, 42].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[42, 0].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[42, 42].VB);

            var taunus = new ImageryTiler(Arma3MapConfigMock.Taunus);
            Assert.Equal(512, taunus.TileSize);
            Assert.Equal(10240, taunus.FullImageSize.Width);
            Assert.Equal(2, taunus.Resolution);
            Assert.Equal(32, taunus.TileOverlap);

            Assert.Equal(0.03125, taunus.Segments[0, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[0, 0].VB);

            Assert.Equal(0.03125, taunus.Segments[0, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[0, 21].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[21, 0].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[21, 21].VB);

            var belfort = new ImageryTiler(Arma3MapConfigMock.Belfort);
            Assert.Equal(1024, belfort.TileSize);
            Assert.Equal(20480, belfort.FullImageSize.Width);
            Assert.Equal(1, belfort.Resolution);
            Assert.Equal(64, belfort.TileOverlap);

            Assert.Equal(0.03125, belfort.Segments[0, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[0, 0].VB);

            Assert.Equal(0.03125, belfort.Segments[0, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[0, 21].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[21, 0].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[21, 21].VB);

            var test01 = new ImageryTiler(512, 1, 2560);
            Assert.Equal(32, test01.TileOverlap);
            Assert.Equal(0.031250, test01.Segments[0, 0].UB);
            Assert.Equal(5.031250, test01.Segments[0, 0].VB);
            Assert.Equal(-4.656250, test01.Segments[5, 5].UB);
            Assert.Equal(0.343750, test01.Segments[5, 5].VB);

            var test02 = new ImageryTiler(1024, 1, 2560);
            Assert.Equal(64, test02.TileOverlap);
            Assert.Equal(0.031250, test02.Segments[0, 0].UB);
            Assert.Equal(2.531250, test02.Segments[0, 0].VB);
            Assert.Equal(-1.843750, test02.Segments[2, 2].UB);
            Assert.Equal(0.656250, test02.Segments[2, 2].VB);

            var test03 = new ImageryTiler(1024, 1, 29696);
            Assert.Equal(96, test03.TileOverlap);
            Assert.Equal(0.046875, test03.Segments[0, 0].UB);
            Assert.Equal(29.046875, test03.Segments[0, 0].VB);
            Assert.Equal(-28.046875, test03.Segments[31, 31].UB);
            Assert.Equal(0.953125, test03.Segments[31, 31].VB);
        }


        [Fact]
        public void ImageryTiler_UA()
        {
            var gossi = new ImageryTiler(Arma3MapConfigMock.Gossi);
            Assert.Equal(0.000488281250, gossi.Segments[0, 0].UA);

            var taunus = new ImageryTiler(Arma3MapConfigMock.Taunus);
            Assert.Equal(0.0009765625, taunus.Segments[0, 0].UA);

            var belfort = new ImageryTiler(Arma3MapConfigMock.Belfort);
            Assert.Equal(0.0009765625, belfort.Segments[0, 0].UA);
        }
    }
}
