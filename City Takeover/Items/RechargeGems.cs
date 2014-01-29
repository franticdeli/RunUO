using System;

using Server;
using Server.Items;

namespace CityTakeover
{
	public class RechargeGem : Item
	{
		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		[Constructable]
		public RechargeGem() : base( 0x186F )
		{
            Name = "a power gem";
            Hue = 1151;
		}

        public RechargeGem(Serial serial)
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

    public class MajorRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public MajorRechargeGem()
            : base(0x186A)
        {
            Name = "a condensed power gem";
            Hue = 1151;
        }

        public MajorRechargeGem(Serial serial)
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

    public class MinorRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public MinorRechargeGem()
            : base(0x2F5E)
        {
            Name = "a small power gem";
            Hue = 1151;
        }

        public MinorRechargeGem(Serial serial)
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

    public class MassiveRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public MassiveRechargeGem()
            : base(0x3194)
        {
            Name = "a massively condensed power gem";
            Hue = 1151;
        }

        public MassiveRechargeGem(Serial serial)
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

    public class DarkRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public DarkRechargeGem()
            : base(0x186F)
        {
            Name = "a power gem";
            Hue = 1175;
        }

        public DarkRechargeGem(Serial serial)
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

    public class DarkMajorRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public DarkMajorRechargeGem()
            : base(0x186A)
        {
            Name = "a condensed power gem";
            Hue = 1175;
        }

        public DarkMajorRechargeGem(Serial serial)
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

    public class DarkMinorRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public DarkMinorRechargeGem()
            : base(0x2F5E)
        {
            Name = "a small power gem";
            Hue = 1175;
        }

        public DarkMinorRechargeGem(Serial serial)
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

    public class DarkMassiveRechargeGem : Item
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public DarkMassiveRechargeGem()
            : base(0x3194)
        {
            Name = "a massively condensed power gem";
            Hue = 1175;
        }

        public DarkMassiveRechargeGem(Serial serial)
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