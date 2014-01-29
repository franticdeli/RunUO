/****************************************************************\
* File Name : TPSpawner.cs                                       *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : Controls and sets up a progressive spawner.        *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY  | Comments                                          |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Items;
using Server.Mobiles;

using ProgressionSpawner;

namespace Server.Mobiles
{
    public class TPSpawner : Item
    {
        #region Initializations
        /// <summary>
        /// Determines if the spawner is active
        /// </summary>
        private bool m_Active;

        /// <summary>
        /// Maximum amount of mobiles that can be spawned
        /// </summary>
        private int m_Count;

        /// <summary>
        /// List of the mobiles spawned
        /// </summary>
        private List<IEntity> m_Creatures;

        /// <summary>
        /// List containing the names of the mobiles to spawn
        /// </summary>
        private List<string> m_CreaturesName;

        /// <summary>
        /// The Current Progression Level that the spawener is on
        /// </summary>
        private int m_CurrLvl;

        /// <summary>
        /// Range away from the spawner considered the mobiles' home
        /// </summary>
        private int m_HomeRange;

        /// <summary>
        /// Amount of levels
        /// </summary>
        private int m_LevelAmt;

        /// <summary>
        /// The levels of the spawner
        /// </summary>
        private List<ProgressionLevel> m_Levels;

        /// <summary>
        /// Time limit for each level
        /// </summary>
        private LimitTimer m_TimeLimit;

        /// <summary>
        /// Range at which a player must be for the spawner to start spawning
        /// </summary>
        private int m_PlayerRange;

        /// <summary>
        /// Use to determine of the spawner is recharged
        /// </summary>
        private DateTime m_RechargeDelay;

        /// <summary>
        /// Time Delay before the TPSpawner is usable again
        /// </summary>
        private TimeSpan m_RechargeTime;

        /// <summary>
        /// Timer that controls the delay between levels
        /// </summary>
        private RecupeTimer m_RecupeTimer;

        /// <summary>
        /// Determines if the spawner has started spawwning
        /// </summary>
        private bool m_SpawnStarted;

        /// <summary>
        /// Team of the spawned mobiles
        /// </summary>
        private int m_Team;

        /// <summary>
        /// Timer to allow for better spawning
        /// </summary>
        private SpawnerTimer m_Timer;

        /// <summary>
        /// Determines whether the Ritual Container should be used
        /// </summary>
        private bool m_UseRitual;

        /// <summary>
        /// Walking range for the mobiles
        /// </summary>
        private int m_WalkingRange;

        /// <summary>
        /// Determines if the wave is started
        /// </summary>
        private bool m_WaveStarted;

        /// <summary>
        /// Waypoint for the mobiles
        /// </summary>
        private WayPoint m_WayPoint;
        #endregion Initializations

        #region Admin Gets/Sets
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return m_Active; }
            set 
            { 
                m_Active = value;

                if (!m_UseRitual)
                {
                    if (m_Active)
                        m_Timer.Start();
                    else
                    {
                        m_Timer.Stop();
                        m_SpawnStarted = false;
                        m_WaveStarted = false;
                        Visible = false;
                    }
                }

                InvalidateProperties(); 
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange
        {
            get { return m_HomeRange; }
            set { m_HomeRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LevelAmt
        {
            get { return m_LevelAmt; }
            set
            {
                if (value >= 0 && value <= 10)
                    m_LevelAmt = value;
                if (m_LevelAmt < m_Levels.Count)
                    RemoveLevel(m_LevelAmt);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PlayerRange
        {
            get { return m_PlayerRange; }
            set { m_PlayerRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RechargeTime
        {
            get { return m_RechargeTime; }
            set { m_RechargeTime = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseRitual
        {
            get { return m_UseRitual; }
            set { m_UseRitual = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WalkingRange
        {
            get { return m_WalkingRange; }
            set { m_WalkingRange = value; InvalidateProperties(); }
        }
        #endregion Admin Gets/Sets

        #region Public Variables
        public List<string> CreaturesName
        {
            get { return m_CreaturesName; }
            set
            {
                m_CreaturesName = value;
                if (m_CreaturesName.Count < 1)
                    Active = false;

                InvalidateProperties();
            }
        }

        public List<IEntity> Creatures
        {
            get { return m_Creatures; }
        }

        public DateTime RechargeDelay
        {
            get { return m_RechargeDelay; }
            set { m_RechargeDelay = value; }
        }

        public int CurrentLevel
        {
            get { return m_CurrLvl; }
            set { m_CurrLvl = value; InvalidateProperties(); }
        }

        public bool SpawnStarted
        {
            get { return m_SpawnStarted; }
            set { m_SpawnStarted = value; }
        }

        public bool WaveStarted
        {
            get { return m_WaveStarted; }
            set { m_WaveStarted = value; }
        }
        #endregion Public Variables

        /// <summary>
        /// Constructor
        /// </summary>
        [Constructable]
        public TPSpawner()
            : base(0xF6C) // 0xF6C = Moongate, 0x1f13 = Generic Spawner
        {
            Visible = false;
            Movable = false;
            Hue = 1175;

            m_Active = false;
            m_Creatures = new List<IEntity>();
            m_CreaturesName = new List<string>();
            m_CurrLvl = 1;
            m_HomeRange = 5;
            m_LevelAmt = 3;
            m_Levels = new List<ProgressionLevel>();
            m_PlayerRange = 10;
            m_RechargeDelay = DateTime.Now;
            m_RechargeTime = new TimeSpan(0, 1, 0);
            m_SpawnStarted = false;            
            m_Timer = new SpawnerTimer(this);
            m_UseRitual = false;
            m_WalkingRange = -1;
        }

        /// <summary>
        /// Used to keep the integrity of the spawner
        /// </summary>
        public void Defrag()
        {
            bool removed = false;

            for (int i = 0; i < m_Creatures.Count; ++i)
            {
                IEntity e = m_Creatures[i];

                if (e is Item)
                {
                    Item item = (Item)e;

                    if (item.Deleted || item.Parent != null)
                    {
                        m_Creatures.RemoveAt(i);
                        --i;
                        removed = true;
                    }
                }
                else if (e is Mobile)
                {
                    Mobile m = (Mobile)e;

                    if (m.Deleted)
                    {
                        m_Creatures.RemoveAt(i);
                        --i;
                        removed = true;
                    }
                    else if (m is BaseCreature)
                    {
                        BaseCreature bc = (BaseCreature)m;
                        if (bc.Controlled || bc.IsStabled)
                        {
                            m_Creatures.RemoveAt(i);
                            --i;
                            removed = true;
                        }
                    }
                }
                else
                {
                    m_Creatures.RemoveAt(i);
                    --i;
                    removed = true;
                }
            }

            if (removed)
                InvalidateProperties();
        }

        #region Spawner Timing Methods
        /// <summary>
        /// Starts the main SpawnerTimer
        /// Meant to be used by a Ritual Object
        /// </summary>
        public void Start()
        {
            if(m_Active)
                m_Timer.Start();
        }

        /// <summary>
        /// Starts the spawn if a player is within range
        /// </summary>
        public void OnTick()
        {
            // Check if the spawner is recharging
            if (DateTime.Now < RechargeDelay)
                return;

            if (!UseRitual)
            {
                if (!SpawnStarted)
                {
                    foreach (Mobile m in GetMobilesInRange(PlayerRange))
                    {
                        if (m is PlayerMobile && (m.AccessLevel == AccessLevel.Player || (m.AccessLevel > AccessLevel.Player && !m.Hidden)))
                        {
                            InitiateSpawn();
                            break;
                        }
                    }
                }
                else
                    NextWave();
            }
            else if(m_Active)
            {
                if (!SpawnStarted)
                    InitiateSpawn();
                else
                    NextWave();
            }
        }

        /// <summary>
        /// Notifies the players of the appearane of the spawn, animates the gate, and spawns the mobiles
        /// </summary>
        public void InitiateSpawn()
        {
            CurrentLevel = 1;

            if (m_Levels.Count >= CurrentLevel)
            {
                SpawnStarted = true;

                foreach (Mobile m in GetMobilesInRange(PlayerRange))
                {
                    m.SendMessage("You sense a strange presence nearby.");
                }

                AnimateGate();
                Spawn();
            }
            else if (m_Levels.Count == 0)
            {
                Active = false;
                SpawnStarted = false;
                Console.WriteLine(String.Format("Uninitialized Progressive Spawner at X:{0} Y:{1} on Map:{2}. Deactivating.", Location.X, Location.Y, Map));
            }
        }

        /// <summary>
        /// Spawns the Next Wave if it exists and if previous wave was successful
        /// </summary>
        public void NextWave()
        {
            if (SuccessfulWave() && !m_WaveStarted)
            {
                CurrentLevel++;

                if (m_Levels.Count >= CurrentLevel)
                {
                    m_WaveStarted = true;
                    m_RecupeTimer = new RecupeTimer(TimeSpan.FromSeconds(3), this);
                    m_RecupeTimer.Start();
                }
                else
                {
                    RechargeDelay = DateTime.Now + RechargeTime;
                    SpawnStarted = false;

                    foreach (Mobile m in GetMobilesInRange(PlayerRange))
                    {
                        m.SendMessage("The strange portal closes as the final enemy is slain.");
                    }

                    Visible = false;

                    if (UseRitual)
                        m_Timer.Stop();
                }
            }
        }

        /// <summary>
        /// Animates the appearance of the gate
        /// </summary>
        private void AnimateGate()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
            Effects.PlaySound(Location, Map, 0x225);
            Visible = true;
        }

        /// <summary>
        /// Returns whether or not the players were successful in completing the current level
        /// </summary>
        /// <returns>true if successful</returns>
        public bool SuccessfulWave()
        {
            bool m_SuccessfulWave = true;

            foreach (Mobile m in Creatures)
            {
                if (m.Alive)
                {
                    m_SuccessfulWave = false;
                    break;
                }
            }
            return m_SuccessfulWave;
        }
        #endregion Spawner Timing Methods

        #region Overrides
        /// <summary>
        /// Changes the default name of the item
        /// </summary>
        public override string DefaultName
        {
            get { return "TPSpawner"; }
        }

        /// <summary>
        /// Sets the property information
        /// </summary>
        /// <param name="list">property info</param>
        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Active)
            {
                list.Add(1060742); // active

                list.Add(1060656, m_Count.ToString()); // amount to make: ~1_val~
                list.Add(1061169, m_HomeRange.ToString()); // range ~1_val~
                list.Add(1060658, "walking range\t{0}", m_WalkingRange); // ~1_val~: ~2_val~ 
                list.Add(1060660, "team\t{0}", m_Team); // ~1_val~: ~2_val~
            }
            else
            {
                list.Add(1060743); // inactive
            }
        }

        /// <summary>
        /// Displays whether the spawner is active upon a single click
        /// </summary>
        /// <param name="from">mobile to send the label to</param>
        public override void OnSingleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                base.OnSingleClick(from);

                if (Active)
                    LabelTo(from, "[Active]");
                else
                    LabelTo(from, "[Inactive]");
            }
            else
                LabelTo(from, "a transdimensional portal");
        }

        /// <summary>
        /// Opens the gump for the administration to choose the progression level
        /// mobiles and amounts
        /// </summary>
        /// <param name="from">mobile so display the gump to</param>
        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel < AccessLevel.GameMaster)
                return;

            TPSpawnerGump g = new TPSpawnerGump(this);
            from.SendGump(g);
        }

        /// <summary>
        /// Removes the mobiles spawned if this object is deleted
        /// </summary>
        public override void OnDelete()
        {
            base.OnDelete();

            RemoveCreatures();

            if(m_TimeLimit != null)
                m_TimeLimit.Stop();
            if(m_Timer != null)
                m_Timer.Stop();
        }
        #endregion Overrides

        #region Progression Level Manipulation
        /// <summary>
        /// Adds or changes a progression level
        /// </summary>
        /// <param name="lvl">level to be added/changed</param>
        /// <param name="names">list of mobiles/items</param>
        /// <param name="amt">list of amounts for the mobiles/items</param>
        /// <param name="limit">time limit for this level</param>
        public void SetLevel(int lvl, List<string> names, List<int> amt, TimeSpan limit)
        {
            ProgressionLevel level = new ProgressionLevel();
            level.Mobs = names;
            level.MobAmts = amt;
            level.TimeLimit = limit;

            if (m_Levels.Count >= lvl)
                m_Levels[lvl - 1] = level;
            else if ((m_Levels.Count + 1) == lvl)
                m_Levels.Add(level);
            else
            {
                for (int i = m_Levels.Count; i < lvl; i++)
                {
                    if (i != (lvl-1))
                        m_Levels.Add(new ProgressionLevel());
                    else
                        m_Levels.Add(level);
                }
            }
        }

        /// <summary>
        /// Gets the mobile/item list of the specified level
        /// </summary>
        /// <param name="level">level to grab the list from</param>
        /// <returns>list of mobiles/items</returns>
        public List<string> GetLevelMobs(int level)
        {
            if (m_Levels.Count != 0 && m_Levels.Count >= level)
                return m_Levels[level - 1].Mobs;
            else
            {
                m_CreaturesName.Clear();
                return m_CreaturesName;
            }
        }

        /// <summary>
        /// Gets the amount list of the specified level
        /// </summary>
        /// <param name="level">level to grab the list from</param>
        /// <returns>list of amounts for the level</returns>
        public List<int> GetLevelAmts(int level)
        {
            if (m_Levels.Count != 0 && m_Levels.Count >= level)
                return m_Levels[level - 1].MobAmts;
            else
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Gets the levels time limit
        /// </summary>
        /// <param name="level">level to grab the time limit from</param>
        /// <returns>time limit of the level</returns>
        public TimeSpan GetLevelLimit(int level)
        {
            if (m_Levels.Count != 0 && m_Levels.Count >= level)
                return m_Levels[level - 1].TimeLimit;
            else
            {
                return new TimeSpan(0,0,0);
            }
        }

        /// <summary>
        /// Removes the level and any levels would be beyond it
        /// </summary>
        /// <param name="lvl">level to be removed</param>
        public void RemoveLevel(int lvl)
        {
            if (m_Levels.Count == lvl)
                m_Levels.RemoveAt(lvl-1);
            else if (m_Levels.Count > lvl)
            {
                m_Levels.RemoveRange(lvl-1, m_Levels.Count-lvl);
            }
        }
        #endregion Progression Level Manipulation

        #region Spawn Methods
        /// <summary>
        /// Respawn the level specified
        /// Used for debugging
        /// </summary>
        /// <param name="level">level to be respawned</param>
        public void Respawn(int level)
        {
            CurrentLevel = level;

            RemoveCreatures();

            Spawn();
        }

        /// <summary>
        /// Sets the level to be spawned and cause all of the mobiles/items
        /// in the list to be spawned
        /// </summary>
        public void Spawn()
        {
            TimeSpan limit;
            m_Count = 0;

            if (m_Levels.Count != 0 && m_Levels.Count >= CurrentLevel)
            {
                m_CreaturesName = m_Levels[CurrentLevel - 1].Mobs;
                for (int i = 0; i < m_Levels[CurrentLevel - 1].MobAmts.Count; i++)
                    m_Count += m_Levels[CurrentLevel - 1].MobAmts[i];

                limit = m_Levels[CurrentLevel - 1].TimeLimit;
                if (limit.TotalSeconds > 0)
                {
                    m_TimeLimit = new LimitTimer(limit, this);
                    m_TimeLimit.Start();
                }
                    
            }
            else
                return;

            

            for(int i = 0; i < m_CreaturesName.Count; i++)
            {
                for (int j = 0; j < m_Levels[CurrentLevel - 1].MobAmts[i]; j++)
                    Spawn(i);
            }
        }

        /// <summary>
        /// Spawns the mobile or item with the index given
        /// </summary>
        /// <param name="index">number representing the mobile or item</param>
        public void Spawn(int index)
        {
            Map map = Map;

            if (map == null || map == Map.Internal || m_CreaturesName.Count == 0 || index >= m_CreaturesName.Count || Parent != null)
                return;

            Defrag();

            if (m_Creatures.Count >= m_Count)
                return;

            Type type = SpawnerType.GetType(m_CreaturesName[index]);

            if (type != null)
            {
                try
                {
                    object o = Activator.CreateInstance(type);

                    if (o is Mobile)
                    {
                        Mobile m = (Mobile)o;

                        m_Creatures.Add(m);


                        Point3D loc = (m is BaseVendor ? this.Location : GetSpawnPosition());

                        m.OnBeforeSpawn(loc, map);
                        InvalidateProperties();


                        m.MoveToWorld(loc, map);

                        if (m is BaseCreature)
                        {
                            BaseCreature c = (BaseCreature)m;

                            if (m_WalkingRange >= 0)
                                c.RangeHome = m_WalkingRange;
                            else
                                c.RangeHome = m_HomeRange;

                            c.CurrentWayPoint = m_WayPoint;

                            if (m_Team > 0)
                                c.Team = m_Team;

                            c.Home = this.Location;
                        }

                        m.OnAfterSpawn();
                    }
                    else if (o is Item)
                    {
                        Item item = (Item)o;

                        m_Creatures.Add(item);

                        Point3D loc = GetSpawnPosition();

                        item.OnBeforeSpawn(loc, map);
                        InvalidateProperties();

                        item.MoveToWorld(loc, map);

                        item.OnAfterSpawn();
                    }
                }
                catch
                {
                }
            }

            m_WaveStarted = false;
        }

        /// <summary>
        /// Gets a position to spawn the mobiles/items
        /// </summary>
        /// <returns>spawn position</returns>
        public Point3D GetSpawnPosition()
        {
            Map map = Map;

            if (map == null)
                return Location;

            // Try 10 times to find a Spawnable location.
            for (int i = 0; i < 10; i++)
            {
                int x = Location.X;// +(Utility.Random((m_HomeRange * 2) + 1) - m_HomeRange);
                int y = Location.Y;// +(Utility.Random((m_HomeRange * 2) + 1) - m_HomeRange);
                int z = Map.GetAverageZ(x, y);

                if (Map.CanSpawnMobile(new Point2D(x, y), this.Z))
                    return new Point3D(x, y, this.Z);
                else if (Map.CanSpawnMobile(new Point2D(x, y), z))
                    return new Point3D(x, y, z);
            }

            return this.Location;
        }
        #endregion Spawn Methods

        #region Creature Manipulation
        /// <summary>
        /// Counts and return how many of a specified mobile has 
        /// been spawned by this spawner
        /// </summary>
        /// <param name="creatureName">specified mobile</param>
        /// <returns>amount of the specified mobile</returns>
        public int CountCreatures(string creatureName)
        {
            Defrag();

            int count = 0;

            for (int i = 0; i < m_Creatures.Count; ++i)
                if (Insensitive.Equals(creatureName, m_Creatures[i].GetType().Name))
                    ++count;

            return count;
        }

        /// <summary>
        /// Removes all of the mobiles specified that are spawned
        /// by this spawner
        /// </summary>
        /// <param name="creatureName">specified mobile</param>
        public void RemoveCreatures(string creatureName)
        {
            Defrag();

            for (int i = 0; i < m_Creatures.Count; ++i)
            {
                IEntity e = m_Creatures[i];

                if (Insensitive.Equals(creatureName, e.GetType().Name))
                    e.Delete();
            }

            InvalidateProperties();
        }

        /// <summary>
        /// Removes all of the mobiles spawned by this spawner
        /// </summary>
        public void RemoveCreatures()
        {
            Defrag();

            for (int i = 0; i < m_Creatures.Count; ++i)
                m_Creatures[i].Delete();

            InvalidateProperties();
        }

        /// <summary>
        /// Move the mobiles spawned by this spawner back to thier home range
        /// </summary>
        public void BringToHome()
        {
            Defrag();

            for (int i = 0; i < m_Creatures.Count; ++i)
            {
                IEntity e = m_Creatures[i];

                if (e is Mobile)
                {
                    Mobile m = (Mobile)e;

                    m.MoveToWorld(Location, Map);
                }
                else if (e is Item)
                {
                    Item item = (Item)e;

                    item.MoveToWorld(Location, Map);
                }
            }
        }
        #endregion Creature Manipulation

        #region Serialization
        /// <summary>
        /// Allows the object to be saved/loaded
        /// </summary>
        /// <param name="serial"></param>
        public TPSpawner(Serial serial)
        {
        }

        /// <summary>
        /// Saves the object
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((bool)m_Active);
            writer.Write((int)m_CurrLvl);
            writer.Write((int)m_HomeRange);
            writer.Write((int)m_LevelAmt);
            writer.Write((int)m_PlayerRange);
            writer.Write((DateTime)m_RechargeDelay);
            writer.Write((TimeSpan)m_RechargeTime);
            writer.Write((bool)m_SpawnStarted);
            writer.Write((bool)m_UseRitual);
            writer.Write((int)m_WalkingRange);
            writer.Write((bool)m_WaveStarted);

            writer.Write(m_CreaturesName.Count);
            for (int i = 0; i < m_CreaturesName.Count; ++i)
                writer.Write((string)m_CreaturesName[i]);

            writer.Write(m_Creatures.Count);
            for (int i = 0; i < m_Creatures.Count; ++i)
            {
                object o = m_Creatures[i];

                if (o is Item)
                    writer.Write((Item)o);
                else if (o is Mobile)
                    writer.Write((Mobile)o);
                else
                    writer.Write(Serial.MinusOne);
            }

            // Hack to save the progression level info
            writer.Write((int)m_Levels.Count);
            foreach (ProgressionLevel l in m_Levels)
            {
                l.Save(writer);
            }

            if (m_RecupeTimer != null)
                m_RecupeTimer.Save(writer);
            else
            {
                m_RecupeTimer = new RecupeTimer(TimeSpan.FromSeconds(0.0), this);
                m_RecupeTimer.Save(writer);
            }

            if (m_TimeLimit != null)
                m_TimeLimit.Save(writer);
            else
            {
                m_TimeLimit = new LimitTimer(TimeSpan.FromSeconds(0.0), this);
                m_TimeLimit.Save(writer);
            }
            
        }

        /// <summary>
        /// Loads the object
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            m_Active = reader.ReadBool();
            m_CurrLvl = reader.ReadInt();
            m_HomeRange = reader.ReadInt();
            m_LevelAmt = reader.ReadInt();
            m_PlayerRange = reader.ReadInt();
            m_RechargeDelay = reader.ReadDateTime();
            m_RechargeTime = reader.ReadTimeSpan();
            m_SpawnStarted = reader.ReadBool();
            m_UseRitual = reader.ReadBool();
            m_WalkingRange = reader.ReadInt();
            m_WaveStarted = reader.ReadBool();

            int size = reader.ReadInt();
            m_CreaturesName = new List<string>();
            for (int i = 0; i < size; ++i)
            {
                string typeName = reader.ReadString();
                m_CreaturesName.Add(typeName);
            }

            int count = reader.ReadInt();
            m_Creatures = new List<IEntity>();
            for (int i = 0; i < count; ++i)
            {
                IEntity e = World.FindEntity(reader.ReadInt());
                if (e != null)
                    m_Creatures.Add(e);
            }

            // Hack to load the progression level info
            m_Levels = new List<ProgressionLevel>();
            int amt = reader.ReadInt();
            for (int i = 0; i < amt; i++)
            {
                ProgressionLevel lvl = new ProgressionLevel();
                lvl.Load(reader);
                m_Levels.Add(lvl);
            }

            m_Timer = new SpawnerTimer(this);
            if (m_Active && !m_UseRitual)
                m_Timer.Start();

            DateTime recupe = reader.ReadDateTime();
            if (recupe > DateTime.Now)
                m_RecupeTimer = new RecupeTimer(recupe.Subtract(DateTime.Now),this);

            DateTime limit = reader.ReadDateTime();
            if (limit > DateTime.Now)
                m_TimeLimit = new LimitTimer(limit.Subtract(DateTime.Now), this);
        }
        #endregion Serialization

        #region SpawnerTimer
        /****************************************************************\
        * Developer : Taryen(Mark S.)                                    *
        * Orig. Date: 03/27/12                                           *
        * Desc.     : Simply Calls the Spawner's OnTick method to allow  *
        *             for better spawning.                               *
        \****************************************************************/

        /************************  Changelog  ***************************\
        |    Date    |                    Changes                        |
        ------------------------------------------------------------------
        |  MM/DD/YY  | Comments                                          |
        \****************************************************************/
        private class SpawnerTimer : Timer
        {
            /// <summary>
            /// TPSpawner tied this timer is tied to
            /// </summary>
            TPSpawner m_Spawner;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="spawner">TPSpawner tied this timer is tied to</param>
            public SpawnerTimer( TPSpawner spawner)
                : base(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            {
                m_Spawner = spawner;
            }

            /// <summary>
            /// Action to be taken upon each tick of the timer
            /// </summary>
            protected override void OnTick()
            {
                m_Spawner.OnTick();
            }
        }
        #endregion SpawnerTimer

        #region LimitTimer
        /****************************************************************\
        * Developer : Taryen(Mark S.)                                    *
        * Orig. Date: 03/27/12                                           *
        * Desc.     : Defines the limit timer class object               *
        *             Limits the amount of time the players have to      *
        *             finish the current progression level. If they      *
        *             succeed in the current progression level, the      *
        *             next level will then be started.                   *
        \****************************************************************/

        /************************  Changelog  ***************************\
        |    Date    |                    Changes                        |
        ------------------------------------------------------------------
        |  MM/DD/YY  | Comments                                          |
        \****************************************************************/
        private class LimitTimer : Timer
        {
            /// <summary>
            /// Time limit for the level
            /// </summary>
            DateTime m_EndTime;

            /// <summary>
            /// Amount of time for recuperating
            /// </summary>
            TPSpawner m_Spawner;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="limit">Time limit for the level</param>
            /// <param name="spawner">TPSpawner tied this timer is tied to</param>
            public LimitTimer(TimeSpan limit, TPSpawner spawner)
                : base(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            {
                m_EndTime = DateTime.Now + limit;
                m_Spawner = spawner;
            }

            /// <summary>
            /// Action to be taken upon each tick of the timer
            /// </summary>
            protected override void OnTick()
            {
                if (DateTime.Now > m_EndTime)
                {
                    if (!m_Spawner.SuccessfulWave())
                    {
                        m_Spawner.SpawnStarted = false;
                        m_Spawner.RechargeDelay = DateTime.Now + m_Spawner.RechargeTime;

                        foreach (Mobile m in m_Spawner.GetMobilesInRange(m_Spawner.PlayerRange))
                        {
                            m.SendMessage("The enemy has abandoned the fight and has closed the strange portal.");
                        }

                        m_Spawner.Visible = false;
                    }

                    Stop();
                }
            }

            #region Save
            /// <summary>
            /// Saves the object
            /// </summary>
            /// <param name="writer">writer that saves the info</param>
            public virtual void Save(GenericWriter writer)
            {
                writer.Write((DateTime)m_EndTime);
            }
            #endregion Save
        }
        #endregion LimitTimer

        #region RecupeTimer
        /****************************************************************\
        * Developer : Taryen(Mark S.)                                    *
        * Orig. Date: 03/27/12                                           *
        * Desc.     : Defines the limit timer class object               *
        *             Limits the amount of time the players have to      *
        *             finish the current progression level. If they      *
        *             succeed in the current progression level, the      *
        *             next level will then be started.                   *
        \****************************************************************/

        /************************  Changelog  ***************************\
        |    Date    |                    Changes                        |
        ------------------------------------------------------------------
        |  MM/DD/YY  | Comments                                          |
        \****************************************************************/
        private class RecupeTimer : Timer
        {
            /// <summary>
            /// Amount of time for recuperating
            /// </summary>
            DateTime m_EndTime;

            /// <summary>
            /// TPSpawner tied this timer is tied to
            /// </summary>
            TPSpawner m_Spawner;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="delay">Amount of time for recuperating</param>
            /// <param name="spawner">TPSpawner tied this timer is tied to</param>
            public RecupeTimer(TimeSpan delay, TPSpawner spawner)
                : base(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            {
                m_EndTime = DateTime.Now + delay;
                m_Spawner = spawner;

                foreach (Mobile m in m_Spawner.GetMobilesInRange(m_Spawner.PlayerRange))
                {
                    if (m_Spawner.CurrentLevel == m_Spawner.LevelAmt)
                        m.SendMessage("The enemy is preparing its final attack...");
                    else
                        m.SendMessage("The enemy is gathering strength...");
                }
            }

            /// <summary>
            /// Action to be taken upon each tick of the timer
            /// </summary>
            protected override void OnTick()
            {
                if (DateTime.Now > m_EndTime)
                {
                    foreach (Mobile m in m_Spawner.GetMobilesInRange(m_Spawner.PlayerRange))
                    {
                        m.SendMessage("The enemy is upon you!");
                    }

                    m_Spawner.Spawn();
                    Stop();
                }
            }

            #region Save
            /// <summary>
            /// Saves the object
            /// </summary>
            /// <param name="writer">writer that saves the info</param>
            public virtual void Save(GenericWriter writer)
            {
                writer.Write((DateTime)m_EndTime);
            }
            #endregion Save
        }
        #endregion RecupeTimer

    }
}
