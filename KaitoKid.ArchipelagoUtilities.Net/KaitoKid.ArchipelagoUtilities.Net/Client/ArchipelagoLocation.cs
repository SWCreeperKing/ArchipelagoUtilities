using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public class ArchipelagoLocation
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public ArchipelagoLocation(string name, long id)
        {
            Name = name;
            Id = id;
        }

        public static IEnumerable<ArchipelagoLocation> LoadLocations(IJsonLoader jsonLoader, params string[] path)
        {
            var pathToLocationTable = Path.Combine(path);
            var locationsTable = jsonLoader.DeserializeFile(pathToLocationTable);
            var locations = locationsTable["locations"];
            foreach (var locationJson in locations)
            {
                yield return LoadLocation(locationJson.Key, locationJson.Value);
            }
        }

        private static ArchipelagoLocation LoadLocation(string locationName, JToken locationJson)
        {
            var id = locationJson.Value<long>();
            var location = new ArchipelagoLocation(locationName, id);
            return location;
        }
    }
}