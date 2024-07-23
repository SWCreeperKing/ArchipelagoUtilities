namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface ILogger
    {
        void LogError(string message);
        void LogWarning(string message);
        void LogInfo(string message);
        void LogMessage(string message);
    }
}
