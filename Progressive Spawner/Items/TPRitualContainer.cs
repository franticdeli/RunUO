/****************************************************************\
* File Name : TPRitualContainer.cs                               *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : A container that will start a TPSpawner if it is   *
*             linked to a TPSpawner and the Ritual               *
*             requirements are met.                              *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY   | Comments                                         |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class TPRitualContainer : BaseContainer
    {
        #region Initializations
        /// <summary>
        /// Item type to be counted towards the ritual
        /// </summary>
        private Type m_RitualItem;

        /// <summary>
        /// Amount of the ritual item for the ritual to be consider complete
        /// </summary>
        private int m_RitualAmount;

        /// <summary>
        /// Current amount of the item in the container
        /// </summary>
        private int m_Count;

        /// <summary>
        /// TPSpawner to start when the ritual is complete
        /// </summary>
        private TPSpawner m_Spawner;

        /// <summary>
        /// List of the ritual items within this container
        /// </summary>
        private List<Item> m_RitualItems;
        #endregion Initializations

        #region Admin Gets/Sets
        [CommandProperty(AccessLevel.GameMaster)]
        public Type RitualItem
        {
            get { return m_RitualItem; }
            set { m_RitualItem = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RitualAmount
        {
            get { return m_RitualAmount; }
            set { m_RitualAmount = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item PSpawner
        {
            get { return m_Spawner; }
            set 
            { 
                if(value is TPSpawner)
                    m_Spawner = (TPSpawner)value; 
            }
        }
        #endregion Admin Gets/Sets

        /// <summary>
        /// Constructor
        /// </summary>
        [Constructable]
        public TPRitualContainer()
            : base(0xE76)
		{
            Name = "ritual container";
            Movable = false;

            m_RitualItem = typeof(Gold);
            m_RitualAmount = 1;
            m_Count = 0;
            m_RitualItems = new List<Item>();
		}

        /// <summary>
        /// Checks if the item added is the ritual item and add the amount to the count
        /// to complete the ritual. Starts the spawn when the ritual is completed and deletes
        /// the ritual item(s).
        /// </summary>
        public void CheckSpawn(Mobile m, Item item)
        {
            if (m_Spawner != null && m_Spawner.Active)
            {
                m_Count += item.Amount;
                m_RitualItems.Add(item);

                double percent = (double)m_Count / (double)m_RitualAmount;
                if (percent > 0 && percent <= 0.25)
                    m.SendMessage("Much more is needed for the ritual to be completed");
                else if (percent > 0.25 && percent <= 0.50)
                    m.SendMessage("More is needed for this ritual");
                else if (percent > 0.50 && percent <= 0.75)
                    m.SendMessage("Just a few more are needed for this ritual");
                else if (percent > 0.75 && percent < 1.00)
                    m.SendMessage("The ritual is nearly complete");
                else if (percent >= 1.00)
                {
                    if (m_Spawner.RechargeDelay < DateTime.Now && m_Spawner.Active && !m_Spawner.SpawnStarted)
                    {
                        m_Count = 0;

                        m.SendMessage("The ritual is complete");

                        for (int i = m_RitualItems.Count - 1; i >= 0; i--)
                        {
                            Item it = m_RitualItems[i];
                            it.Delete();
                        }

                        m_Spawner.Start();
                    }
                    else
                        m.SendMessage("You complete the ritual, but the bag's magic is too weak");
                }

            }
        }

        #region Overrides
        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (m_Spawner == null || !(dropped.GetType().Equals(m_RitualItem)) || m_Spawner.RechargeDelay > DateTime.Now || m_Spawner.SpawnStarted)
                return false;

            CheckSpawn(from, dropped);

            return base.TryDropItem(from, dropped, sendFullMessage);
        }


        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (m_Spawner == null || !(item.GetType().Equals(m_RitualItem)) || m_Spawner.RechargeDelay > DateTime.Now || m_Spawner.SpawnStarted)
                return false;

            CheckSpawn(from, item);

            return base.OnDragDropInto(from, item, p);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendMessage("You are unable to open this due a strange magic");
        }
        #endregion Overrides

        #region Serialization
        /// <summary>
        /// Allows Saving/Loading
        /// </summary>
        /// <param name="serial"></param>
        public TPRitualContainer(Serial serial)
            : base(serial)
        {
        }

        /// <summary>
        /// Saves info
        /// </summary>
        /// <param name="writer"></param>
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 ); // version 

            writer.Write((string)m_RitualItem.Name);
            writer.Write((int)m_RitualAmount);
            writer.Write((int)m_Spawner.Serial);
		}

        /// <summary>
        /// Loads info
        /// </summary>
        /// <param name="reader"></param>
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

            m_RitualItem = SpawnerType.GetType(reader.ReadString());
            m_RitualAmount = reader.ReadInt();

            Serial serial = reader.ReadInt();
            PSpawner = World.FindItem(serial);
        }
        #endregion Serialization
    }
}
