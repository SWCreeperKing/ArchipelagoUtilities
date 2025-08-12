using System;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface ILogger
    {
        void LogError(string message);
        void LogError(string message, Exception e);
        void LogWarning(string message);
        void LogInfo(string message);
        void LogMessage(string message);
        void LogDebug(string message);

        void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod, params object[] arguments);
        void LogDebug(string message, params object[] arguments);
        void LogErrorException(string prefixMessage, Exception ex, params object[] arguments);
        void LogWarningException(string prefixMessage, Exception ex, params object[] arguments);
        void LogErrorException(Exception ex, params object[] arguments);
        void LogWarningException(Exception ex, params object[] arguments);
        void LogErrorMessage(string message, params object[] arguments);
        void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments);
    }
}
