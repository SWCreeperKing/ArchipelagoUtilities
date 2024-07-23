using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaitoKid.ArchipelagoUtilities.Net.Interfaces
{
    public interface ISlotData
    {
        string MultiworldVersion { get; }
        bool? DeathLink { get; }
    }
}
