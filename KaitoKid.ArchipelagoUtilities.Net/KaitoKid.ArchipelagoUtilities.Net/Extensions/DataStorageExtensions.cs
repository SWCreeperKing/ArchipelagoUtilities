using System;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.Utilities.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net.Extensions
{
    public static class DataStorageExtensions
    {
        private static ILogger _logger = null;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static void InitializeToZero(this DataStorageElement dataStorageElement)
        {
            try
            {
                dataStorageElement.Initialize(0);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Exception occurred in InitializeToZero task: {ex}");
            }
        }
    }
}
