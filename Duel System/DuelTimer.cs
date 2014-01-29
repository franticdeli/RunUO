/****************************************************************\
* File Name : DuelTimer.cs                                       *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 03/27/12                                           *
* Desc.     : Plays out the actual due with the information      *
*             provided by the duel stone. Stops when either      *
*             there is a winner or when the time limit is up.    *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  MM/DD/YY   | Comments                                         |
\****************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;
using Server.Spells.First;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;
using Server.Misc;
using Server.Targeting;
using Server.Regions;

namespace Duel
{
    class DuelTimer : Timer
    {
        #region Initilization
        /// <summary>
        /// Duelist One
        /// </summary>
        Mobile m_Duelist1;

        /// <summary>
        /// Duelist Two
        /// </summary>
        Mobile m_Duelist2;

        /// <summary>
        /// Amount of times Duelist 1 has won
        /// </summary>
        int m_Wins1 = 0;

        /// <summary>
        /// Amount of times Duelist 2 has won
        /// </summary>
        int m_Wins2 = 0;

        /// <summary>
        /// Calculated fee for the duel
        /// </summary>
        int m_CalcFee = 0;

        /// <summary>
        /// The duel stone which contains the duel info
        /// </summary>
        DuelStone m_Stone;

        /// <summary>
        /// Time the duelists have for staging
        /// </summary>
        DateTime m_Staging;

        /// <summary>
        /// Has the round started yet?
        /// </summary>
        bool m_RoundStart = false;

        /// <summary>
        /// The Current Round for the Duel
        /// </summary>
        int m_Round = 1;

        /// <summary>
        /// The duel's time limit timer
        /// </summary>
        DuelLimit limit;
        #endregion Initilization

        #region Timer Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="d1">Duelist One</param>
        /// <param name="d2">Duelist Two</param>
        /// <param name="item">Duel Stone that contains the Duel info</param>
        public DuelTimer(Mobile d1, Mobile d2, DuelStone item) : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(1.0))
        {
            m_Duelist1 = d1;
            m_Duelist2 = d2;
            m_Stone = item;

            double fee = (double)m_Stone.Wager * ((double)m_Stone.Fee / 100);
            m_CalcFee = (int)fee;

            m_Staging = DateTime.Now + m_Stone.StagingTime;
            GetDuelists();
        }

        /// <summary>
        /// Commences the duel
        /// </summary>
        protected override void OnTick()
        {
            if (!m_Duelist1.Alive || !m_Duelist2.Alive)
                FinishRound();
            else if (m_Staging > DateTime.Now && !m_RoundStart)
                StartRound();
        }
        #endregion Timer Constructor

        #region Duel Commencement Methods
        /// <summary>
        /// Grab the Duelists and place them into the staging area
        /// </summary>
        private void GetDuelists()
        {
            // Check if they both are alive
            if (m_Round == 1 && (!m_Duelist1.Alive || !m_Duelist2.Alive))
            {
                if (!m_Stone.SuppliedDuel)
                {
                    if (!m_Duelist1.Alive)
                    {
                        m_Duelist1.SendMessage("The Duel is ending. Why are you already dead?");
                        m_Duelist2.SendMessage("The Duel is ending. Your opponent is already dead.");
                    }
                    if (!m_Duelist2.Alive)
                    {
                        m_Duelist1.SendMessage("The Duel is ending. Your opponent is already dead.");
                        m_Duelist2.SendMessage("The Duel is ending. Why are you already dead?");
                    }

                    m_Stone.refreshDuel();
                    return;
                }
                else
                {
                    if (!m_Duelist1.Alive)
                        m_Duelist1.Resurrect();
                    if (!m_Duelist2.Alive)
                        m_Duelist2.Resurrect();
                }
            }
            
            if (m_Round == 1 && m_Stone.SuppliedDuel)
            {
                ItemsToBank(m_Duelist1);
                ItemsToBank(m_Duelist2);
            }

            if (m_Stone.Fee > 0)
            {
                AlterFunds(m_Duelist1, m_CalcFee, false);
                AlterFunds(m_Duelist2, m_CalcFee, false);
                m_Duelist1.SendMessage(String.Format("Withdrawing the Fee for the duel from your bankbox. Fee: {0}", m_CalcFee));
                m_Duelist2.SendMessage(String.Format("Withdrawing the Fee for the duel from your bankbox. Fee: {0}", m_CalcFee));
            }


            m_Duelist1.MoveToWorld(m_Stone.StageLoc1, m_Stone.ArenaMap);
            m_Duelist1.SendMessage(String.Format("You have {0} seconds to prepare for the duel.", m_Stone.StagingTime.TotalSeconds));

            m_Duelist2.MoveToWorld(m_Stone.StageLoc2, m_Stone.ArenaMap);
            m_Duelist2.SendMessage(String.Format("You have {0} seconds to prepare for the duel.", m_Stone.StagingTime.TotalSeconds));
        }

        /// <summary>
        /// Place the duelist in the arena and start the countdown
        /// </summary>
        private void StartRound()
        {
            m_RoundStart = true;

            m_Duelist1.MoveToWorld(m_Stone.DuelLoc1, m_Stone.ArenaMap);
            m_Duelist1.Paralyzed = true;

            m_Duelist2.MoveToWorld(m_Stone.DuelLoc2, m_Stone.ArenaMap);
            m_Duelist2.Paralyzed = true;

            Countdown c = new Countdown(this);
            c.Start();
        }

        /// <summary>
        /// Unparalyze the duelists, make each of them criminal(to eliminate murder
        /// counts), and begin the fight 
        /// </summary>
        public void Fight()
        {
            limit = new DuelLimit(this, m_Stone.MaxDuelTime);

            Refresh(m_Duelist1);
            Refresh(m_Duelist2);
            
            if (!m_Duelist1.Criminal)
                m_Duelist1.Criminal = true;
            if (!m_Duelist2.Criminal)
                m_Duelist2.Criminal = true;

            limit.Start();
        }
        #endregion Duel Commencement Methods

        #region Duel Finishing Methods
        /// <summary>
        /// Finish the current round
        /// </summary>
        private void FinishRound()
        {
            limit.Stop();
            m_RoundStart = false;

            if (!m_Duelist1.Alive)
            {
                m_Duelist1.Resurrect();
                m_Wins2 += 1;
            }
            if (!m_Duelist2.Alive)
            {
                m_Duelist2.Resurrect();
                m_Wins1 += 1;
            }

            if (m_Round != m_Stone.Rounds)
            {
                m_Round += 1;

                m_Staging = DateTime.Now + m_Stone.StagingTime;
                GetDuelists();
            }
            else
                FinishDuel();
        }

        /// <summary>
        /// Award the winner and give the reward
        /// </summary>
        public void FinishDuel()
        {
            // Determining Winner
            if (m_Wins1 > m_Wins2)
            {
                AlterFunds(m_Duelist1, m_Stone.Wager - m_CalcFee, true);
                AlterFunds(m_Duelist2, m_Stone.Wager - m_CalcFee, false);
                m_Duelist1.SendMessage("Your winnings have been added to your bankbox.");
                m_Duelist2.SendMessage("The amount you had wagered has been removed from your bankbox.");
            }
            else if (m_Wins2 > m_Wins1)
            {
                AlterFunds(m_Duelist1, m_Stone.Wager - m_CalcFee, false);
                AlterFunds(m_Duelist2, m_Stone.Wager - m_CalcFee, true);
                m_Duelist2.SendMessage("Your winnings have been added to your bankbox.");
                m_Duelist1.SendMessage("The amount you had wagered has been removed from your bankbox.");
            }
            else
            {
                m_Duelist1.SendMessage("You are equals in battle.");
                m_Duelist2.SendMessage("You are equals in battle.");
            }

            if (m_Stone.SuppliedDuel)
            {
                m_Duelist1.Kill();
                foreach (Item item in m_Duelist1.Backpack.Items)
                {
                    item.Delete();
                }

                m_Duelist2.Kill();
                foreach (Item item in m_Duelist2.Backpack.Items)
                {
                    item.Delete();
                }

                m_Duelist1.Resurrect();
                m_Duelist2.Resurrect();
            }

            m_Duelist1.MoveToWorld(m_Stone.Exit, m_Stone.Map);
            m_Duelist2.MoveToWorld(m_Stone.Exit, m_Stone.Map);
            m_Stone.refreshDuel();
            this.Stop();
        }

        private void AlterFunds(Mobile m, int amt, bool addAmt)
        {
            BankBox bank = m.BankBox;
            int totAmt = 0;

            if (addAmt)
            {
                bank.AddItem(new BankCheck(amt));
            }
            else
            {
                while (totAmt != amt)
                {
                    Item gp = bank.FindItemByType(typeof(Gold));
                    if (gp.Amount > (amt - totAmt))
                    {
                        gp.Amount -= (amt - totAmt);
                        totAmt = amt;
                    }
                    else
                    {
                        totAmt = gp.Amount;
                        gp.Delete();
                    }
                }
            }
        }
        #endregion Duel Finishing Methods

        #region Misc.
        /// <summary>
        /// Fully refreshes the targetted player.
        /// Prevents any type of pre-casting or other advantages.
        /// </summary>
        /// <param name="targ"> The target to be refreshed</param>
        private void Refresh(Mobile targ)
        {
            try
            {
                targ.Mana = targ.ManaMax;
                targ.Hits = targ.HitsMax;
                targ.Stam = targ.StamMax;
                targ.Poison = null;

                targ.Say("*Refreshed!*");
                targ.Say("*Debuffed!*");

                Server.Targeting.Target.Cancel(targ);

                if (targ.MeleeDamageAbsorb > 0)
                {
                    targ.MeleeDamageAbsorb = 0;
                    
                    //targ.EndAction(typeof(RechargeSpell));
                    //ReactiveArmorSpell.EndArmor(targ);

                    targ.SendMessage("Reactive armor has been nullified.");
                }

                if (targ.MagicDamageAbsorb > 0)
                {
                    targ.MagicDamageAbsorb = 0;
                    targ.SendMessage("Magic Reflection has been nullified.");
                }

                StatMod mod;
                mod = targ.GetStatMod("[Magic] Str Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Str Offset");

                mod = targ.GetStatMod("[Magic] Dex Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Dex Offset");

                mod = targ.GetStatMod("[Magic] Int Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Int Offset");

                targ.Paralyzed = false;

                BuffInfo.RemoveBuff(targ, BuffIcon.Clumsy);
                BuffInfo.RemoveBuff(targ, BuffIcon.FeebleMind);
                BuffInfo.RemoveBuff(targ, BuffIcon.Weaken);
                BuffInfo.RemoveBuff(targ, BuffIcon.MassCurse);
                BuffInfo.RemoveBuff(targ, BuffIcon.Agility);
                BuffInfo.RemoveBuff(targ, BuffIcon.Cunning);
                BuffInfo.RemoveBuff(targ, BuffIcon.Strength);
                BuffInfo.RemoveBuff(targ, BuffIcon.Bless);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : " + e.InnerException);
            }
        }

        /// <summary>
        /// Moves all the players items to thier bank.
        /// Notifies the player that thier belongings have been placed in thier bank.
        /// </summary>
        /// <param name="m"> The mobile to be have items moved</param>
        public static void ItemsToBank(Mobile m)
        {
            try
            {
                if (m.Backpack != null && m.BankBox != null)
                {
                    Container bp = m.Backpack;
                    BankBox bank = m.BankBox;
                    Container pack = new Backpack();
                    List<Item> list = new List<Item>();
                    Item addItem;

                    foreach (Item item in m.Items)
                    {
                        if (item != bp && item != bank)
                            list.Add(item);
                    }
                    foreach (Item item in bp.Items)
                    {
                        list.Add(item);
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        addItem = list[i];
                        pack.AddItem(addItem);
                    }

                    if (pack.Items.Count > 0)
                        bank.AddItem(pack);
                }

                m.SendMessage("All of your items have been sent to your bankbox via a backpack.");

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : " + e.InnerException);
            }
        }

        /// <summary>
        /// Causes the duelists to countdown to the start of the Duel
        /// </summary>
        private class Countdown : Timer
        {
            /// <summary>
            /// Used to determine if the time is up
            /// </summary>
            DateTime m_StartTime, m_EndTime;

            DuelTimer m_Timer;
            int m_Time = 3;

            /// <summary>
            /// Ensures that the duel does not go longer than the maximum time limit
            /// </summary>
            /// <param name="duelTimer">Duel Timer to stop when the time is reached</param>
            /// <param name="duelLength">the time limit</param>
            public Countdown(DuelTimer timer)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_Timer = timer;
                m_StartTime = DateTime.Now;
                m_EndTime = m_StartTime + TimeSpan.FromSeconds(4.0);
            }

            /// <summary>
            /// Used to check if the time is up and respond accordingly
            /// </summary>
            protected override void OnTick()
            {
                m_StartTime = DateTime.Now;

                if (m_StartTime >= m_EndTime)
                {
                    m_Timer.Fight();
                    this.Stop();
                }
                else
                {
                    m_Timer.m_Duelist1.Say(m_Time.ToString());
                    m_Timer.m_Duelist2.Say(m_Time.ToString());

                    m_Time -= 1;
                }
            }
        }

        /// <summary>
        /// The simulates the time limit for each duel
        /// </summary>
        private class DuelLimit : Timer
        {
            /// <summary>
            /// Duel Timer this timer is linked to
            /// </summary>
            DuelTimer m_DuelTimer;

            /// <summary>
            /// Used to determine if the time is up
            /// </summary>
            DateTime m_StartTime, m_EndTime;

            /// <summary>
            /// Ensures that the duel does not go longer than the maximum time limit
            /// </summary>
            /// <param name="duelTimer">Duel Timer to stop when the time is reached</param>
            /// <param name="duelLength">the time limit</param>
            public DuelLimit(DuelTimer duelTimer, TimeSpan duelLength)
                : base(TimeSpan.FromSeconds(0.0), TimeSpan.FromMinutes(1.0))
            {
                m_StartTime = DateTime.Now;
                m_EndTime = m_StartTime + duelLength;
            }

            /// <summary>
            /// Used to check if the time is up and respond accordingly
            /// </summary>
            protected override void OnTick()
            {
                m_StartTime = DateTime.Now;

                if (m_StartTime >= m_EndTime)
                {
                    m_DuelTimer.FinishRound();
                }
            }
        }
        #endregion Misc.
    }
}
