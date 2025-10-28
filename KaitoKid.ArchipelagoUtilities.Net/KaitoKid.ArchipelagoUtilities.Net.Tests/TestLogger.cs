
namespace KaitoKid.ArchipelagoUtilities.Net.Tests
{
    internal class TestLogger : ILogger
    {
        public void LogError(string message)
        {
            Console.WriteLine($"Test Error: {message}");
        }

        public void LogError(string message, Exception e)
        {
            LogError(message);
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"Test Warning: {message}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"Test Info: {message}");
        }

        public void LogMessage(string message)
        {
            Console.WriteLine($"Test Message: {message}");
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"Test Debug: {message}");
        }

        public void LogDebugPatchIsRunning(string patchedType, string patchedMethod, string patchType, string patchMethod,
            params object[] arguments)
        {
        }

        public void LogDebug(string message, params object[] arguments)
        {
        }

        public void LogErrorException(string prefixMessage, Exception ex, params object[] arguments)
        {
        }

        public void LogWarningException(string prefixMessage, Exception ex, params object[] arguments)
        {
        }

        public void LogErrorException(Exception ex, params object[] arguments)
        {
        }

        public void LogWarningException(Exception ex, params object[] arguments)
        {
        }

        public void LogErrorMessage(string message, params object[] arguments)
        {
        }

        public void LogErrorException(string patchType, string patchMethod, Exception ex, params object[] arguments)
        {
        }
    }
}
