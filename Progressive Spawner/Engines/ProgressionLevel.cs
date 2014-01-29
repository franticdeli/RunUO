/****************************************************************\
* File Name : ProgressionLevel.cs                                * 
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : Defines the progression level class object         *
*             Contains a list of mobiles, mobile amounts, and    *
*             the time limit for these mobs to be spawned.       *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY  | Comments                                          |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;

namespace ProgressionSpawner
{
    public class ProgressionLevel
    {
        #region Initializations
        /// <summary>
        /// List of mobiles in this progression level
        /// </summary>
        private List<string> m_Mobs;

        /// <summary>
        /// The amount for each of the mobiles
        /// </summary>
        private List<int> m_MobAmts;

        /// <summary>
        /// Length of the progression level
        /// </summary>
        private TimeSpan m_TimeLimit;
        #endregion Initializations

        #region Gets/Sets
        /// <summary>
        /// Gets/Sets the progression level's time limit
        /// </summary>
        public TimeSpan TimeLimit
        {
            get { return m_TimeLimit; }
            set { m_TimeLimit = value; }
        }

        /// <summary>
        /// Gets the list of mobiles
        /// </summary>
        public List<string> Mobs
        {
            get { return m_Mobs; }
            set { m_Mobs = value; }
        }

        /// <summary>
        /// Gets the list of mobile amounts
        /// </summary>
        public List<int> MobAmts
        {
            get { return m_MobAmts; }
            set { m_MobAmts = value; }
        }
        #endregion Gets/Sets

        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressionLevel()
        {
            m_Mobs = new List<string>();
            m_MobAmts = new List<int>();
        }

        /// <summary>
        /// Adds a mobile to the m_Mobs list and sets the amount to 1
        /// </summary>
        /// <param name="m">mobile to be added</param>
        public void Add(string m)
        {
            m_Mobs.Add(m);
            m_MobAmts.Add(1);
        }

        /// <summary>
        /// Adds a mobile to the m_Mobs list and sets its amount
        /// </summary>
        /// <param name="m">mobile to be added</param>
        /// <param name="amt">amount of the mobile</param>
        public void Add(string m, int amt)
        {
            m_Mobs.Add(m);
            m_MobAmts.Add(amt);
        }

        #region Save/Load
        /// <summary>
        /// Saves the object
        /// </summary>
        /// <param name="writer">writer that saves the info</param>
        public virtual void Save(GenericWriter writer)
        {
            writer.Write((int)m_Mobs.Count);
            foreach (string m in m_Mobs)
            {
                writer.Write((string)m);
            }

            writer.Write((int)m_MobAmts.Count);
            foreach (int i in m_MobAmts)
            {
                writer.Write((int)i);
            }

            writer.Write((TimeSpan)m_TimeLimit);

        }

        /// <summary>
        /// Loads the object
        /// </summary>
        /// <param name="reader">reader that loads the info</param>
        public virtual void Load(GenericReader reader)
        {
            int size = reader.ReadInt();
            for (int i = 0; i < size; i++)
            {
                m_Mobs.Add(reader.ReadString());
            }

            size = reader.ReadInt();
            for (int i = 0; i < size; i++)
            {
                m_MobAmts.Add(reader.ReadInt());
            }

            m_TimeLimit = reader.ReadTimeSpan();
        }
        #endregion Save/Load
    }
}