using System.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.CustomAssets
{
    internal class Alias
    {
        public string AliasName;
        public string[] ItemNames;

        public bool this[string itemName] => ItemNames.Contains(itemName);
    }
}