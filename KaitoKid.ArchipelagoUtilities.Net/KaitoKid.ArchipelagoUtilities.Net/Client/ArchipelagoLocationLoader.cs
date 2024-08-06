using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ArchipelagoLocationLoader : IArchipelagoLoader<ArchipelagoLocation>
    {
        private IJsonLoader _jsonLoader;

        public ArchipelagoLocationLoader(IJsonLoader jsonLoader)
        {
            _jsonLoader = jsonLoader;
        }

        public IEnumerable<ArchipelagoLocation> LoadAll(params string[] path)
        {
            var pathToLocationTable = Path.Combine(path);
            var locationsTable = _jsonLoader.DeserializeFile(pathToLocationTable);
            var locations = locationsTable["locations"];
            foreach (var locationJson in locations)
            {
                yield return Load(locationJson.Key, locationJson.Value);
            }
        }

        public virtual ArchipelagoLocation Load(string locationName, JToken locationJson)
        {
            // var id = locationJson["code"].Value<long>();
            var id = locationJson.Value<long>();
            var location = new ArchipelagoLocation(locationName, id);
            return location;
        }
    }
}
