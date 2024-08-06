using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface IArchipelagoLoader<T>
    {
        IEnumerable<T> LoadAll(params string[] path);
        T Load(string locationName, JToken locationJson);
    }
}
