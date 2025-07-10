using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public class SuccessConnectionResult : ConnectionResult
    {
        public override bool Success => true;
        public override string Message { get; }

        public SuccessConnectionResult() : this("Success")
        {

        }

        public SuccessConnectionResult(string message)
        {
            Message = message;
        }
    }
}
