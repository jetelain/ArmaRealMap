using System.Windows.Input;

namespace GameRealisticMap.Studio.Resources
{
    internal static class GrmCursors
    {
        private static Cursor GetCursor(string name)
        {
            using (var stream = typeof(GrmCursors).Assembly.GetManifestResourceStream($"GameRealisticMap.Studio.Resources.{name}.cur"))
            {
                return new Cursor(stream);
            }
        }

        public static Cursor Grab { get; } = GetCursor("hand_grab");

        public static Cursor Grabbing { get; } = GetCursor("hand_grabbing");
    }
}
