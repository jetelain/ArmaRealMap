namespace GameRealisticMap.Studio.Controls
{
    internal interface IGrmMapLayer
    {
        GrmMap? ParentMap { get; set; }

        void OnViewportChanged();
    }
}