/****************************************************************\
* File Name : TPLevelGump.cs                                     *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : A modified version of the original RunUO           *
*             SpawnerGump. Used to allow in-game changes to the  *
*             progressive spawner.                               *
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
    public class TPLevelGump : Gump
    {
        /// <summary>
        /// TPSpawner this gump is associated with
        /// </summary>
        private TPSpawner m_Spawner;

        /// <summary>
        /// The Progression level this gump is defining
        /// </summary>
        private int m_Level;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spawner">TPSpawner this gump is associated with</param>
        /// <param name="level">The Progression level this gump is defining</param>
        public TPLevelGump(TPSpawner spawner, int level)
            : base(50, 50)
        {
            m_Spawner = spawner;
            m_Level = level;

            AddPage(0);

            AddBackground(0, 0, 262, 371, 5054);

            AddLabel(75, 1, 0, String.Format("Level {0} Creatures List", m_Level));

            AddButton(5, 347, 0xFB1, 0xFB3, 0, GumpButtonType.Reply, 0);
            AddLabel(38, 347, 0x384, "Cancel");

            AddButton(5, 325, 0xFB7, 0xFB9, 1, GumpButtonType.Reply, 0);
            AddLabel(38, 325, 0x384, "Okay");

            AddButton(110, 325, 0xFB4, 0xFB6, 2, GumpButtonType.Reply, 0);
            AddLabel(143, 325, 0x384, "Bring to Home");

            AddButton(110, 347, 0xFA8, 0xFAA, 3, GumpButtonType.Reply, 0);
            AddLabel(143, 347, 0x384, "Total Respawn");

            for (int i = 0; i < 12; i++)
            {
                AddButton(5, (22 * i) + 20, 0xFA5, 0xFA7, 4 + (i * 2), GumpButtonType.Reply, 0);
                AddButton(38, (22 * i) + 20, 0xFA2, 0xFA4, 5 + (i * 2), GumpButtonType.Reply, 0);

                AddImageTiled(71, (22 * i) + 20, 159, 23, 0xA40);
                AddImageTiled(72, (22 * i) + 21, 157, 21, 0xBBC);

                string str = "";

                if ( i < spawner.GetLevelMobs(m_Level).Count)
                {
                    str = (string)spawner.GetLevelMobs(m_Level)[i];
                    int count = m_Spawner.CountCreatures(str);
                }

                AddTextEntry(75, (22 * i) + 21, 154, 21, 0, i, str);

                AddImageTiled(231, (22 * i) + 20, 24, 23, 0xA40);
                AddImageTiled(232, (22 * i) + 21, 22, 21, 0xBBC);

                if (i < spawner.GetLevelAmts(m_Level).Count)
                    AddTextEntry(233, (22 * i) + 21, 21, 21, 0, i + 12, spawner.GetLevelAmts(m_Level)[i].ToString());
                else
                    AddTextEntry(233, (22 * i) + 21, 21, 21, 0, i + 12, "0");
            }

            int j = 12;
            AddImageTiled(60, (22 * j) + 22, 79, 23, 0xA40);
            AddImageTiled(61, (22 * j) + 23, 77, 21, 0xBBC);
            AddLabel(63, (22 * j) + 23, 0x384, "Time Limit");

            AddImageTiled(147, (22 * j) + 22, 24, 23, 0xA40);
            AddImageTiled(148, (22 * j) + 23, 22, 21, 0xBBC);
            AddLabel(174, (22 * j) + 21, 0x384, ":");
            AddImageTiled(180, (22 * j) + 22, 24, 23, 0xA40);
            AddImageTiled(181, (22 * j) + 23, 22, 21, 0xBBC);

            AddTextEntry(150, (22 * j) + 23, 21, 21, 0, j + 13, spawner.GetLevelLimit(m_Level).Minutes.ToString()); 
            AddTextEntry(183, (22 * j) + 23, 21, 21, 0, j + 14, spawner.GetLevelLimit(m_Level).Seconds.ToString()); 
        }

        /// <summary>
        /// Create the list of mobiles/items and determines if they are valid
        /// </summary>
        /// <param name="info">gump information</param>
        /// <param name="from">mobile that the gump is displayed to</param>
        /// <returns>list of mobiles/items</returns>
        public List<string> CreateArray(RelayInfo info, Mobile from)
        {
            List<string> creaturesName = new List<string>();

            for (int i = 0; i < 12; i++)
            {
                TextRelay te = info.GetTextEntry(i);

                if (te != null)
                {
                    string str = te.Text;

                    if (str.Length > 0)
                    {
                        str = str.Trim();

                        Type type = SpawnerType.GetType(str);

                        if (type != null)
                            creaturesName.Add(str);
                        else
                            from.SendMessage("{0} is not a valid type name.", str);
                    }
                }
            }

            return creaturesName;
        }

        /// <summary>
        /// Create the list of amounts
        /// </summary>
        /// <param name="info">gump information</param>
        /// <param name="from">mobile that the gump is displayed to</param>
        /// <returns>list of amounts</returns>
        public List<int> CreateAmtList(RelayInfo info, Mobile from)
        {
            List<int> mobAmt = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                TextRelay te = info.GetTextEntry(i+12);

                if (te != null)
                {
                    string str = te.Text;

                    if (str.Length > 0)
                    {
                        str = str.Trim();
                        int amt = 1;
                        int.TryParse(str, out amt);
                        mobAmt.Add(amt);
                    }
                }
            }

            return mobAmt;
        }

        /// <summary>
        /// Creates the time limit
        /// </summary>
        /// <param name="info">gump information</param>
        /// <param name="from">mobile that the gump is displayed to</param>
        /// <returns>time limit</returns>
        public TimeSpan CreateLimit(RelayInfo info, Mobile from)
        {
            TimeSpan limit;
            int minutes = 0;
            int seconds = 0;

            int.TryParse(info.GetTextEntry(25).Text, out minutes);
            int.TryParse(info.GetTextEntry(26).Text, out seconds);

            limit = new TimeSpan(0, minutes, seconds);

            return limit;
        }

        /// <summary>
        /// Handles the various gump responses
        /// </summary>
        /// <param name="state">client receiving the gump packets</param>
        /// <param name="info">gump information</param>
        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_Spawner.Deleted)
                return;

            Mobile m = state.Mobile;

            switch (info.ButtonID)
            {
                case 0: // Closed
                    {
                        m.SendGump(new TPSpawnerGump(m_Spawner));
                        break;
                    }
                case 1: // Okay
                    {
                        List<string> creatures = CreateArray(info, state.Mobile);
                        List<int> amts = CreateAmtList(info, state.Mobile);
                        TimeSpan limit = CreateLimit(info, state.Mobile);

                        m_Spawner.SetLevel(m_Level, creatures, amts, limit);
                        m.SendGump(new TPSpawnerGump(m_Spawner));
                        break;
                    }
                case 2: // Bring everything home
                    {
                        m_Spawner.BringToHome();
                        m.SendGump(new TPSpawnerGump(m_Spawner));
                        break;
                    }
                case 3: // Complete respawn
                    {
                        m_Spawner.Respawn(m_Level);
                        m.SendGump(new TPSpawnerGump(m_Spawner));
                        break;
                    }
                default:
                    {
                        int buttonID = info.ButtonID - 4;
                        int index = buttonID / 2;
                        int type = buttonID % 2;

                        TextRelay entry = info.GetTextEntry(index);

                        if (entry != null && entry.Text.Length > 0)
                        {
                            if (type == 0) // Spawn creature
                            {
                                m.SendMessage("This feature is currently disabled.");
                                //m_Spawner.Spawn(entry.Text);
                            }
                            else // Remove creatures
                                m_Spawner.RemoveCreatures(entry.Text);
                        
                            m_Spawner.CreaturesName = CreateArray(info, state.Mobile);
                        }
                        m.SendGump(new TPSpawnerGump(m_Spawner));
                        break;
                    }
            }
        }
    }
}
