using System;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    internal class TooUpdatedClientConnectionResult : ClientVersionMismatchConnectionResult
    {
        public TooUpdatedClientConnectionResult(string clientName, string currentVersion, string expectedVersion) :
            this($"This Multiworld has been created for {clientName} version {expectedVersion},{Environment.NewLine}but this is {clientName} version {currentVersion}.{Environment.NewLine}This can be caused by accidentally installing a pre-release mod.{Environment.NewLine}Please downgrade to a compatible mod version.", clientName, currentVersion, expectedVersion)
        {
        }

        public TooUpdatedClientConnectionResult(string errorMessage, string clientName, string currentVersion, string expectedVersion) : base(errorMessage, clientName, currentVersion, expectedVersion)
        {
        }
    }
}