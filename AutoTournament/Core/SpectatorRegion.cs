using System;
using System.Collections;
using System.Text;

using Server;
using Server.Items;
using Server.Spells.Fifth;
using Server.Spells.Eighth;
using Server.Spells.Fourth;
using Server.Spells.Third;
using Server.Spells.Sixth;
using Server.Spells.Seventh;

using Tournaments.Items;

namespace Tournaments.Regions
{
	public class SpectatorRegion : Region
	{
        private ArenaControl m_Controller;

        public ArenaControl Controller
        {
            get { return m_Controller; }
        }

        public SpectatorRegion(ArenaControl control)
            : base(control.Set+control.Name, Map.Felucca, Region.DefaultPriority, control.SpectatorArea)
		{
            m_Controller = control;
		}

		public override bool CanUseStuckMenu( Mobile m )
		{
			return false;
		}

		public override bool OnBeginSpellCast( Mobile m, ISpell s )
		{
			if ( m.AccessLevel == AccessLevel.Player )
            {
				m.SendMessage( "Mysterious Energies prohibit spell casting." );
				return false;
			}
			else
			{
				return base.OnBeginSpellCast( m, s );
			}
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return from.AccessLevel != AccessLevel.Player;
		}

        public override bool AllowBeneficial(Mobile from, Mobile target)
        {
            return false;
        }

		public override bool AllowHarmful(Mobile from, Mobile target)
		{
            return false;
		}

        public override bool OnDamage(Mobile m, ref int Damage)
        {
            m.SendMessage("You cannot be damaged here.");
            return false;
        }

        public override bool OnResurrect(Mobile m)
        {
            m.SendMessage("You cannot ressurect here.");
            return false;
        }

        public override bool OnHeal(Mobile m, ref int Heal)
        {
            m.SendMessage("You cannot be healed here.");
            return false;
        }

        public override bool OnSkillUse(Mobile m, int skill)
        {
            m.SendMessage("You cannot use skills here.");
            return false;
        }

        public override void OnEnter(Mobile m)
        {
            m.SendMessage("You have entered into tournament grounds.");
            base.OnEnter(m);
        }

        public override bool OnDoubleClick(Mobile m, object o)
        {
            if (o is BasePotion)
            {
                m.SendMessage("You cannot drink potions here.");
                return false;
            }

            if (o is Corpse)
            {
                Corpse c = (Corpse)o;
                bool canLoot;

                if (c.Owner == m)
                    canLoot = true;
                else
                    canLoot = false;

                if (!canLoot)
                    m.SendMessage("You cannot loot that corpse here.");

                if (m.AccessLevel >= AccessLevel.GameMaster && !canLoot)
                {
                    m.SendMessage("This is unlootable but you are able to open that with your Godly powers.");
                    return true;
                }

                return canLoot;
            }

            return base.OnDoubleClick(m, o);
        }
	}
}
