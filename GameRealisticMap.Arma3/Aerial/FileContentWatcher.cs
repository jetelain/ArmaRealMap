using System.Runtime.CompilerServices;

namespace GameRealisticMap.Arma3.Aerial
{
    internal static class FileContentWatcher
    {
        internal static async Task Watch(string fileName, CancellationToken cancellationToken, Action<string> onNewLine)
        {
            try
            {
                await foreach(var line in WatchLines(fileName, cancellationToken))
                {
                    onNewLine(line);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        internal static async IAsyncEnumerable<string> WatchLines(string fileName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(new FileStream(fileName,
                     FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            var lastPosition = 0L;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (reader.BaseStream.Length != lastPosition)
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        yield return line;
                    }
                    lastPosition = reader.BaseStream.Position;
                }

                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
