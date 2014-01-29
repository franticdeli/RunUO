/****************************************************************\
* File Name : BarrierCrystals.cs                                 *
* Developer : Taryn(Mark S.)                                     *
* Orig. Date: 02/03/13                                           *
* Desc.     : The BarrierCrystal and MinorBarrierCrystal cause   *
*             the guarded zone they are placed within to become  *
*             unguarded and despawned when they are fully        *
*             discharged. Only one BarrierCrystal is to be       *
*             placed within each guarded region.                 *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY  | Comments                                          |
\****************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Guilds;
using Server.Network;
using Server.Targeting;
using Server.Regions;
using Server.Items;
using Server.Mobiles;

namespace CityTakeover
{
    /// <summary>
    /// Main crystal of the City Takeover System.
    /// Attaches to the MinorBarrierCrystals within the determined
    /// Guarded Region, toggles the Guarded Regions Guards and NPCs,
    /// toggles whether the crystal or minor crystals can be charged,
    /// calls the spawner to spawn the monster takeover of the city.
    /// </summary>
	public class BarrierCrystal : Container
    {
        /// <summary>
        /// Used to allow to set the main barrier crystal as desecrated or sanctified.
        /// </summary>
        public enum Desecration
        {
            Normal,
            Desecrated,
            Sanctified
        }

        /// <summary>
        /// Definition for the difficulty levels of the city system
        /// </summary>
        public enum Difficulty
        {
            Novice,
            Expert,
            Master
        }

        #region Initialization
        /// <summary>
        /// The maximum charges the crystal can hold. Charges do not exceed this number
        /// </summary>
        public static readonly int MaxCharges = 2000;

        /// <summary>
        /// The total current charges of the crystal
        /// </summary>
        private int m_Charges;

        /// <summary>
        /// Determines whether or not the crystal is desecrated, sanctified, or normal
        /// </summary>
        private Desecration m_Desecrated;

        /// <summary>
        /// Difficulty of the BarrierCrystal
        /// </summary>
        private Difficulty m_Difficulty;
        
        /// <summary>
        /// The rate at which the crystal discharges
        /// </summary>
        private int m_DischargeRate;

        /// <summary>
        /// The guarded region the crystal is within
        /// </summary>
        private GuardedRegion m_GuardedRegion;

        /// <summary>
        /// The guild that controls this crystal and its guardedregion
        /// </summary>
        private Guild m_ControllingGuild;

        /// <summary>
        /// Determines whether or not the crystal can be charged
        /// </summary>
        private bool m_Locked;

        /// <summary>
        /// The control that determines what spawns and spawns those mobiles
        /// </summary>
        private CitySpawner m_Spawner;

        /// <summary>
        /// The timer that uses the discharge rate to decrease the crystal charges
        /// </summary>
        private DischargeTimer m_DischargeTimer;

        /// <summary>
        /// The timer that will relock the crystal if it is fully discharged and unlocked
        /// </summary>
        private RelockTimer m_RelockTimer;

        /// <summary>
        /// The timer that determines whether the leaders of the crystal or its minor 
        /// crystals are dead. Then it unlocks the respective crystals 
        /// </summary>
        private LeaderDeathTimer m_LeaderDeathTimer;

        /// <summary>
        /// A list of all the mobiles that are spawned by the crystal's spawner
        /// </summary>
        private ArrayList m_Mobiles;

        /// <summary>
        /// A list of all the premiumspawners within the crystals guarded region
        /// </summary>
        public ArrayList CitySpawners;

        /// <summary>
        /// A list of all the minor crystals within the crystals guarded region
        /// </summary>
        public ArrayList MinorCrystals;

        /// <summary>
        /// The boss mobile that unlocks this crystal upon its death
        /// </summary>
        public Mobile General;

        /// <summary>
        /// A listing of the leader mobiles that unlock their respective minor crystals upon death
        /// </summary>
        public Dictionary<MinorBarrierCrystal, Mobile> Leaders;
        #endregion Initialization

        #region Admin Get/Set
        /// <summary>
        /// Determines whether the crystal is active and can spawn or discharge
        /// </summary>
        [CommandProperty( AccessLevel.GameMaster )]
		public bool Active
		{
            get { return ItemID != 0x2257; }
            set
            {
                if (MinorCrystals.Count > 0)
                {
                    ItemID = value ? 0x35EF : 0x2257;
                    Visible = value;

                    foreach (MinorBarrierCrystal crystal in MinorCrystals)
                    {
                        crystal.Active = value;
                    }

                    InvalidateProperties();
                    CheckTimers();
                }
            }
		}

        /// <summary>
        /// Sets and returns the total amount of charges the crystal has
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get
            {
                if (Active)
                    ItemID = m_Charges == 0 ? 14284 : 0x35EF;

                return m_Charges;
            }
            set
            {
                m_Charges = value;

                if (Active)
                    ItemID = m_Charges == 0 ? 14284 : 0x35EF;

                InvalidateProperties();
                CheckTimers();
            }
        }

        /// <summary>
        /// Sets and gets whether the crystal is desecrated, sanctified, or normal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public Desecration Desecrated
        {
            get { return m_Desecrated; }
            set { m_Desecrated = value; }
        }

        /// <summary>
        /// Sets and gets whether the difficulty of the crystal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public Difficulty SpawnDifficulty
        {
            get { return m_Difficulty; }
            set 
            { 
                m_Difficulty = value;
                m_Spawner = new CitySpawner(this);
            }
        }

        /// <summary>
        /// Sets and gets the crystal's charge rate
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public int DischargeRate
        {
            get { return m_DischargeRate; }
            set
            {
                m_DischargeRate = value;
                InvalidateProperties();
            }
        }

        /// <summary>
        /// Gets the crystal's Guarded Region
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public GuardedRegion GRegion
        {
            get { return m_GuardedRegion; }
        }

        /// <summary>
        /// Sets and gets the guild that controls this crystal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public Guild ControllingGuild
        {
            get { return m_ControllingGuild; }
            set { m_ControllingGuild = value; }
        }

        /// <summary>
        /// Sets and gets the crystal's locked condition
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Locked
        {
            get { return m_Locked; }
            set
            {
                m_Locked = value;
                CheckTimers();
                InvalidateProperties();
            }
        }

        /// <summary>
        /// Gets whether the crystal is recharging
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Discharging
        {
            get { return m_DischargeTimer.Running; }
        }

        /// <summary>
        /// Gets whether the crystal is relocking
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Relocking
        {
            get { return m_RelockTimer.Running; }
        }

        /// <summary>
        /// Gets whether the crystal is checking if the leaders are alive
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool CheckingDeaths
        {
            get { return m_LeaderDeathTimer.Running; }
        }

        /// <summary>
        /// Sets and gets the Spawner for the crystal
        /// </summary>
        public CitySpawner CSpawner
        {
            get { return m_Spawner; }
            set { m_Spawner = value; }
        }
		#endregion Admin Get/Set

        #region Constructor
        /// <summary>
        /// Constructor Method:
        /// Creates the BarrierCrystal item
        /// </summary>
		[Constructable]
        public BarrierCrystal()
            : base(0x2257)
		{
            Name = "a barrier crystal";
            Hue = 1151;
            Light = LightType.Circle150;
            Movable = false;
            Visible = false;

            m_Charges = 100;
            m_Desecrated = Desecration.Normal;
            m_DischargeRate = 10;
            m_Difficulty = Difficulty.Novice;
            m_Locked = false;
            m_Spawner = new CitySpawner(this);
            m_DischargeTimer = new DischargeTimer(this);
            m_RelockTimer = new RelockTimer(this);
            m_LeaderDeathTimer = new LeaderDeathTimer(this);
            m_Mobiles = new ArrayList();
            CitySpawners = new ArrayList();
            MinorCrystals = new ArrayList();
            Leaders = new Dictionary<MinorBarrierCrystal, Mobile>();
		}
        #endregion Constructor

        #region Overrides
        /// <summary>
        /// Initalizes the Guarded Region and determine the Minor Crystals and
        /// PremiumSpawners associated with it.
        /// </summary>
        public override void OnMapChange()
        {
            base.OnMapChange();

            m_GuardedRegion = (GuardedRegion)Region.Find(this.Location, this.Map).GetRegion(typeof(GuardedRegion));
            GetMinorCrystals();
            GetCitySpawners();
        }

        /// <summary>
        /// Initalizes the Guarded Region and determine the Minor Crystals and
        /// PremiumSpawners associated with it.
        /// </summary>
        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);

            m_GuardedRegion = (GuardedRegion)Region.Find(this.Location, this.Map).GetRegion(typeof(GuardedRegion));
            GetMinorCrystals();
            GetCitySpawners();
        }

        /// <summary>
        /// Removes all of the crystals minor crystls, mobiles, and stops all
        /// of the timers if they are running.
        /// </summary>
        public override void OnDelete()
        {
            if (MinorCrystals != null)
            {
                foreach (MinorBarrierCrystal cryst in MinorCrystals)
                {
                    if(cryst.MajorCrystal == this)
                        cryst.Delete();
                }
            }

            foreach(Mobile mob in m_Mobiles)
            {
                if (!mob.Deleted)
                    mob.Delete();
            }

            m_DischargeTimer.Stop();
            m_RelockTimer.Stop();
            m_LeaderDeathTimer.Stop();
            base.OnDelete();
        }

        /// <summary>
        /// Replaces the typical container label to simply be the name of the item
        /// </summary>
        /// <param name="from">Mobile to display the label to</param>
        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, Name);
        }

        /// <summary>
        /// Gives the mobile clicking a vague idea at the amount of charges the
        /// crystal current has
        /// </summary>
        /// <param name="from">Mobile to display charge level to</param>
        public override void OnDoubleClick(Mobile from)
        {
            if (Charges > 0)
            {
                if (Charges >= 2000)
                    from.SendMessage("This crystal is fully charged.");
                else if (Charges < 2000 && Charges >= 1250)
                    from.SendMessage("This crystal is charged.");
                else if (Charges < 1250 && Charges >= 750)
                    from.SendMessage("This crystal is half-charged.");
                else if (Charges < 750 && Charges >= 250)
                    from.SendMessage("This crystal needs charging.");
                else if (Charges < 250 && Charges >= 1)
                    from.SendMessage("This crystal is in desperate need of charging.");
            }
        }

        /// <summary>
        /// Simple override to ensure that the items dropped onto the crystal
        /// aren't rejected due to generic container rules
        /// </summary>
        /// <param name="m"></param>
        /// <param name="item"></param>
        /// <param name="message"></param>
        /// <param name="checkItems"></param>
        /// <param name="plusItems"></param>
        /// <param name="plusWeight"></param>
        /// <returns></returns>
        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            return true;
        }

        /// <summary>
        /// Checks if the item dropped upon the crystal can be used to recharge it.
        /// If it can, the item dropped is consumed and the crystal is charged
        /// </summary>
        /// <param name="from">Mobile dropping the item</param>
        /// <param name="dropped">Item that is being dropped</param>
        /// <param name="sendFullMessage">unused</param>
        /// <returns></returns>
        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (Locked)
            {
                from.SendMessage("Dark energies repel the item.");
                return false;
            }

            if (dropped != null && dropped.VerifyMove(from))
            {
                BarrierCrystalRechargeInfo info = BarrierCrystalRechargeInfo.Get(dropped.GetType(), Desecrated == Desecration.Desecrated);

                if (info != null)
                {
                    if (Charges >= MaxCharges)
                    {
                        from.SendLocalizedMessage(500678); // This crystal is already fully charged.
                    }
                    else
                    {
                        dropped.Consume();

                        if (Charges + info.Amount >= MaxCharges)
                        {
                            Charges = MaxCharges;
                            from.SendLocalizedMessage(500679); // You completely recharge the crystal.
                        }
                        else
                        {
                            Charges += info.Amount;
                            if (ItemID != 14284)
                                from.SendLocalizedMessage(500680); // You recharge the crystal.
                            else
                                from.SendMessage("The crystal reforms as it absorbs the gem."); 
                        }
                    }

                    return true;
                }
                else if (dropped is Skull)
                {
                    Skull sk = (Skull)dropped;
                    if (sk.Desecrated)
                    {
                        Desecrated = Desecration.Desecrated;
                        ToggleGuards(true);
                        sk.Consume();
                        from.SendMessage("The dark action poisons the crystal.");
                        from.SendMessage("The city guards vanish.");

                        return true;
                    }
                }
                else if (dropped is HugeSkull)
                {
                    HugeSkull sk = (HugeSkull)dropped;
                    if (sk.Desecrated)
                    {
                        Desecrated = Desecration.Desecrated;
                        ToggleGuards(true);
                        sk.Consume();
                        from.SendMessage("The dark action poisons the crystal.");
                        from.SendMessage("The city guards vanish.");

                        return true;
                    }
                }
            }

            from.SendLocalizedMessage(500681); // You cannot use this crystal on that.
            return false;
        }
        #endregion Overrides

        #region Misc
        /// <summary>
        /// Toggles if the guards are active within the crystals guarded region
        /// </summary>
        /// <param name="disabled">true to disable the guards</param>
        public void ToggleGuards(bool disabled)
        {
            if (GRegion != null)
            {
                GRegion.Disabled = disabled;
            }
        }

        /// <summary>
        /// Toggles the city's guards and the PremiumSpawner
        /// If the guards are disabled, the PremiumSpawners are turned off and
        /// their mobiles are removed
        /// If the guards are enabled, the PremiumSpawners are turned on and
        /// their mobiles are respawned
        /// </summary>
        /// <param name="disable">true to disable the guards</param>
        public void ToggleCity(bool disable)
        {
            if (GRegion != null)
            {
                GRegion.Disabled = disable;
                foreach (PremiumSpawner spawn in CitySpawners)
                {
                    spawn.Running = !GRegion.Disabled;
                    if (spawn.Running)
                        spawn.Respawn();
                    else
                    {
                        foreach (string str in spawn.CreaturesName)
                        {
                            spawn.RemoveCreatures(str);
                        }
                        foreach (string str in spawn.SubSpawnerA)
                        {
                            spawn.RemoveCreaturesA(str);
                        }
                        foreach (string str in spawn.SubSpawnerB)
                        {
                            spawn.RemoveCreaturesB(str);
                        }
                        foreach (string str in spawn.SubSpawnerC)
                        {
                            spawn.RemoveCreaturesC(str);
                        }
                        foreach (string str in spawn.SubSpawnerD)
                        {
                            spawn.RemoveCreaturesD(str);
                        }
                        foreach (string str in spawn.SubSpawnerE)
                        {
                            spawn.RemoveCreaturesE(str);
                        }
                    }
                }

                if (GRegion.Disabled)
                {
                    foreach (Mobile mob in GRegion.GetMobiles())
                    {
                        if (mob is PlayerMobile)
                            mob.SendMessage(String.Format("The citizens of {0} flee as the city is overrun.", GRegion.Name));
                    }
                }
                else
                {
                    foreach (Mobile mob in GRegion.GetMobiles())
                    {
                        if (mob is PlayerMobile)
                            mob.SendMessage(String.Format("The citizens of {0} return as the city's barrier powers on.", GRegion.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Finds all of the minor crystals within the guarded region and
        /// places them in the list MinorCrystals
        /// </summary>
        private void GetMinorCrystals()
        {
            if (GRegion != null)
            {
                ArrayList duplicates = new ArrayList();
                foreach (Rectangle3D r in GRegion.Area)
                {
                    Rectangle2D rect = new Rectangle2D(r.Start, r.End);
                    foreach (Item item in Map.GetItemsInBounds(rect))
                    {
                        if (item is MinorBarrierCrystal)
                        {
                            MinorBarrierCrystal cryst = item as MinorBarrierCrystal;
                            cryst.MajorCrystal = this;
                            if(!MinorCrystals.Contains(cryst))
                                MinorCrystals.Add(cryst);
                        }
                        else if(item is BarrierCrystal && item != this)
                        {
                            duplicates.Add(item);
                        }
                    }
                }

                foreach(Item item in duplicates)
                {
                    item.Delete();
                }
            }
        }

        /// <summary>
        /// Finds all of the premium spawners within the guarded region and
        /// places them in the list CitySpawners
        /// </summary>
        private void GetCitySpawners()
        {
            if (GRegion != null)
            {
                foreach (Rectangle3D r in GRegion.Area)
                {
                    Rectangle2D rect = new Rectangle2D(r.Start, r.End);
                    foreach (Item item in Map.GetItemsInBounds(rect))
                    {
                        if (item is PremiumSpawner)
                        {
                            if(!CitySpawners.Contains(item))
                                this.CitySpawners.Add(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Triggers the Spawner's spawn methods for the barrier crystal
        /// Spawns the General and a set amount of minions
        /// </summary>
        public void CrystalSpawn()
        {
            ArrayList minions;
            if (CSpawner != null)
            {
                minions = CSpawner.SpawnMinions(1, this);
                General = CSpawner.SpawnGeneral(this);
            }
            else
            {
                CSpawner = new CitySpawner(this);
                minions = CSpawner.SpawnMinions(1, this);
                General = CSpawner.SpawnGeneral(this);
            }

            m_Mobiles.AddRange(minions);
            m_Mobiles.Add(General);
        }

        /// <summary>
        /// Triggers the Spawner's spawn methods for the minor barrier crystal
        /// Spawns the minor crystal's leader and a set amount of minions
        /// </summary>
        /// <param name="crystal">MinorBarrierCrystal to spawn to</param>
        public void MinorCrystalSpawn(MinorBarrierCrystal crystal)
        {
            ArrayList minions = CSpawner.SpawnMinions(1, crystal);
            if (!Leaders.ContainsKey(crystal))
                Leaders.Add(crystal, CSpawner.SpawnCaptain(crystal));
            else
                Leaders[crystal] = CSpawner.SpawnCaptain(crystal);

            m_Mobiles.AddRange(minions);
            m_Mobiles.Add(Leaders[crystal]);
        }

        /// <summary>
        /// Checks whether the the General or one of the Leaders is still
        /// alive. Returns true if all of the leaders and the General are dead
        /// </summary>
        /// <returns></returns>
        public bool LeadersDead()
        {
            bool AllDead = false;
            ArrayList unlocked = new ArrayList();
            foreach (MinorBarrierCrystal cryst in Leaders.Keys)
            {
                Mobile m;
                Leaders.TryGetValue(cryst, out m);
                if (!m.Alive && cryst.Locked)
                {
                    cryst.Locked = false;
                    if (General == null)
                        cryst.StartRelock();
                    unlocked.Add(cryst);
                }
            }

            if (General != null && !General.Alive && unlocked.Count == Leaders.Count )
            {
                Locked = false;
                General = null;
                Leaders.Clear();
                AllDead = true;
            }

            if (Leaders.Count > 0)
                foreach (MinorBarrierCrystal cryst in unlocked)
                    Leaders.Remove(cryst);

            return AllDead;
        }

        /// <summary>
        /// Checks to see if any of the timers should be running
        /// Also, ensures that more than one timer isn't running
        /// </summary>
        public void CheckTimers()
        {
            if (Active)
            {
                if (Charges > 0)
                {
                    bool beginDischarge = false;
                    ArrayList minorsCharged = new ArrayList();
                    foreach (MinorBarrierCrystal crystal in MinorCrystals)
                    {
                        if (crystal.Charges <= 0 && crystal.Active && !crystal.Relocking)
                            beginDischarge = true;
                        else
                        {
                            beginDischarge = false;
                            break;
                        }
                    }
                    foreach (MinorBarrierCrystal crystal in MinorCrystals)
                    {
                        if (crystal.Charges > 0)
                            minorsCharged.Add(true);
                        else
                            minorsCharged.Add(false);
                    }

                    if (beginDischarge && !Discharging)
                    {
                        if (m_RelockTimer.Running)
                            m_RelockTimer.Stop();
                        if (m_LeaderDeathTimer.Running)
                            m_LeaderDeathTimer.Stop();

                        m_DischargeTimer.Start();
                    }
                    else if (minorsCharged.Contains(false) && !Discharging)
                    {
                        if (m_DischargeTimer.Running)
                            m_DischargeTimer.Stop();
                        if (m_RelockTimer.Running)
                            m_RelockTimer.Stop();

                        m_LeaderDeathTimer.Start();
                    }

                    if (!minorsCharged.Contains(false) && !Discharging)
                    {
                        if (m_DischargeTimer.Running)
                            m_DischargeTimer.Stop();
                        if (m_RelockTimer.Running)
                            m_RelockTimer.Stop();
                        if (m_LeaderDeathTimer.Running)
                            m_LeaderDeathTimer.Stop();

                        ToggleCity(false);
                    }
                }
                else if (Charges <= 0)
                {
                    if (!Locked && !m_RelockTimer.Running)
                    {
                        if (m_DischargeTimer.Running)
                            m_DischargeTimer.Stop();
                        if (m_LeaderDeathTimer.Running)
                            m_LeaderDeathTimer.Stop();

                        m_RelockTimer.Start();
                    }
                    else
                    {
                        ArrayList minorsCharged = new ArrayList();
                        foreach (MinorBarrierCrystal crystal in MinorCrystals)
                        {
                            if (crystal.Charges > 0)
                                minorsCharged.Add(true);
                            else
                                minorsCharged.Add(false);
                        }
                        if (!minorsCharged.Contains(true))
                        {
                            if (m_DischargeTimer.Running)
                                m_DischargeTimer.Stop();
                            if (m_RelockTimer.Running)
                                m_RelockTimer.Stop();

                            m_LeaderDeathTimer.Start();
                        }
                    }
                }
            }

        }
        #endregion Misc

        #region Private Classes
        /// <summary>
        /// Discharges the Crystal
        /// When fully discharged, spawns the crystals mobiles and its
        /// minor crystals mobiles
        /// </summary>
        private class DischargeTimer : Timer
        {
            /// <summary>
            /// Crystal that is to be discharged and spawned
            /// </summary>
            private BarrierCrystal m_Crystal;

            public DischargeTimer(BarrierCrystal crystal)
                : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                m_Crystal = crystal;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                if (m_Crystal.Active && !m_Crystal.m_LeaderDeathTimer.Running)
                {
                    if(m_Crystal.Charges > 0)
                        m_Crystal.Charges -= m_Crystal.DischargeRate;

                    if (m_Crystal.Charges <= 0 && !m_Crystal.Locked)
                    {
                        m_Crystal.Charges = 0;
                        m_Crystal.Locked = true;
                        Effects.SendLocationParticles(EffectItem.Create(m_Crystal.Location, m_Crystal.Map, EffectItem.DefaultDuration), 0x3728, 10, 30, 5052);
                        Effects.SendLocationParticles(EffectItem.Create(m_Crystal.Location, m_Crystal.Map, EffectItem.DefaultDuration), 0x374A, 10, 30, 5052);
                        Effects.PlaySound(m_Crystal.Location, m_Crystal.Map, 0x666);
                        m_Crystal.ToggleCity(true);
                        m_Crystal.CrystalSpawn();
                        foreach (MinorBarrierCrystal cryst in m_Crystal.MinorCrystals)
                        {
                            m_Crystal.MinorCrystalSpawn(cryst);
                        }
                        m_Crystal.CheckTimers();
                        this.Stop();
                    }
                }
                else
                    this.Stop();
            }
        }

        /// <summary>
        /// Relocks the Crystal
        /// The Crystal is discharged but has not been recharged
        /// This timer will relock the crystal and respawn its mobiles
        /// when the timer is ended
        /// </summary>
        private class RelockTimer : Timer
        {
            /// <summary>
            /// Crystal to be relocked and respawned
            /// </summary>
            private BarrierCrystal m_Crystal;

            public RelockTimer(BarrierCrystal crystal)
                : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                m_Crystal = crystal;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Crystal.Locked = true;
                m_Crystal.CrystalSpawn();
                m_Crystal.CheckTimers();
                this.Stop();
            }
        }

        /// <summary>
        /// Short timed timer to determine if the leaders of the main
        /// crystal and minor crystals are dead.
        /// If they are dead, the respective crystal is unlocked and
        /// its relock timer is started
        /// </summary>
        private class LeaderDeathTimer : Timer
        {
            /// <summary>
            /// Crystal that contains the mobile information
            /// </summary>
            private BarrierCrystal m_Crystal;

            public LeaderDeathTimer(BarrierCrystal crystal)
                : base(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1))
            {
                m_Crystal = crystal;
                Priority = TimerPriority.TenMS;
            }

            protected override void OnTick()
            {
                bool alldead = m_Crystal.LeadersDead();
                if (alldead)
                {
                    m_Crystal.CheckTimers();
                    this.Stop();
                }
            }
        }
        #endregion Private Classes

        #region Serialization
        /// <summary>
        /// Used by the core to serialize and deserialize the 
        /// specific instances of this item
        /// </summary>
        /// <param name="serial"></param>
        public BarrierCrystal(Serial serial) : base(serial)
        {
        }

        /// <summary>
        /// Saves the settings of the specific instances of this
        /// item
        /// </summary>
        /// <param name="writer"></param>
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version

			writer.WriteEncodedInt( m_Charges );
            writer.Write((int)m_Desecrated);
            writer.Write((int)m_Difficulty);
            writer.WriteEncodedInt( m_DischargeRate);
            writer.WriteGuild<Guild>(m_ControllingGuild);
            writer.Write((bool)m_Locked);
            writer.WriteMobileList(m_Mobiles);
            writer.WriteItemList(MinorCrystals);
            writer.WriteItemList(CitySpawners);
		}

        /// <summary>
        /// Loads the settings of the specific instances of this item
        /// </summary>
        /// <param name="reader"></param>
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();

			m_Charges = reader.ReadEncodedInt();
            m_Desecrated = (Desecration)reader.ReadInt();
            m_Difficulty = (Difficulty)reader.ReadInt();
            m_DischargeRate = reader.ReadEncodedInt();
            m_ControllingGuild = (Guild)reader.ReadGuild();
            m_Locked = reader.ReadBool();
            m_Mobiles = reader.ReadMobileList();
            MinorCrystals = reader.ReadItemList();
            CitySpawners = reader.ReadItemList();

            m_GuardedRegion = (GuardedRegion)Region.Find(this.Location, this.Map).GetRegion(typeof(GuardedRegion));
            m_Spawner = new CitySpawner(this);
            m_DischargeTimer = new DischargeTimer(this);
            m_RelockTimer = new RelockTimer(this);
            m_LeaderDeathTimer = new LeaderDeathTimer(this);
            Leaders = new Dictionary<MinorBarrierCrystal, Mobile>();
            CheckTimers();
        }
        #endregion Serialization
    }

    /// <summary>
    /// Used to spread out the monster takeover of the city. These
    /// are linked and controlled by the BarrierCrystal.
    /// </summary>
    public class MinorBarrierCrystal : Container
    {
        #region Initialization
        /// <summary>
        /// The maximum charges the crystal can have
        /// </summary>
        public static readonly int MaxCharges = 2000;

        /// <summary>
        /// The BarrierCrystal this crystals is linked to
        /// </summary>
        private BarrierCrystal m_MajorCrystal;

        /// <summary>
        /// The current amount of charges of the crystal
        /// </summary>
        private int m_Charges;

        /// <summary>
        /// The discharge rate of the crystal
        /// </summary>
        private int m_DischargeRate = 10;

        /// <summary>
        /// Determines whether the crystal is unlocked and can be recharged
        /// </summary>
        private bool m_Locked;

        /// <summary>
        /// Timer used to decrease the crystal's charges
        /// </summary>
        private DischargeTimer m_DischargeTimer;

        /// <summary>
        /// Timer used to relock the crystal if it is fully discharged and unlocked
        /// </summary>
        private RelockTimer m_RelockTimer;
        #endregion Initialization

        #region Admin Get/Set
        /// <summary>
        /// Determines whether the crystal is active and can be discharged and spawned 
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return ItemID != 0x2256; }
            set
            {
                if (m_MajorCrystal != null)
                {
                    ItemID = value ? 0x1ECD : 0x2256;
                    Visible = value;

                    if (m_MajorCrystal.Active != value)
                        m_MajorCrystal.Active = value;

                    InvalidateProperties();
                    CheckTimers();
                }
            }
        }

        /// <summary>
        /// Sets and gets the total charges of the crystal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get 
            {
                if (Active)
                    ItemID = m_Charges == 0 ? 0x37BE : 0x1ECD;

                return m_Charges; 
            }
            set
            {
                bool firstcharge = false;
                if (m_Charges <= 0 && value > 0)
                    firstcharge = true;

                m_Charges = value;

                if (Active)
                    ItemID = value == 0 ? 0x37BE : 0x1ECD;

                if (firstcharge)
                    MajorCrystal.CheckTimers();

                InvalidateProperties();
                CheckTimers();
            }
        }

        /// <summary>
        /// Sets and gets the discharge rate of the crystal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public int DischargeRate
        {
            get { return m_DischargeRate; }
            set
            {
                m_DischargeRate = value;
                InvalidateProperties();
            }
        }

        /// <summary>
        /// Sets and gets the locked state of the crystal
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Locked
        {
            get{return m_Locked;}
            set
            {
                m_Locked = value;
                InvalidateProperties();
                CheckTimers();
            }
        }

        /// <summary>
        /// Gets whether the crystal is discharging
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Discharging
        {
            get { return m_DischargeTimer.Running; }
        }

        /// <summary>
        /// Gets whether the crystal is relocking itself
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Relocking
        {
            get { return m_RelockTimer.Running; }
        }

        /// <summary>
        /// Sets and gets the crystals linked BarrierCrystal
        /// </summary>
        public BarrierCrystal MajorCrystal
        {
            get { return m_MajorCrystal; }
            set
            {
                m_MajorCrystal = value;
                Active = m_MajorCrystal.Active;
            }
        }
        #endregion Admin Get/Set

        #region Constructor
        /// <summary>
        /// Constructor Method:
        /// Creates the MinorBarrierCrystal item
        /// </summary>
        [Constructable]
        public MinorBarrierCrystal()
            : base(0x2256)
        {
            Name = "a minor barrier crystal";
            Hue = 1151;
            m_Charges = 200;
            m_Locked = false;
            Light = LightType.Circle150;
            Movable = false;
            Visible = false;
            m_DischargeTimer = new DischargeTimer(this);
            m_RelockTimer = new RelockTimer(this);
        }
        #endregion Constructor

        #region Overrides
        /// <summary>
        /// Initalizes the Guarded Region and determines the Major Crystal associated with it.
        /// </summary>
        public override void OnMapChange()
        {
            base.OnMapChange();
            GuardedRegion GRegion = (GuardedRegion)Region.Find(this.Location, this.Map).GetRegion(typeof(GuardedRegion));
            if (GRegion != null)
            {
                foreach (Rectangle3D r in GRegion.Area)
                {
                    Rectangle2D rect = new Rectangle2D(r.Start, r.End);
                    foreach (Item item in Map.GetItemsInBounds(rect))
                    {
                        if (item is BarrierCrystal)
                        {
                            BarrierCrystal cryst = item as BarrierCrystal;
                            m_MajorCrystal = cryst;
                            if (!cryst.MinorCrystals.Contains(this))
                                cryst.MinorCrystals.Add(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the typical container label to simply be the name of the item
        /// </summary>
        /// <param name="from">Mobile to display the label to</param>
        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, Name);
        }

        /// <summary>
        /// Gives the mobile clicking a vague idea at the amount of charges the
        /// crystal current has
        /// </summary>
        /// <param name="from">Mobile to display charge level to</param>
        public override void OnDoubleClick(Mobile from)
        {
            if (Charges > 0)
            {
                if (Charges >= 2000)
                    from.SendMessage("This crystal is fully charged.");
                else if (Charges < 2000 && Charges >= 1250)
                    from.SendMessage("This crystal is charged.");
                else if (Charges < 1250 && Charges >= 750)
                    from.SendMessage("This crystal is half-charged.");
                else if (Charges < 750 && Charges >= 250)
                    from.SendMessage("This crystal needs charging.");
                else if (Charges < 250 && Charges >= 1)
                    from.SendMessage("This crystal is in desperate need of charging.");
            }
        }

        /// <summary>
        /// Stops all of the timers if they are running.
        /// </summary>
        public override void OnDelete()
        {
            m_DischargeTimer.Stop();
            m_RelockTimer.Stop();
            base.OnDelete();
        }

        /// <summary>
        /// Simple override to ensure that the items dropped onto the crystal
        /// aren't rejected due to generic container rules
        /// </summary>
        /// <param name="m"></param>
        /// <param name="item"></param>
        /// <param name="message"></param>
        /// <param name="checkItems"></param>
        /// <param name="plusItems"></param>
        /// <param name="plusWeight"></param>
        /// <returns></returns>
        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            return true;
        }

        /// <summary>
        /// Checks if the item dropped upon the crystal can be used to recharge it.
        /// If it can, the item dropped is consumed and the crystal is charged
        /// </summary>
        /// <param name="from">Mobile dropping the item</param>
        /// <param name="dropped">Item that is being dropped</param>
        /// <param name="sendFullMessage">unused</param>
        /// <returns></returns>
        public override bool TryDropItem(Mobile from, Item given, bool sendFullMessage)
        {
            if (Locked)
            {
                from.SendMessage("Dark energies repel the item.");
                return false;
            }

            if (given != null && given.VerifyMove(from))
            {
                BarrierCrystalRechargeInfo info = BarrierCrystalRechargeInfo.Get(given.GetType(), MajorCrystal != null ? MajorCrystal.Desecrated == BarrierCrystal.Desecration.Desecrated : false);

                if (info != null)
                {
                    if (Charges >= MaxCharges)
                    {
                        from.SendLocalizedMessage(500678); // This crystal is already fully charged.
                    }
                    else
                    {
                        given.Consume();

                        if (Charges + info.Amount >= MaxCharges)
                        {
                            Charges = MaxCharges;
                            from.SendLocalizedMessage(500679); // You completely recharge the crystal.
                        }
                        else
                        {
                            Charges += info.Amount;
                            if (ItemID != 14284)
                                from.SendLocalizedMessage(500680); // You recharge the crystal.
                            else
                                from.SendMessage("The crystal reforms as it absorbs the gem."); 
                        }
                    }

                    return true;
                }
            }

            from.SendLocalizedMessage(500681); // You cannot use this crystal on that.
            return false;
        }
        #endregion Overrides

        #region Misc
        /// <summary>
        /// Checks to see if any of the timers should be running
        /// Also, ensures that more than one timer isn't running
        /// </summary>
        private void CheckTimers()
        {
            if (MajorCrystal != null)
            {
                if (m_Charges > 0)
                {
                    if (m_RelockTimer.Running)
                        m_RelockTimer.Stop();
                    if (!m_DischargeTimer.Running)
                        m_DischargeTimer.Start();
                }
                else
                {
                    if (MajorCrystal.General != null && !m_RelockTimer.Running && !Locked && Active && ItemID == 0x37BE)
                    {
                        if (m_DischargeTimer.Running)
                            m_DischargeTimer.Stop();
                        m_RelockTimer.Start();
                        MajorCrystal.CheckTimers();
                    }
                }

                if (m_RelockTimer != null && m_RelockTimer.Running && Charges > 0)
                    m_RelockTimer.Stop();
            }
        }

        /// <summary>
        /// Starts the relock timer
        /// Used to assist the BarrierCrystal start the MinorCrystal's
        /// relock timer when the General is null
        /// </summary>
        public void StartRelock()
        {
            m_RelockTimer.Start();
        }
        #endregion Misc

        #region Private Classes
        /// <summary>
        /// Discharges the Crystal
        /// When fully discharged, sends a check to begin discharging of
        /// the linked BarrierCrystal
        /// </summary>
        private class DischargeTimer : Timer
        {
            private MinorBarrierCrystal m_Crystal;

            public DischargeTimer(MinorBarrierCrystal crystal)
                : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                m_Crystal = crystal;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                if (m_Crystal.Active)
                {
                    m_Crystal.Charges -= 10;

                    if (m_Crystal.Charges <= 0)
                    {
                        m_Crystal.Charges = 0;
                        m_Crystal.Locked = true;
                        Effects.SendLocationParticles(EffectItem.Create(m_Crystal.Location, m_Crystal.Map, EffectItem.DefaultDuration), 0x377A, 10, 30, 5052);
                        Effects.PlaySound(m_Crystal.Location, m_Crystal.Map, 0x666);
                        m_Crystal.MajorCrystal.CheckTimers();
                        this.Stop();
                    }
                }
                else
                    this.Stop();
            }
        }

        /// <summary>
        /// Relocks the Crystal
        /// The Crystal is discharged but has not been recharged
        /// This timer will relock the crystal and respawn its mobiles
        /// when the timer is ended
        /// </summary>
        private class RelockTimer : Timer
        {
            private MinorBarrierCrystal m_Crystal;

            public RelockTimer(MinorBarrierCrystal crystal)
                : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
            {
                m_Crystal = crystal;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Crystal.Locked = true;
                m_Crystal.MajorCrystal.MinorCrystalSpawn(m_Crystal);
                m_Crystal.MajorCrystal.CheckTimers();
                this.Stop();
            }
        }
        #endregion Private Classes

        #region Serialization
        /// <summary>
        /// Used by the core to serialize and deserialize the 
        /// specific instances of this item
        /// </summary>
        /// <param name="serial"></param>
        public MinorBarrierCrystal(Serial serial)
            : base(serial)
        {
        }

        /// <summary>
        /// Saves the settings of the specific instances of this
        /// item
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.WriteEncodedInt(m_DischargeRate);
            writer.WriteItem<BarrierCrystal>(m_MajorCrystal);
            writer.WriteEncodedInt(m_Charges);
            writer.Write((bool)m_Locked);
        }

        /// <summary>
        /// Loads the settings of the specific instances of this item
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            DischargeRate = reader.ReadEncodedInt();
            m_MajorCrystal = reader.ReadItem<BarrierCrystal>();
            m_Charges = reader.ReadEncodedInt();
            m_Locked = reader.ReadBool();

            m_DischargeTimer = new DischargeTimer(this);
            m_RelockTimer = new RelockTimer(this);
            this.CheckTimers();
        }
        #endregion Serialization
    }
}