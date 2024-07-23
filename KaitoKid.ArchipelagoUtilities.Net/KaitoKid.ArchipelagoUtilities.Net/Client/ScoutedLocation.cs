using System;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ScoutedLocation
    {
        private const string UNKNOWN_AP_ITEM = "Item for another world in this Archipelago";

        public string LocationName { get; private set; }
        public string ItemName { get; private set; }
        public string PlayerName { get; private set; }
        public long LocationId { get; private set; }
        public long ItemId { get; private set; }
        public long PlayerId { get; private set; }
        public string Classification { get; private set; }

        public ScoutedLocation(string locationName, string itemName, string playerName, long locationId, long itemId,
            long playerId, string classification)
        {
            LocationName = locationName;
            ItemName = itemName;
            PlayerName = playerName;
            LocationId = locationId;
            ItemId = itemId;
            PlayerId = playerId;
            Classification = classification;
        }

        public string GetItemName(Func<string, string> nameTransform = null)
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                return ItemId.ToString();
            }

            return nameTransform == null ? ItemName : nameTransform(ItemName);
        }

        public override string ToString()
        {
            return $"{PlayerName}'s {GetItemName()}";
        }

        public static string GenericItemName()
        {
            return UNKNOWN_AP_ITEM;
        }
    }
}
