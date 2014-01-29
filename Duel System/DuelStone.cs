/****************************************************************\
* File Name : DuelStone.cs                                       *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : A stone that allows a player start a duel with     *
*             another player.                                    *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY   | Comments                                         |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Accounting;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Items
{
    class DuelStone :Item
    {
        #region Positioning
        /// <summary>
        /// Map Location of the Arena
        /// </summary>
        private Map m_ArenaMap = Map.Felucca;

        /// <summary>
        /// Fighting Location for a duelists(RED)
        /// </summary>
        private Point3D redLOC;

        /// <summary>
        /// Fighting Location for a duelists(BLUE)
        /// </summary>
        private Point3D blueLOC;

        /// <summary>
        /// Staging Location for a duelists(RED)
        /// </summary>
        private Point3D redStage;

        /// <summary>
        /// Staging Location for a duelists(BLUE)
        /// </summary>
        private Point3D blueStage;

        /// <summary>
        /// Exit location
        /// </summary>
        private Point3D m_Exit;
        #endregion Positioning and Map Used

        #region Duel Settings
        /// <summary>
        /// Region in which the duel will be held
        /// </summary>
        //private DuelArenaRegion m_REGION;

        /// <summary>
        /// Delay before the start of each round
        /// </summary>
        private TimeSpan m_StagingTime = new TimeSpan(0, 0, 10); // 10 second delay

        /// <summary>
        /// Registration fee for those who wish to duel
        /// </summary>
        private int m_Fee = 0;

        /// <summary>
        /// Minimum wager amount to start a duel
        /// </summary>
        private int m_MinWager = 100;

        /// <summary>
        /// Wager of the current duel
        /// </summary>
        private int m_Wager;

        /// <summary>
        /// Amount of rounds
        /// </summary>
        private int m_Rounds = 1;

        /// <summary>
        /// Limit the length of the duels
        /// </summary>
        private TimeSpan m_MaxDuelTime = new TimeSpan(0, 30, 0); // 30min limit on duels
        #endregion Duel Settings

        #region Misc.
        /// <summary>
        /// Is the stone enabled?
        /// </summary>
        private bool m_Enabled = false;

        /// <summary>
        /// Is a Duel Gump out for this Duel Stone?
        /// </summary>
        private bool m_GumpOut = false;

        /// <summary>
        /// List of players in Queue to use the Duel Stone
        /// </summary>
        private List<Mobile> m_Queue = new List<Mobile>();

        /// <summary>
        /// Maximum Distance a player can be when his/her 
        /// place in the queue is called.
        /// </summary>
        private int m_QueuePaces = 30;

        /// <summary>
        /// Distance from which the double-click method will fire
        /// </summary>
        private int m_ClickDist = 10;

        /// <summary>
        /// Determines whether the duelists' items should be banked for
        /// the duel because they will be given supplies.
        /// </summary>
        private bool m_SuppliedDuel = false;
        #endregion Misc.

        #region Admin Get/Set
        [CommandProperty(AccessLevel.GameMaster)]
        public Map ArenaMap
        {
            get { return m_ArenaMap; }
            set { m_ArenaMap = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D DuelLoc1
        {
            get { return redLOC; }
            set { redLOC = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D DuelLoc2
        {
            get { return blueLOC; }
            set { blueLOC = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D StageLoc1
        {
            get { return redStage; }
            set { redStage = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D StageLoc2
        {
            get { return blueStage; }
            set { blueStage = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Exit
        {
            get { return m_Exit; }
            set { m_Exit = value; }
        }

        //[CommandProperty(AccessLevel.GameMaster)]
        //public DuelArenaRegion REGION
        //{
        //    get { return m_REGION; }
        //    set { m_REGION = value; }
        //}

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan StagingTime
        {
            get { return m_StagingTime; }
            set { m_StagingTime = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Fee
        {
            get { return m_Fee; }
            set { m_Fee = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinimumWager
        {
            get { return m_MinWager; }
            set { m_MinWager = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Wager
        {
            get { return m_Wager; }
            set { m_Wager = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Rounds
        {
            get { return m_Rounds; }
            set { m_Rounds = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int QueuePaces
        {
            get { return m_QueuePaces; }
            set { m_QueuePaces = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Enabled
        {
            get { return m_Enabled; }
            set 
            { 
                m_Enabled = value;
                if (m_Queue.Count != 0)
                {
                    foreach (Mobile m in m_Queue)
                    {
                        m.SendMessage("The Duel Stone has been disabled and is undergoing maintenance.");
                    }
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ClickDistance
        {
            get { return m_ClickDist; }
            set { m_ClickDist = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MaxDuelTime
        {
            get { return m_MaxDuelTime; }
            set { m_MaxDuelTime = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SuppliedDuel
        {
            get { return m_SuppliedDuel; }
            set { m_SuppliedDuel = value; }
        }
        #endregion Admin Get/Set

        /// <summary>
        /// Constructor
        /// </summary>
        [Constructable]
        public DuelStone() : base(0xED4)
        {
            Movable = false;
            Hue = 1175;
        }

        /// <summary>
        /// Refreshes the Duel Stone Queue and sends the gump to the
        /// next valid mobile in the Queue.
        /// </summary>
        public void refreshDuel()
        {
            if (m_Enabled)
            {
                List<Mobile> toRemove = new List<Mobile>();
                m_GumpOut = false;

                if (m_Queue.Count > 0)
                {
                    for (int i = 0; i < m_Queue.Count; i++)
                    {
                        if (!m_GumpOut)
                        {
                            Mobile m = m_Queue[i];
                            if (m.NetState != null && m.GetDistanceToSqrt(this.Location) <= m_QueuePaces)
                            {
                                m.SendGump(new DuelGump(m, this));
                                m_GumpOut = true;
                            }
                            toRemove.Add(m);
                        }
                    }

                    foreach (Mobile m in toRemove)
                    {
                        m_Queue.Remove(m);
                    }

                }
            }
        }

        #region Overrides
        /// <summary>
        /// Overrides the Double-Click option on the item. Supplies the
        /// player that double-clicks on the stone with appropriate gear
        /// based upon their skill levels.
        /// </summary>
        /// <param name="m">Mobile to be supplied</param>
        public override void OnDoubleClick(Mobile m)
        {
            if (m_Enabled && m.GetDistanceToSqrt(this.Location) <= m_ClickDist)
            {
                if (!m_GumpOut && m_Queue.Count == 0)
                {
                    m.SendGump(new DuelGump(m, this));
                    m_GumpOut = true;
                }
                else if (!m_Queue.Contains(m) && m.HasGump(typeof(DuelGump)))
                {
                    m.SendMessage("You are already setting up a duel.");
                }
                else if (!m_Queue.Contains(m))
                {
                    m.SendMessage("This duel stone is currently in use.");
                    m.SendMessage("You are being added to the queue.");
                    m.SendMessage(String.Format("Place in Queue: {0}.", m_Queue.Count + 1));
                    m.SendMessage(String.Format("If you are not with {0} paces of the stone when you are chosen, your place will be forfeited in the queue.", m_QueuePaces));
                    m_Queue.Add(m);
                }
                else
                    m.SendMessage("You have already been added to the queue.");
            }
            else
                m.SendMessage("This stone has not been enabled.");
        }

        /// <summary>
        /// Default name of the supply stone
        /// </summary>
        public override string DefaultName
        {
            get { return "a duel stone"; }
        }
        #endregion Overrides

        #region Serialization
        /// <summary>
        /// Allows for serialization of the duel stone 
        /// </summary>
        /// <param name="serial"></param>
        public DuelStone(Serial serial) : base(serial)
        {
        }

        /// <summary>
        /// Saves the duel stone settings
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        
            writer.Write((int)1); // version
            writer.Write((Map)m_ArenaMap);
            writer.Write((Point3D)redLOC);
            writer.Write((Point3D)blueLOC);
            writer.Write((Point3D)redStage);
            writer.Write((Point3D)blueStage);
            writer.Write((Point3D)m_Exit);
            writer.Write((TimeSpan)m_StagingTime);
            writer.Write((int)m_Fee);
            writer.Write((int)m_MinWager);
            writer.Write((int)m_Wager);
            writer.Write((int)m_Rounds);
            writer.Write((bool)m_Enabled);
            writer.Write((int)m_ClickDist);
            writer.Write((TimeSpan)m_MaxDuelTime);
            writer.Write((bool)m_SuppliedDuel);
        }

        /// <summary>
        /// Loads the duel stone settings
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                        break;
                case 1:
                {
                    m_ArenaMap = reader.ReadMap();
                    redLOC = reader.ReadPoint3D();
                    blueLOC = reader.ReadPoint3D();
                    redStage = reader.ReadPoint3D();
                    blueStage = reader.ReadPoint3D();
                    m_Exit = reader.ReadPoint3D();
                    m_StagingTime = reader.ReadTimeSpan();
                    m_Fee = reader.ReadInt();
                    m_MinWager = reader.ReadInt();
                    m_Wager = reader.ReadInt();
                    m_Rounds = reader.ReadInt();
                    m_Enabled = reader.ReadBool();
                    m_ClickDist = reader.ReadInt();
                    m_MaxDuelTime = reader.ReadTimeSpan();
                    m_SuppliedDuel = reader.ReadBool();
                    break;
                }
            }
        }
        #endregion Serialization
    }
}
