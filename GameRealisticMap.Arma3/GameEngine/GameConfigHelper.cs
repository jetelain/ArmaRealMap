using BIS.Core.Config;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal static class GameConfigHelper
    {
        private static bool IsBinaryConfig(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Position = 0;
            return buffer.SequenceEqual(new byte[] { 0, (byte)'r', (byte)'a', (byte)'P' });
        }

        public static async Task<string> GetText(Stream stream)
        {
            if (IsBinaryConfig(stream))
            {
                return new ParamFile(stream).ToString();
            }
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
