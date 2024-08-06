using System;
using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ArchipelagoItem
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public ItemClassification Classification { get; set; }

        public ArchipelagoItem(string name, long id, ItemClassification classification)
        {
            Name = name;
            Id = id;
            Classification = classification;
        }

        public static IEnumerable<ArchipelagoItem> LoadItems(IJsonLoader jsonLoader, params string[] path)
        {
            var pathToItemTable = Path.Combine(path);
            var itemsTable = jsonLoader.DeserializeFile(pathToItemTable);
            var items = itemsTable["items"];
            foreach (var itemJson in items)
            {
                yield return LoadItem(itemJson.Key, itemJson.Value);
            }
        }

        private static ArchipelagoItem LoadItem(string itemName, JToken itemJson)
        {
            var id = itemJson["code"].Value<long>();
            var classification = (ItemClassification)Enum.Parse(typeof(ItemClassification), itemJson["classification"].Value<string>(), true);
            var item = new ArchipelagoItem(itemName, id, classification);
            return item;
        }
    }

    [Flags]
    public enum ItemClassification
    {
        Filler = 0b0000,
        Progression = 0b0001,
        Useful = 0b0010,
        Trap = 0b0100,
        SkipBalancing = 0b1000,
        ProgressionSkipBalancing = Progression | SkipBalancing,
        skip_balancing = SkipBalancing,
        progression_skip_balancing = ProgressionSkipBalancing,
    }
}
