using System;
using System.Collections.Generic;
using System.Text;

namespace Duel.Arena
{
    public enum ArenaFlag : int
    {
        StuckMenuEnabled = 1,
        Leavable = 2,
        LootingEnabled = 3,
        LootSelfEnabled = 4,
        PotionsEnabled = 5,
    }

    class DuelArenaControl
    {
        /* :TODO: Design an alternative to using Custom Regions in a box
                  to enable the duel system to be standalone.
        */
    }
}
