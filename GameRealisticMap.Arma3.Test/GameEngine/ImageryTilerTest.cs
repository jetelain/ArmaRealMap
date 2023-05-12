using GameRealisticMap.Arma3.GameEngine;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class ImageryTilerTest
    {
        [Fact]
        public void ImageryTiler_UVB()
        {
            var gossi = new ImageryTiler(Arma3MapConfigMock.Gossi);

            Assert.Equal(0.03125, gossi.Segments[0, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[0, 0].VB);

            Assert.Equal(0.03125, gossi.Segments[0, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[0, 42].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[42, 0].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[42, 42].VB);

            var taunus = new ImageryTiler(Arma3MapConfigMock.Taunus);

            Assert.Equal(0.03125, taunus.Segments[0, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[0, 0].VB);

            Assert.Equal(0.03125, taunus.Segments[0, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[0, 21].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[21, 0].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[21, 21].VB);

            var belfort = new ImageryTiler(Arma3MapConfigMock.Belfort);

            Assert.Equal(0.03125, belfort.Segments[0, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[0, 0].VB);

            Assert.Equal(0.03125, belfort.Segments[0, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[0, 21].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[21, 0].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[21, 21].VB);
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
