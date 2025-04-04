using System;
using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ArchipelagoItemLoader : IArchipelagoLoader<ArchipelagoItem>
    {
        private IJsonLoader _jsonLoader;

        public ArchipelagoItemLoader(IJsonLoader jsonLoader)
        {
            _jsonLoader = jsonLoader;
        }

        public IEnumerable<ArchipelagoItem> LoadAll(params string[] path)
        {
            var pathToItemTable = Path.Combine(path);
            var itemsTable = _jsonLoader.DeserializeFile(pathToItemTable);
            var items = itemsTable["items"];
            foreach (var itemJson in items)
            {
                yield return Load(itemJson.Key, itemJson.Value);
            }
        }

        public virtual ArchipelagoItem Load(string itemName, JToken itemJson)
        {
            var id = itemJson["code"].Value<long>();
            var classification = (ItemClassification)Enum.Parse(typeof(ItemClassification), itemJson["classification"].Value<string>().Replace("|", ","), true);
            var item = new ArchipelagoItem(itemName, id, classification);
            return item;
        }
    }
}
