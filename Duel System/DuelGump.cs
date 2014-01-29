/****************************************************************\
* File Name : DuelGump.cs                                        *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : Gumps used to decide the wager for the duel,       *
*             the opponent, and whether the opponent wishes      *
*             to duel for the terms.                             *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY   | Comments                                         |
\****************************************************************/

using System;

using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

using Duel;

namespace Server.Gumps
{
    class DuelGump : Gump
    {
        /// <summary>
        /// Mobile the gump is displayed to
        /// </summary>
        Mobile m_Owner;

        /// <summary>
        /// Duel Stone the gump is linked to
        /// </summary>
        DuelStone m_Stone;

        /// <summary>
        /// Prevents the user from hogging the duel stone
        /// </summary>
        InternalTimer m_InTime;

        /// <summary>
        /// Constructor
        /// Contains design info for the Duel Gump
        /// </summary>
        /// <param name="from">Mobile the gump is displayed to</param>
        /// <param name="stone">Duel Stone the gump is linked to</param>
        public DuelGump(Mobile from, DuelStone stone) : base(20, 30)
        {
            m_Owner = from;
            m_Stone = stone;

            this.Closable=false;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

            AddPage(0);
            AddBackground(100, 100, 276, 331, 9350);
            AddHtml(117, 115, 241, 32, @"<center>Duel Terms</center>", (bool)true, (bool)false);
            AddLabel(122, 164, 0, @"Duel Fee(%):");
            AddLabel(122, 201, 0, @"Gear:");
            AddLabel(122, 239, 0, @"Time Limit:");
            AddLabel(124, 278, 0, @"Rules:");
            AddHtml(202, 159, 153, 30, m_Stone.Fee.ToString(), (bool)true, (bool)false); // Duel Fee
            AddHtml(202, 195, 153, 30, m_Stone.SuppliedDuel ? "Provided":"User", (bool)true, (bool)false); // Gear
            AddHtml(202, 233, 153, 30, m_Stone.MaxDuelTime.TotalMinutes.ToString()+"min", (bool)true, (bool)false); // Time Limit
            AddHtml(202, 272, 153, 30, @"Field", (bool)true, (bool)false); // Ruleset
            AddLabel(140, 319, 0, @"How much do you wish to wager?");
            AddBackground(124, 342, 230, 33, 9350);
            AddButton(222, 393, 247, 248, 1, GumpButtonType.Reply, 0);
            AddButton(291, 393, 241, 242, 0, GumpButtonType.Reply, 0);
            AddTextEntry(129, 346, 217, 23, 0, 0, m_Stone.MinimumWager.ToString());

            m_InTime = new InternalTimer(m_Owner, m_Stone);
            m_InTime.Start();
        }

        /// <summary>
        /// Handles the Gump's responses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="info"></param>
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            TextRelay entry0 = info.GetTextEntry(0);
            string text0 = (entry0 == null ? "" : entry0.Text.Trim());
            int minWager;

            m_InTime.Stop();

            switch (info.ButtonID)
            {
                case 0:
                    {
                        m_Stone.refreshDuel();
                        break;
                    }
                case 1:
                    {
                        if (int.TryParse(text0, out minWager))
                        {
                            if (minWager >= m_Stone.MinimumWager)
                            {
                                if (m_Owner.BankBox.TotalGold < (minWager))
                                {
                                    from.SendMessage("You have insufficient funds.");
                                    m_Stone.refreshDuel();
                                }
                                else
                                {
                                    m_Stone.Wager = minWager;
                                    m_Owner.Target = new InternalTarget(m_Owner, m_Stone);
                                }
                            }
                            else
                            {
                                from.SendMessage(String.Format("Your wager must be more than {0}.", m_Stone.MinimumWager));
                                m_Stone.refreshDuel();
                            }
                            break;
                        }
                        else
                        {
                            from.SendMessage("You have input an invalid wager.");
                            m_Stone.refreshDuel();
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Allows the Duelist to choose who he/she wishes to duel against
        /// Must be within 10 paces from Duelist.
        /// </summary>
        private class InternalTarget : Target
        {
            /// <summary>
            /// Mobile that is targetting
            /// </summary>
            Mobile m_Owner;

            /// <summary>
            /// Duel Stone linked to the targetting
            /// </summary>
            DuelStone m_Stone;

            /// <summary>
            /// Prevents the user from hogging the duel stone
            /// </summary>
            InternalTimer m_InTime;

            /// <summary>
            /// Used to allow the mobile to target its duel opponent
            /// and send the duel request
            /// </summary>
            /// <param name="owner">Mobile requesting the duel</param>
            /// <param name="stone">Duel Stone linked to the targetting</param>
            public InternalTarget(Mobile owner, DuelStone stone) : base(5, true, TargetFlags.None)
            {
                m_Owner = owner;
                m_Stone = stone;

                m_InTime = new InternalTimer(m_Owner, m_Stone);
                m_InTime.Start();
            }

            /// <summary>
            /// Finishes the targetting and send the Request gump
            /// </summary>
            /// <param name="from">Mobile doing the targetting</param>
            /// <param name="o">Object that is targetted</param>
            protected override void OnTarget(Mobile from, object o)
            {
                m_InTime.Stop();

                if (o is Mobile)
                {
                    Mobile target = (Mobile)o as Mobile;
                    if (m_Owner.GetDistanceToSqrt(target) <= 5)
                        target.SendGump(new DuelConfirmGump(m_Owner, target, m_Stone));
                    else
                    {
                        m_Owner.SendMessage("Your target is too far away.");
                    }
                }
            }
        }

        /// <summary>
        /// Timer used to ensure players not trolling the duel stone by
        /// keeping the one gump open or not targetting an opponent.
        /// </summary>
        private class InternalTimer : Timer
        {
            /// <summary>
            /// Time allowed to keep the gumps open or target option available
            /// </summary>
            TimeSpan m_ChoiceTime = new TimeSpan(0, 1, 0); // 1min period to make decisions

            /// <summary>
            /// Used to determine if the time is up
            /// </summary>
            DateTime m_StartTime, m_EndTime;

            /// <summary>
            /// Mobile that has either the gumps or the target option
            /// </summary>
            Mobile m_Owner;

            /// <summary>
            /// Duel Stone that is linked to these events
            /// </summary>
            DuelStone m_Stone;

            /// <summary>
            /// Timer constructer
            /// </summary>
            /// <param name="m">Mobile that has either the gumps or the target option</param>
            /// <param name="stone">Duel Stone that is linked to these events</param>
            public InternalTimer(Mobile m, DuelStone stone) : base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(1.0))
            {
                m_StartTime = DateTime.Now;
                m_EndTime = m_StartTime + m_ChoiceTime;
                m_Owner = m;
                m_Stone = stone;
            }

            /// <summary>
            /// Used to check if the time is up and respond accordingly
            /// </summary>
            protected override void OnTick()
            {
                m_StartTime = DateTime.Now;

                if (m_StartTime >= m_EndTime)
                {
                    m_Owner.CloseGump(typeof(DuelGump));
                    m_Owner.Target = null;
                    m_Stone.refreshDuel();
                    this.Stop();
                }
            }
        }
    }

    class DuelConfirmGump : Gump
    {
        /// <summary>
        /// Mobile requested to duel
        /// </summary>
        Mobile m_Acceptor;

        /// <summary>
        /// Mobile requesting the duel
        /// </summary>
        Mobile m_Requester;

        /// <summary>
        /// Duel Stone the gump is linked to
        /// </summary>
        DuelStone m_Stone;

        /// <summary>
        /// Prevents the user from hogging the duelstone
        /// </summary>
        InternalTimer m_InTime;

        /// <summary>
        /// Contains the design infor for the Duel Confirmation gump
        /// </summary>
        /// <param name="from">Mobile requesting the duel</param>
        /// <param name="to">Mobile requested to duel</param>
        /// <param name="stone">Duel Stone the gump is linked to</param>
        public DuelConfirmGump(Mobile from, Mobile to, DuelStone stone) : base(20, 30)
        {
            m_Requester = from;
            m_Acceptor = to;
            m_Stone = stone;

            this.Closable = false;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(100, 100, 276, 331, 9350);
            AddHtml(117, 115, 241, 32, @"<center>Duel Terms</center>", (bool)true, (bool)false);
            AddLabel(122, 164, 0, @"Duel Fee(%):");
            AddLabel(122, 201, 0, @"Gear:");
            AddLabel(122, 239, 0, @"Time Limit:");
            AddLabel(124, 278, 0, @"Rules:");
            AddLabel(124, 312, 0, @"Opponent");
            AddLabel(124, 347, 0, @"Wager");
            AddHtml(202, 159, 153, 30, m_Stone.Fee.ToString(), (bool)true, (bool)false); // Duel Fee
            AddHtml(202, 195, 153, 30, m_Stone.SuppliedDuel ? "Provided" : "User", (bool)true, (bool)false); // Gear
            AddHtml(202, 233, 153, 30, m_Stone.MaxDuelTime.TotalMinutes.ToString() + "min", (bool)true, (bool)false); // Time Limit
            AddHtml(202, 272, 153, 30, @"Field", (bool)true, (bool)false); // Ruleset
            AddHtml(202, 306, 153, 30, m_Requester.Name, (bool)true, (bool)false);
            AddHtml(202, 341, 153, 30, m_Stone.Wager.ToString(), (bool)true, (bool)false);
            AddButton(222, 393, 247, 248, 1, GumpButtonType.Reply, 0);
            AddButton(291, 393, 241, 242, 0, GumpButtonType.Reply, 0);

            m_InTime = new InternalTimer(m_Acceptor, m_Stone);
            m_InTime.Start();
        }

        /// <summary>
        /// Handles the Gump's responses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="info"></param>
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            m_InTime.Stop();

            switch (info.ButtonID)
            {
                case 0:
                    {
                        m_Stone.refreshDuel();
                        break;
                    }
                case 1:
                    {
                        if (m_Acceptor.BankBox.TotalGold < (m_Stone.Wager))
                        {
                            m_Acceptor.SendMessage("You have insufficient funds.");
                            m_Stone.refreshDuel();
                        }
                        else
                        {
                            DuelTimer timer = new DuelTimer(m_Requester, m_Acceptor, m_Stone);
                            timer.Start();
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Timer used to ensure players not trolling the duel stone by
        /// keeping the one gump open or not targetting an opponent.
        /// </summary>
        private class InternalTimer : Timer
        {
            /// <summary>
            /// Time allowed to keep the gumps open or target option available
            /// </summary>
            TimeSpan m_ChoiceTime = new TimeSpan(0, 1, 0); // 1min period to make decisions

            /// <summary>
            /// Used to determine if the time is up
            /// </summary>
            DateTime m_StartTime, m_EndTime;

            /// <summary>
            /// Mobile that has either the gumps or the target option
            /// </summary>
            Mobile m_Owner;

            /// <summary>
            /// Duel Stone that is linked to these events
            /// </summary>
            DuelStone m_Stone;

            /// <summary>
            /// Timer constructer
            /// </summary>
            /// <param name="m">Mobile that has either the gumps or the target option</param>
            /// <param name="stone">Duel Stone that is linked to these events</param>
            public InternalTimer(Mobile m, DuelStone stone)
                : base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(1.0))
            {
                m_StartTime = DateTime.Now;
                m_EndTime = m_StartTime + m_ChoiceTime;
                m_Owner = m;
                m_Stone = stone;
            }

            /// <summary>
            /// Used to check if the time is up and respond accordingly
            /// </summary>
            protected override void OnTick()
            {
                m_StartTime = DateTime.Now;

                if (m_StartTime >= m_EndTime)
                {
                    m_Owner.CloseGump(typeof(DuelConfirmGump));
                    m_Stone.refreshDuel();
                    this.Stop();
                }
            }
        }
    }
}
