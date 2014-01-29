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
    public class ArenaRegion : Region
    {
        private ArenaControl m_Controller;

        public ArenaControl Controller
        {
            get { return m_Controller; }
        }

        public ArenaRegion(ArenaControl control)
            : base(control.Name, Map.Felucca, Region.DefaultPriority+10, control.ArenaArea)
        {
            m_Controller = control;
        }

        public override bool CanUseStuckMenu(Mobile m)
        {
            return false;
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            m.Criminal = true;
            if (m.AccessLevel == AccessLevel.Player &&
                (s is MarkSpell || s is RecallSpell || s is GateTravelSpell || s is PolymorphSpell ||
                s is SummonDaemonSpell || s is AirElementalSpell || s is EarthElementalSpell || s is EnergyVortexSpell ||
                s is FireElementalSpell || s is WaterElementalSpell || s is BladeSpiritsSpell || s is SummonCreatureSpell ||
                s is PoisonFieldSpell || s is EnergyFieldSpell || s is WallOfStoneSpell || s is ParalyzeFieldSpell || s is FireFieldSpell))
            {
                m.SendMessage("That spell is not allowed.");
                return false;
            }
            else
            {
                return base.OnBeginSpellCast(m, s);
            }
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return from.AccessLevel != AccessLevel.Player;
        }

        public override bool AllowBeneficial(Mobile from, Mobile target)
        {
            from.Criminal = true;
            target.Criminal = true;
            return GetMobiles().Contains(target);
        }

        public override bool AllowHarmful(Mobile from, Mobile target)
        {
            from.Criminal = true;
            target.Criminal = true;
            return GetMobiles().Contains(target);
        }

        public override bool OnDamage(Mobile m, ref int Damage)
        {
            return base.OnDamage(m, ref Damage);
        }

        public override bool OnResurrect(Mobile m)
        {
            return true;
        }

        public override bool OnHeal(Mobile m, ref int Heal)
        {
            return base.OnHeal(m, ref Heal);
        }

        public override bool OnSkillUse(Mobile m, int skill)
        {
            /*
            bool restricted = m_Controller.IsRestrictedSkill(skill);
            if (restricted && m.AccessLevel == AccessLevel.Player)
            {
                m.SendMessage("You cannot use that skill here.");
                return false;
            }
            */
            m.Criminal = true;
            return base.OnSkillUse(m, skill);
        }

        public override void OnEnter(Mobile m)
        {
            m.Criminal = true;
            base.OnEnter(m);
        }

        public override void OnLocationChanged(Mobile m, Point3D oldLocation)
        {
            m.Criminal = true;
            base.OnLocationChanged(m, oldLocation);
        }

        public override bool OnDoubleClick(Mobile m, object o)
        {
            m.Criminal = true;
            if (o is Mobile)
            {
                Mobile mob = (Mobile)o;
                mob.Criminal = true;
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

        /*
        public bool IsRestrictedSkill(int skill)
        {
            if (m_RestrictedSkills.Length != SkillInfo.Table.Length)
            {

                m_RestrictedSkills = new BitArray(SkillInfo.Table.Length);

                for (int i = 0; i < m_RestrictedSkills.Length; i++)
                    m_RestrictedSkills[i] = false;
            }

            if (skill < 0)
                return false;

            return m_RestrictedSkills[skill];
        }
        */
    }
}
