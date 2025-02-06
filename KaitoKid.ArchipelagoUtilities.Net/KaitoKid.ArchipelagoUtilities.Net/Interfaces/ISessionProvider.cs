using Archipelago.MultiClient.Net;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface ISessionProvider
    {
        ArchipelagoSession GetSession();
    }
}
