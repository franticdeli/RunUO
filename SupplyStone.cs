/****************************************************************\
* File Name : SupplyStone.cs                                     *
* Developer : Taryen(Mark S.) a.k.a. Rilian or Ewok              *
* Orig. Date: 5/14/07                                            *
* Desc.     : Contains the checks to enable and spawn the        *
*             Tourney Stones to register for the tournament.     *
*             Also spawns the spectators moongates and moves     *
*             the contestants to the Tournament grounds.         *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  5/18/10   | Logic Check and Code Comments                     |
\****************************************************************/

using System;
using Server.Items;

namespace Server.Items
{
    public class SupplyStone : Item
    {
        #region Initialization & Constructor
        /// <summary>
        /// Determines usage tournament supply ruleset
        /// </summary>
        private bool m_Tourney;
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Tourney
        {
            get { return m_Tourney; }
            set { m_Tourney = value; }
        }

        /// <summary>
        /// The hue of the gear that is supplied
        /// </summary>
        private int m_EquipHue;
        [CommandProperty(AccessLevel.GameMaster)]
        public int EquipHue
        {
            get { return m_EquipHue; }
            set
            {
                m_EquipHue = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        [Constructable]
        public SupplyStone()
            : base(0xED4)
        {
            Movable = false;
        }
        #endregion Initialization & Constructor

        #region Overrides
        /// <summary>
        /// Overrides the Double-Click option on the item. Supplies the
        /// player that double-clicks on the stone with appropriate gear
        /// based upon their skill levels.
        /// </summary>
        /// <param name="m">PlayerMobile to be supplied</param>
        public override void OnDoubleClick(Mobile m)
        {
            if (m.Backpack != null)
            {
                if (((m.X) == this.X || (m.X - 1) == this.X || (m.X + 1) == this.X) && ((m.Y) == this.Y || (m.Y - 1) == this.Y || (m.Y + 1) == this.Y))
                {
                    if (!Tourney)
                    {
                        Supply(new BagOfPots(8, 8, 4, 4, 5), m, typeof(BagOfPots));
                        for (int i = 1; i <= 5; i++)
                        {
                            TrapableContainer con = new Pouch();
                            con.TrapType = TrapType.MagicTrap;
                            con.TrapLevel = 1;
                            con.TrapPower = 1;

                            if (!m.AddToBackpack(con))
                                con.Delete();
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            Item item = new TotalRefreshPotion();

                            if (!m.AddToBackpack(item))
                                item.Delete();
                        }
                    }

                    if (m.Skills[SkillName.Magery].Value >= 50.0 || m.Str <= 50)
                    {
                        EquipArmor(new LeatherLegs(), true, m, typeof(LeatherLegs), m_EquipHue);
                        EquipArmor(new LeatherGorget(), true, m, typeof(LeatherGorget), m_EquipHue);
                        EquipArmor(new LeatherGloves(), true, m, typeof(LeatherGloves), m_EquipHue);
                        EquipArmor(new LeatherChest(), true, m, typeof(LeatherChest), m_EquipHue);
                        EquipArmor(new LeatherArms(), true, m, typeof(LeatherArms), m_EquipHue);
                        EquipArmor(new LeatherCap(), true, m, typeof(LeatherCap), m_EquipHue);
                    }
                    else
                    {
                        EquipArmor(new ChainLegs(), true, m, typeof(ChainLegs), m_EquipHue);
                        EquipArmor(new LeatherGorget(), true, m, typeof(LeatherGorget), m_EquipHue);
                        EquipArmor(new RingmailGloves(), true, m, typeof(RingmailGloves), m_EquipHue);
                        EquipArmor(new ChainChest(), true, m, typeof(ChainChest), m_EquipHue);
                        EquipArmor(new RingmailArms(), true, m, typeof(RingmailArms), m_EquipHue);
                        EquipArmor(new CloseHelm(), true, m, typeof(CloseHelm), m_EquipHue);
                    }

                    if (m.Skills[SkillName.Magery].Value >= 50.0)
                    {
                        Container pack = m.Backpack;
                        if (pack.FindItemByType(typeof(Spellbook)) == null)
                        {
                            Spellbook book = new Spellbook();
                            book.Content = ulong.MaxValue;
                            book.LootType = LootType.Regular;

                            if (!m.AddToBackpack(book))
                                book.Delete();
                        }

                        Supply(new BagOfReagents(50), m, typeof(BagOfReagents));
                    }

                    if (m.Skills[SkillName.Healing].Value >= 50.0 || m.Skills[SkillName.Veterinary].Value >= 50)
                    {
                        Supply(new Bandage(50), m, typeof(Bandage));
                    }

                    if (m.Skills[SkillName.Swords].Value >= 50.0)
                    {

                        EquipItem(new Katana(), true, m, typeof(Katana));
                        EquipItem(new Halberd(), false, m, typeof(Halberd));
                        EquipItem(new BattleAxe(), false, m, typeof(BattleAxe));
                    }

                    if (m.Skills[SkillName.Fencing].Value >= 50.0)
                    {

                        EquipItem(new Kryss(), true, m, typeof(Kryss));
                        EquipItem(new ShortSpear(), false, m, typeof(ShortSpear));
                        EquipItem(new Spear(), false, m, typeof(Spear));
                    }

                    if (m.Skills[SkillName.Macing].Value >= 50.0)
                    {
                        EquipItem(new Mace(), true, m, typeof(Mace));
                        EquipItem(new QuarterStaff(), false, m, typeof(QuarterStaff));
                        EquipItem(new WarHammer(), false, m, typeof(WarHammer));
                    }

                    if (m.Skills[SkillName.Archery].Value >= 50.0)
                    {

                        EquipItem(new Bow(), false, m, typeof(Bow));
                        EquipItem(new Crossbow(), false, m, typeof(Crossbow));
                        EquipItem(new HeavyCrossbow(), false, m, typeof(HeavyCrossbow));

                        Supply(new Arrow(50), m, typeof(Arrow));
                        Supply(new Bolt(50), m, typeof(Bolt));
                    }
                }
                else
                {
                    m.SendMessage("You reach out but cannot seem to touch the stone...");
                }
            }
            else
            {
                m.SendMessage("For some reason you don't seem to have a backpack...");
            }
        }

        /// <summary>
        /// Default name of the supply stone
        /// </summary>
        public override string DefaultName
        {
            get { return "a supply stone"; }
        }
        #endregion Overrides

        #region Serialization
        /// <summary>
        /// Allows for serialization of the supply stone 
        /// </summary>
        /// <param name="serial"></param>
        public SupplyStone(Serial serial) : base(serial)
        {
        }

        /// <summary>
        /// Saves the equip hue and the tournament setting
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_EquipHue);
            writer.Write((bool)m_Tourney);
        }

        /// <summary>
        /// Loads the equip hue and the tournament setting
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_EquipHue = reader.ReadInt();
                        m_Tourney = reader.ReadBool();

                        goto case 0;
                    }
                case 0:
                    {
                        break;
                    }
            }
        }
        #endregion Serialization

        #region Checks
        /// <summary>
        /// Checks if the mobile can hold the item
        /// </summary>
        /// <param name="item">Item to be added</param>
        /// <param name="m">Mobile to add the item to</param>
        /// <param name="t">Type of the item</param>
        private static void Supply(Item item, Mobile m, Type t)
        {
            int w = m.TotalWeight;
            int mw = m.MaxWeight;
            Container pack = m.Backpack;

            if ((pack.FindItemByType(t) != null) && (w >= (mw - 25)))
                return;
            else if (!m.AddToBackpack(item))
                item.Delete();
        }

        /// <summary>
        /// Auto-equips the weapon if the mobile can equip it.
        /// Else, the item is added to the mobile backpack.
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <param name="mustEquip">Forces the item to be equipped</param>
        /// <param name="m">Mobile to be equipped</param>
        /// <param name="t">Type of the item</param>
        private static void EquipItem(BaseWeapon item, bool mustEquip, Mobile m, Type t)
        {
            Container pack = m.Backpack;
            Layer layer = item.Layer;
            Item equips = m.FindItemOnLayer(layer);

            item.Quality = WeaponQuality.Exceptional;

            if (mustEquip)
            {
                if (equips != null)
                    pack.DropItem(equips);
            }

            if (pack.FindItemByType(t) != null || equips == item)
            {
                m.EquipItem(equips);
                return;
            }
            else
            {
                if (!Core.AOS)
                {
                    item.LootType = LootType.Regular;
                }

                if (mustEquip)
                {
                    if (equips != null)
                        pack.DropItem(equips);
                }

                if (m != null && m.EquipItem(item))
                    return;

                if (!mustEquip && pack != null)
                    pack.DropItem(item);
                else
                    item.Delete();
            }
        }

        /// <summary>
        /// Auto-equips the armor if the mobile can equip it.
        /// Else, the item is added to the mobile backpack.
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <param name="mustEquip">Forces the item to be equipped</param>
        /// <param name="m">Mobile to be equipped</param>
        /// <param name="t">Type of the item</param>
        private static void EquipArmor(BaseArmor item, bool mustEquip, Mobile m, Type t, int ehue)
        {
            Container pack = m.Backpack;
            Layer layer = item.Layer;
            Item equips = m.FindItemOnLayer(layer);

            item.Quality = ArmorQuality.Exceptional;

            if (mustEquip)
            {
                if (equips != null)
                    pack.DropItem(equips);
            }

            if (pack.FindItemByType(t) != null)
            {
                m.EquipItem(equips);
                return;
            }
            else
            {

                item.Hue = ehue;

                if (!Core.AOS)
                {
                    item.LootType = LootType.Regular;
                }

                if (mustEquip)
                {
                    if (equips != null)
                        pack.DropItem(equips);
                }

                if (m != null && m.EquipItem(item))
                    return;

                if (!mustEquip && pack != null)
                    pack.DropItem(item);
                else
                    item.Delete();
            }
        }
        #endregion Checks
    }

    /// <summary>
    /// Used to add a bag of potions to the supplies
    /// </summary>
    public class BagOfPots : Bag
    {
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        [Constructable]
        public BagOfPots()
            : this(50)
        {
        }

        /// <summary>
        /// Constructor.
        /// Adds the potions to a bag
        /// </summary>
        /// <param name="rAMT">Amount of Total Refresh Potions</param>
        /// <param name="hAMT">Amount of Greater Heal Potions</param>
        /// <param name="cAMT">Amount of Greater Cure Potions</param>
        /// <param name="eAMT">Amount of Explosion Potions</param>
        /// <param name="dpAMT">Amount of Deadly Poison Potions</param>
        [Constructable]
        public BagOfPots(int rAMT, int hAMT, int cAMT, int eAMT, int dpAMT)
        {
            for (int i = 1; i <= rAMT; i++)
            {
                DropItem(new TotalRefreshPotion());
            }
            for(int i = 1; i <= hAMT; i++)
            {
                DropItem(new GreaterHealPotion());
            }
            for (int i = 1; i <= cAMT; i++)
            {
                DropItem(new GreaterCurePotion());
            }
            for (int i = 1; i <= eAMT; i++)
            {
                DropItem(new GreaterExplosionPotion());
            }
            for (int i = 0; i < dpAMT; i++)
            {
                DropItem(new DeadlyPoisonPotion());
            }
        }
        #endregion Constructors

        #region Serialization
        /// <summary>
        /// Allows for serialization of the BagOfPots
        /// </summary>
        /// <param name="serial"></param>
        public BagOfPots(Serial serial)
            : base(serial)
        {
        }

        /// <summary>
        /// Saves the item
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        /// <summary>
        /// Loads the item
        /// </summary>
        /// <param name="reader"></param>
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
        #endregion Serialization
    }
}