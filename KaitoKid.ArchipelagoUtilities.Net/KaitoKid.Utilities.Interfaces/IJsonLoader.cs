namespace KaitoKid.Utilities.Interfaces
{
    public interface IJsonLoader
    {
        Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath);

        Dictionary<string, JObject> DeserializeFile(string filePath);
    }
}
