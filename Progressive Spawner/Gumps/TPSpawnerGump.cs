/****************************************************************\
* File Name : TPSpawnerGump.cs                                   *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : Displays a list of the levels that can be          *
*             defined.                                           *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY  | Comments                                          |
\****************************************************************/

using System;
using System.Collections;
using Server.Network;
using Server.Gumps;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class TPSpawnerGump : Gump
    {
        /// <summary>
        /// TPSpawner this gump is associated with
        /// </summary>
        private TPSpawner m_Spawner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spawner">TPSpawner this gump is associated with</param>
        public TPSpawnerGump(TPSpawner spawner)
            : base(50, 50)
        {
            m_Spawner = spawner;

            AddPage(0);

            AddBackground(0, 0, 125, 32 + m_Spawner.LevelAmt*22, 5054);

            AddLabel(32, 1, 0, "Levels List");
            
            for (int i = 0; i < m_Spawner.LevelAmt; i++)
            {
                AddButton(5, (22 * i) + 20, 0xFA5, 0xFA7, 1 + i, GumpButtonType.Reply, 0);

                AddImageTiled(40, (22 * i) + 20, 69, 23, 0xA40);
                AddImageTiled(41, (22 * i) + 21, 67, 21, 0xBBC);

                AddLabel(43, (22 * i) + 21, 154, String.Format("Level {0}", (i + 1)));
            }
        }

        /// <summary>
        /// Handles the various gump responses
        /// </summary>
        /// <param name="state">client receiving the gump packets</param>
        /// <param name="info">gump information</param>        
        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile m = state.Mobile;

            if (m_Spawner.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 0:
                    {
                        break;
                    }
                default:
                    {
                        m.SendGump(new TPLevelGump(m_Spawner, info.ButtonID));                        
                        break;
                    }
            }
        }
    }
}
