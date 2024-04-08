namespace GameRealisticMap.Arma3.Aerial
{
    internal static class FileContentWatcher
    {
        internal static async Task Watch(string fileName, CancellationToken cancellationToken, Action<string> onNewLine)
        {
            using var reader = new StreamReader(new FileStream(fileName,
                     FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            var lastPosition = 0L;

            var remain = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);

                if (reader.BaseStream.Length != lastPosition)
                {
                    reader.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    var data = remain + await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                    var lines = data.Split('\n');
                    foreach (var line in lines.Take(lines.Length - 1))
                    {
                        onNewLine(line.TrimEnd('\r'));
                    }
                    remain = lines[lines.Length - 1];

                    //string? line;
                    //while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
                    //{
                    //    line.Split('\n');
                    //    Interpret(line, process);
                    //}

                    lastPosition = reader.BaseStream.Position;
                }
            }
        }
    }
}
