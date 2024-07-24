using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    public abstract class Logger : ILogger
    {
        public abstract void LogError(string message);

        public abstract void LogWarning(string message);

        public abstract void LogInfo(string message);

        public abstract void LogMessage(string message);

        public abstract void LogDebug(string message);

        public void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod, params object[] arguments)
        {
#if DEBUG
            LogDebug($"{patchedType}.{patchedMethod}.{patchType}.{patchMethod}({GenerateArgumentsString(arguments)})");
#endif
        }

        public void LogDebug(string message, params object[] arguments)
        {
            LogDebug($"{message}{Environment.NewLine}{GenerateArgumentsString(arguments)})");
        }

        public void LogErrorException(string prefixMessage, Exception ex, params object[] arguments)
        {
            LogError($"{prefixMessage}:{Environment.NewLine}{ex}{Environment.NewLine}{GenerateArgumentsString(arguments)}");
        }

        public void LogWarningException(string prefixMessage, Exception ex, params object[] arguments)
        {
            LogWarning($"{prefixMessage}:{Environment.NewLine}{ex}{Environment.NewLine}{GenerateArgumentsString(arguments)}");
        }

        public void LogErrorException(Exception ex, params object[] arguments)
        {
            LogErrorException("Exception Thrown", ex, arguments);
        }

        public void LogWarningException(Exception ex, params object[] arguments)
        {
            LogWarningException("Exception Thrown", ex, arguments);
        }

        public void LogErrorMessage(string message, params object[] arguments)
        {
            LogError($"{message}{Environment.NewLine}{GenerateArgumentsString(arguments)}");
        }

        public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        {
            LogError($"Failed in {patchType}.{patchMethod}:{Environment.NewLine}{ex}{Environment.NewLine}{GenerateArgumentsString(arguments)}");
        }

        private static string GenerateArgumentsString(object[] arguments)
        {
            var argumentsString = arguments != null && arguments.Any() ? string.Join(", ", arguments) : "";
            return argumentsString;
        }
    }
}
