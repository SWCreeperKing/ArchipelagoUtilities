using Archipelago.MultiClient.Net.Packets;

namespace KaitoKid.ArchipelagoUtilities.Net.BouncePackets
{
    public static class BouncePacketExtensions
    {
        public static T GetDataValue<T>(this BouncePacket bouncePacket, string key, T defaultValue)
        {
            return bouncePacket.Data.TryGetValue(key, out var dataValue) ? dataValue.ToObject<T>() : defaultValue;
        }
    }
}
