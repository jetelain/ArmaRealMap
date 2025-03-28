﻿using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.GameEngine;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class ImageryTilerHugeImagePartitionerTest
    {
        [Fact]
        public void CreateParts()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);

            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 1);
            var parts = partionner.CreateParts(new Size());
            Assert.Equal(484, parts.Count);
            var first = parts.First();
            Assert.Equal(new Rectangle(-16, -16, 512, 512), first.RealRectangle);
            Assert.Equal(new Rectangle(0, 0, 480, 480), first.Rectangle);

            var last = parts.Last();
            Assert.Equal(new Rectangle(10064, 10064, 512, 512), last.RealRectangle);
            Assert.Equal(new Rectangle(10080, 10080, 480, 480), last.Rectangle);

        }

        [Fact]
        public void CreateParts_X2()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);

            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 2);
            var parts = partionner.CreateParts(new Size());
            Assert.Equal(484, parts.Count);
            var first = parts.First();
            Assert.Equal(new Rectangle(-32, -32, 1024, 1024), first.RealRectangle);
            Assert.Equal(new Rectangle(0, 0, 960, 960), first.Rectangle);

            var last = parts.Last();
            Assert.Equal(new Rectangle(20128, 20128, 1024, 1024), last.RealRectangle);
            Assert.Equal(new Rectangle(20160, 20160, 960, 960), last.Rectangle);
        }

        [Fact]
        public void GetPartFromId_ValidId()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);
            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 1);

            var part = partionner.GetPartFromId(1);

            Assert.NotNull(part);
            Assert.Equal(0, part.X);
            Assert.Equal(0, part.Y);
        }

        [Fact]
        public void GetPartFromId_InvalidId()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);
            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => partionner.GetPartFromId(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => partionner.GetPartFromId(485));
        }

        [Fact]
        public void CreateParts_EmptySize()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);
            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 1);

            var parts = partionner.CreateParts(new Size());

            Assert.Equal(484, parts.Count);
        }

        [Fact]
        public void CreateParts_NonEmptySize()
        {
            var tiler = new ImageryTiler(512, 1, 10240, 1);
            var partionner = new ImageryTilerHugeImagePartitioner(tiler, 1);

            var parts = partionner.CreateParts(new Size(10240, 10240));

            Assert.Equal(484, parts.Count);
        }

    }
}
