using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Json
{
    public class NewtonsoftJsonLoader : IJsonLoader
    {
        public Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return DeserializeJson<TKey, TValue>(json);
        }

        public Dictionary<TKey, TValue> DeserializeJson<TKey, TValue>(string json)
        {
           return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
        }

        public Dictionary<string, JObject> DeserializeFile(string filePath)
        {
            return DeserializeFile<string, JObject>(filePath);
        }

        public Dictionary<string, JObject> DeserializeJson(string json)
        {
            return DeserializeJson<string, JObject>(json);
        }
    }
}
