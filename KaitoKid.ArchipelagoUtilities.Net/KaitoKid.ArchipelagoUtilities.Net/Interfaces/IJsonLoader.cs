using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface IJsonLoader
    {
        Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath);

        Dictionary<string, JObject> DeserializeFile(string filePath);
    }
}
