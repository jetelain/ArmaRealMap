using System.Numerics;
using GameRealisticMap.Arma3.GameEngine;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class ImageryTilerTest
    {
        [Fact]
        public void ImageryTiler_Multipler()
        {
            var tiler = new ImageryTiler(new Arma3MapConfigMock() { CellSize = new Vector2(2), Resolution = 1, Size = 2048, TileSize = 512, IdMapMultiplier = 1 });
            Assert.Equal(512, tiler.TileSize);
            Assert.Equal(new Size(4096), tiler.FullImageSize);
            Assert.Equal(512, tiler.IdMapTileSize);
            Assert.Equal(new Size(4096), tiler.IdMapFullImageSize);

            tiler = new ImageryTiler(new Arma3MapConfigMock() { CellSize = new Vector2(2), Resolution = 1, Size = 2048, TileSize = 512, IdMapMultiplier = 2 });
            Assert.Equal(512, tiler.TileSize);
            Assert.Equal(new Size(4096), tiler.FullImageSize);
            Assert.Equal(1024, tiler.IdMapTileSize);
            Assert.Equal(new Size(8192), tiler.IdMapFullImageSize);

            Assert.Throws<ArgumentException>(() => new ImageryTiler(new Arma3MapConfigMock() { CellSize = new Vector2(2), Resolution = 1, Size = 2048, TileSize = 4096, IdMapMultiplier = 2 }));
        }

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

        [Fact]
        public void ImageryTiler_Constructor_ValidParameters()
        {
            var tiler = new ImageryTiler(512, 1, 2048, 1);
            Assert.Equal(512, tiler.TileSize);
            Assert.Equal(new Size(2048), tiler.FullImageSize);
            Assert.Equal(1, tiler.Resolution);
            Assert.Equal(1, tiler.IdMapMultiplier);
            Assert.Equal(512, tiler.IdMapTileSize);
            Assert.Equal(new Size(2048), tiler.IdMapFullImageSize);
        }

        [Fact]
        public void ImageryTiler_Constructor_InvalidIdMapMultiplier_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ImageryTiler(512, 1, 2048, 10));
        }

        [Fact]
        public void ImageryTiler_TileOverlap_Computation_2048()
        {
            var tiler = new ImageryTiler(512, 1, 2048);
            Assert.Equal(64, tiler.TileOverlap);
        }

        [Fact]
        public void ImageryTiler_TileOverlap_Computation_4096()
        {
            var tiler = new ImageryTiler(512, 1, 4096);
            Assert.Equal(128, tiler.TileOverlap);
        }

        [Fact]
        public void ImageryTiler_TileOverlap_Computation_8192()
        {
            var tiler = new ImageryTiler(512, 1, 8192);
            Assert.Equal(128, tiler.TileOverlap);
        }

        [Fact]
        public void ImageryTiler_Segments_Creation()
        {
            var tiler = new ImageryTiler(512, 1, 2048);
            Assert.NotNull(tiler.Segments);
            Assert.Equal(5, tiler.Segments.GetLength(0));
            Assert.Equal(5, tiler.Segments.GetLength(1));
        }

        [Fact]
        public void ImageryTiler_Segments_Properties_2048()
        {
            var tiler = new ImageryTiler(512, 1, 2048);
            var segment = tiler.Segments[0, 0];
            Assert.Equal(0, segment.X);
            Assert.Equal(0, segment.Y);
            Assert.Equal(512, segment.Size);
            Assert.Equal(0.001953125, segment.UA);
            Assert.Equal(0.0625, segment.UB);
            Assert.Equal(4.0625, segment.VB);
        }

        [Fact]
        public void ImageryTiler_Segments_Properties_4096()
        {
            var tiler = new ImageryTiler(512, 1, 4096);
            var segment = tiler.Segments[0, 0];
            Assert.Equal(0, segment.X);
            Assert.Equal(0, segment.Y);
            Assert.Equal(512, segment.Size);
            Assert.Equal(0.001953125, segment.UA);
            Assert.Equal(0.125, segment.UB);
            Assert.Equal(8.125, segment.VB);
        }

        [Fact]
        public void ImageryTiler_Segments_Properties_8192()
        {
            var tiler = new ImageryTiler(512, 1, 8192);
            var segment = tiler.Segments[0, 0];
            Assert.Equal(0, segment.X);
            Assert.Equal(0, segment.Y);
            Assert.Equal(512, segment.Size);
            Assert.Equal(0.001953125, segment.UA);
            Assert.Equal(0.125, segment.UB);
            Assert.Equal(16.125, segment.VB);
        }
    }
}
