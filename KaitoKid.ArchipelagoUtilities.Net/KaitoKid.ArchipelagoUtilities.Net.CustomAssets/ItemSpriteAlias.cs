using System.Collections.Generic;

namespace KaitoKid.ArchipelagoUtilities.Net.CustomAssets
{
    public class ItemSpriteAlias
    {
        public string AliasName { get; set; } = "";

        public List<string> ItemNames { get; set; } = new List<string>();

        public ItemSpriteAlias()
        {
        }

        public bool ContainsAlias(string alias)
        {
            return ItemNames.Contains(alias);
        }
    }
}
