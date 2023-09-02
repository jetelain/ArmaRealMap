namespace GameRealisticMap.ManMade.Roads.Libraries
{
    public class DefaultBridgeInfos : IBridgeInfos
    {
        public static IBridgeInfos NoBridge = new DefaultBridgeInfos(false, float.MaxValue);

        public static IBridgeInfos AnyBridgeSize = new DefaultBridgeInfos(true, 0);

        public DefaultBridgeInfos(bool hasBridge, float minimalBridgeLength)
        {
            HasBridge = hasBridge;
            MinimalBridgeLength = minimalBridgeLength;
        }

        public bool HasBridge { get; }

        public float MinimalBridgeLength { get; }
    }
}
