using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface IJsonLoader
    {
        Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath);
        Dictionary<TKey, TValue> DeserializeJson<TKey, TValue>(string json);

        Dictionary<string, JObject> DeserializeFile(string filePath);
        Dictionary<string, JObject> DeserializeJson(string json);
    }
}
