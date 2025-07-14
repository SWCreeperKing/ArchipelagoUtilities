using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    internal class OutdatedClientConnectionResult : ClientVersionMismatchConnectionResult
    {
        public OutdatedClientConnectionResult(string clientName, string currentVersion, string expectedVersion) :
            this($"This Multiworld has been created for {clientName} version {expectedVersion},{Environment.NewLine}but this is {clientName} version {currentVersion}.{Environment.NewLine}Please update to a compatible mod version.", clientName, currentVersion, expectedVersion)
        {
        }

        public OutdatedClientConnectionResult(string errorMessage, string clientName, string currentVersion, string expectedVersion) : base(errorMessage, clientName, currentVersion, expectedVersion)
        {
        }
    }
}
