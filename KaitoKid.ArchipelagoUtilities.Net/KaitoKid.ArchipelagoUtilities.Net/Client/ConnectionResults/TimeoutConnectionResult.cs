using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public class TimeoutConnectionResult : FailedConnectionResult
    {
        public override bool RetryPossible => true;

        public TimeoutConnectionResult() : this("Connection timed out")
        {
        }

        public TimeoutConnectionResult(string errorMessage) : base(errorMessage)
        {
        }
    }
}
