using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.Imagery
{
    public class Arma3ColorRenderTest
    {
        [Fact]
        public void ToArma3_Rgba32()
        {
            Assert.Equal(new Rgba32(0, 0, 0, 255), Arma3ColorRender.ToArma3(new Rgba32(0, 0, 0, 255)));
            Assert.Equal(new Rgba32(212, 204, 200, 255), Arma3ColorRender.ToArma3(new Rgba32(128, 128, 128, 255)));
            Assert.Equal(new Rgba32(249, 247, 249, 255), Arma3ColorRender.ToArma3(new Rgba32(255, 255, 255, 255)));
        }

        [Fact]
        public void ToArma3_Bgra32()
        {
            Assert.Equal(new Bgra32(0, 0, 0, 255), Arma3ColorRender.ToArma3(new Bgra32(0, 0, 0, 255)));
            Assert.Equal(new Bgra32(212, 204, 200, 255), Arma3ColorRender.ToArma3(new Bgra32(128, 128, 128, 255)));
            Assert.Equal(new Bgra32(249, 247, 249, 255), Arma3ColorRender.ToArma3(new Bgra32(255, 255, 255, 255)));
        }

        [Fact]
        public void ToArma3()
        {
            // Check consistent convertion for Bgra32 and Rgba32
            Rgba32 actual = new Rgba32();
            for (var i = 0; i < 256; ++i)
            {
                var expected = Arma3ColorRender.ToArma3(new Rgba32((byte)i, (byte)i, (byte)i, (byte)i));
                Arma3ColorRender.ToArma3(new Bgra32((byte)i, (byte)i, (byte)i, (byte)i)).ToRgba32(ref actual);
                Assert.Equal(expected, actual);
            }

        }

        [Fact]
        public void FromArma3_Rgba32()
        {
            Assert.Equal(new Rgba32(0, 0, 0, 255), Arma3ColorRender.FromArma3(new Rgba32(0, 0, 0, 255)));
            Assert.Equal(new Rgba32(128, 128, 128, 255), Arma3ColorRender.FromArma3(new Rgba32(212, 204, 200, 255)));
            Assert.Equal(new Rgba32(255, 255, 255, 255), Arma3ColorRender.FromArma3(new Rgba32(249, 247, 249, 255)));
        }

        [Fact]
        public void FromArma3_Bgra32()
        {
            Assert.Equal(new Bgra32(0, 0, 0, 255), Arma3ColorRender.FromArma3(new Bgra32(0, 0, 0, 255)));
            Assert.Equal(new Bgra32(128, 128, 128, 255), Arma3ColorRender.FromArma3(new Bgra32(212, 204, 200, 255)));
            Assert.Equal(new Bgra32(255, 255, 255, 255), Arma3ColorRender.FromArma3(new Bgra32(249, 247, 249, 255)));
        }

        [Fact]
        public void FromArma3()
        {
            // Check consistent convertion for Bgra32 and Rgba32
            Rgba32 actual = new Rgba32();
            for (var i = 0; i < 256; ++i)
            {
                var expected = Arma3ColorRender.FromArma3(new Rgba32((byte)i, (byte)i, (byte)i, (byte)i));
                Arma3ColorRender.FromArma3(new Bgra32((byte)i, (byte)i, (byte)i, (byte)i)).ToRgba32(ref actual);
                Assert.Equal(expected, actual);
            }

        }

        [Fact]
        public void Mutate()
        {
            var img = new Image<Rgba32>(2, 2);
            img[0, 0] = new Rgba32(0, 0, 0, 255);
            img[0, 1] = new Rgba32(128, 128, 128, 255);
            img[1, 0] = new Rgba32(128, 128, 128, 255);
            img[1, 1] = new Rgba32(255, 255, 255, 255);

            Arma3ColorRender.Mutate(img, Arma3ColorRender.ToArma3);

            Assert.Equal(new Rgba32(0, 0, 0, 255), img[0, 0]);
            Assert.Equal(new Rgba32(212, 204, 200, 255), img[0, 1]);
            Assert.Equal(new Rgba32(212, 204, 200, 255), img[1, 0]);
            Assert.Equal(new Rgba32(249, 247, 249, 255), img[1, 1]);
        }
    }
}
