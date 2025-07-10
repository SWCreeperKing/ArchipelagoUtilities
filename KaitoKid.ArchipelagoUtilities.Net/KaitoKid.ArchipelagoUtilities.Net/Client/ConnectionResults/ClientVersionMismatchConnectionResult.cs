using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    internal class ClientVersionMismatchConnectionResult : FailedConnectionResult
    {
        public virtual string ClientName { get; }
        public virtual string CurrentVersion { get; }
        public virtual string ExpectedVersion { get; }

        public ClientVersionMismatchConnectionResult(string clientName, string currentVersion, string expectedVersion) :
            this($"This Multiworld has been created for {clientName} version {expectedVersion},{Environment.NewLine}but this is {clientName} version {currentVersion}.{Environment.NewLine}Please update to a compatible mod version.", clientName, currentVersion, expectedVersion)
        {
        }

        public ClientVersionMismatchConnectionResult(string errorMessage, string clientName, string currentVersion, string expectedVersion) : base(errorMessage)
        {
            ClientName = clientName;
            CurrentVersion = currentVersion;
            ExpectedVersion = expectedVersion;
        }
    }
}
