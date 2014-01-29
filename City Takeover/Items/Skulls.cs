using System;

using Server;
using Server.Items;

namespace CityTakeover
{
	public class Skull : Item
	{
		public override double DefaultWeight
		{
			get { return 1.0; }
		}

        private bool m_Desecrated;
        private static int[] m_ItemIDs = new int[]
		{
            0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3, 0x1AE4
		};

        public static int GetRandomItemID()
        {
            return Utility.RandomList(m_ItemIDs);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Desecrated
        {
            get { return m_Desecrated; }
            set 
            { 
                m_Desecrated = value;
                if(m_Desecrated)
                    Hue = 1157;
            }
        }

		[Constructable]
		public Skull() : base( 0x1AE4 )
		{
            Name = "a skull";
            ItemID = GetRandomItemID();
            Desecrated = false;
		}

        public override void OnSingleClick(Mobile from)
        {
            if (m_Desecrated)
                LabelTo(from, "[Desecrated]");

            base.OnSingleClick(from);
        }

        public Skull(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

    public class HugeSkull : Item
    {
        public override double DefaultWeight
        {
            get { return 4.0; }
        }

        private bool m_Desecrated;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Desecrated
        {
            get { return m_Desecrated; }
            set
            {
                m_Desecrated = value;
                if (m_Desecrated)
                    Hue = 1157;
            }
        }

        [Constructable]
        public HugeSkull() : base(0x2203)
        {
            Name = "a huge skull";
            Desecrated = false;
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Desecrated)
                LabelTo(from, "[Desecrated]");

            base.OnSingleClick(from);
        }

        public HugeSkull(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}