using System;

using Server;
using Server.Network;
using Server.Mobiles;
using Server.Targeting;
using Server.Items;

namespace CityTakeover
{
    public class BloodVial : Item
    {
        [Constructable]
        public BloodVial()
            : base(0xF7D)
        {
            Name = "a vial of human blood";
            Weight = 1.2;
            Movable = true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.Backpack.Items.Contains(this))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private BloodVial m_Vial;

            public InternalTarget(BloodVial vial)
                : base(2, false, TargetFlags.None)
            {
                m_Vial = vial;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Skull )
                {
                    Skull s = (Skull)targeted;
                    s.Desecrated = true;
                    m_Vial.Consume();
                }
                else if(targeted is HugeSkull)
                {
                    HugeSkull s = (HugeSkull)targeted;
                    s.Desecrated = true;
                    m_Vial.Consume();
                }
                else
                    from.SendMessage("You decide against wasting the vial.");
            }
        }

        public BloodVial(Serial serial)
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

    public class EmptyBloodVial : Item
    {
        [Constructable]
        public EmptyBloodVial() : base(0x0E24)
        {
            Weight = 1.0;
            Movable = true;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.Backpack.Items.Contains(this))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private EmptyBloodVial m_Vial;

            public InternalTarget(EmptyBloodVial vial)
                : base(2, false, TargetFlags.None)
            {
                m_Vial = vial;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                bool hasKnife = false;

                foreach (Item item in from.Backpack.Items)
                {
                    if(item is BaseKnife)
                        hasKnife = true;
                }

                if (hasKnife && targeted is PlayerMobile && !(targeted == from))
                {
                    Item v = new BloodVial();
                    from.AddToBackpack(v);
                    m_Vial.Consume();
                }
                else if(!hasKnife)
                {
                    from.SendMessage("You are missing a knife.");
                }
                else 
                    from.SendMessage("The blood evaporates upon hitting the vial.");
            }
        }

        public EmptyBloodVial(Serial serial)
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