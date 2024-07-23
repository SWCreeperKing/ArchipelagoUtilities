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
            LogDebug($"{message}\n\t{GenerateArgumentsString(arguments)})");
        }

        public void LogErrorException(Exception ex, params object[] arguments)
        {
            LogError($"Exception Thrown:\n\t{ex}\n\t{GenerateArgumentsString(arguments)}");
        }

        public void LogWarningException(Exception ex, params object[] arguments)
        {
            LogWarning($"Exception Thrown:\n\t{ex}\n\t{GenerateArgumentsString(arguments)}");
        }

        public void LogErrorMessage(string message, params object[] arguments)
        {
            LogError($"{message}\n\t{GenerateArgumentsString(arguments)}");
        }

        public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        {
            LogError($"Failed in {patchType}.{patchMethod}:\n\t{ex}\n\t{GenerateArgumentsString(arguments)}");
        }

        private static string GenerateArgumentsString(object[] arguments)
        {
            var argumentsString = arguments != null && arguments.Any() ? string.Join(", ", arguments) : "";
            return argumentsString;
        }
    }
}
