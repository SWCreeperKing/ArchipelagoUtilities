using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults
{
    public abstract class ConnectionResult
    {
        public abstract bool Success { get; }
        public virtual string Message => Success.ToString();
    }
}
