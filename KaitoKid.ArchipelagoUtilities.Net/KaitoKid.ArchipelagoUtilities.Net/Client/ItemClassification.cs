using System;

namespace KaitoKid.ArchipelagoUtilities.Net.Client
{
    [Flags]
    public enum ItemClassification
    {
        Filler = 0b0000,
        Progression = 0b0001,
        Useful = 0b0010,
        Trap = 0b0100,
        SkipBalancing = 0b1000,
        ProgressionSkipBalancing = Progression | SkipBalancing,
        skip_balancing = SkipBalancing,
        progression_skip_balancing = ProgressionSkipBalancing,
    }
}