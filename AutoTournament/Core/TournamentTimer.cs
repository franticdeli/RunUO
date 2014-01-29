/***************************************************************\
* File Name : TournamentTimer.cs                                *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 5/18/2010                                         *
* Desc.     : Gets and places the participants into their       *
*             respective places to proceed with the             *
*             tournament. Plays out the tournament until        *
*             it is finished.                                   *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
\****************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;
using Server.Misc;
using Server.Targeting;

using Tournaments.Mobiles;
using Tournaments.Items;

namespace Tournaments
{
	public class TournamentTimer
	{
		#region Initialization
        public bool m_TournamentStarted;
		public TimeSpan m_DelayStart;
        private string m_Set;
        private int m_CurrentRound;
        private int m_CurrentMatch;
        private Tournament m_Tournament;
		private BuildBracket m_Bracket;
		private List<Teams> m_Contestants;
        private List<Match> m_Matches;
        private List<ArenaControl> m_Arenas;
        private ArenaControl m_NextArena;
		#endregion
		
		#region Timer Constructor
        /// <summary>
        /// The main tournament timer.
        /// Setup the brackets and start the timer
        /// </summary>
        public TournamentTimer(Tournament tournament)
        {
            Random rand = new Random();
            Manager.FilterInactiveTeams(tournament);
            m_CurrentRound = 0;
            m_CurrentMatch = 0;
            m_DelayStart = TimeSpan.FromSeconds(30.0);
            m_Contestants = new List<Teams>();
            m_Matches = new List<Match>();
            m_Arenas = new List<ArenaControl>();
            m_Tournament = tournament;
            m_Bracket = new BuildBracket();
            m_TournamentStarted = false;
            m_Contestants.AddRange(tournament.Teams);
            ShuffleTeams();

            switch (m_Tournament.Type)
            {
                case TournamentType.RoundRobin:
                {
                    break;
                }
                case TournamentType.SingleElimination:
                {
                    m_Bracket.SingleElimination(m_Contestants);
                    break;
                }
                case TournamentType.DoubleElimination:
                {
                    break;
                }
                case TournamentType.Hybrid:
                {
                    break;
                }
            }

            string arenaset = (m_Tournament.ArenaSets != null && m_Tournament.ArenaSets.Count > 0)? m_Tournament.ArenaSets[rand.Next(m_Tournament.ArenaSets.Count)] : ""; 
            m_Arenas = ArenaControl.GetArenaSet(m_Tournament.TeamSize, arenaset);
            if (!(m_Arenas.Count > 0))
                m_Arenas = ArenaControl.RandomArenaSet(m_Tournament.TeamSize);
        }
		#endregion

        public void Prepare()
        {
            if (m_Contestants.Count > 1 && HaveUnoccupiedArena())
            {
                foreach (Teams team in m_Contestants)
                {
                    if (m_Tournament.Supplied)
                        foreach (Mobile m in team.getOwners())
                            Manager.ItemsToBank((PlayerMobile)m);

                    MoveTeam(team, m_NextArena.Map, m_NextArena.HoldingArea, "Entering the tournament grounds.", false);
                }
                // Broadcast "The tournament has begun." at the beginning of the tournament
                if (!m_TournamentStarted)
                {
                    World.Broadcast(0x35, true, String.Format("The {0} tournament has begun.", m_Tournament.TeamSize));

                    m_TournamentStarted = true;
                }

                m_Matches = m_Bracket.SEGetRound(m_CurrentRound);
                DelayTimer delay = new DelayTimer(this, m_DelayStart);
                delay.Start();
            }
            else
                World.Broadcast(0x35, true, String.Format("The {0} tournament has been canceled due to lack of participants.", m_Tournament.TeamSize));
        }

        public void Start()
        {
            int i = 0;
            while( i < m_Arenas.Count && m_CurrentMatch <= m_Matches.Count )
            {
                Progress();
                i++;
            }
        }

        public void Progress()
        {
            switch (m_Tournament.Type)
            {
                #region RoundRobin Progression
                case TournamentType.RoundRobin:
                {
                    break;
                }
                #endregion

                #region Single Elimination Progression
                case TournamentType.SingleElimination:
                {
                    if (m_Matches.Count != 0)
                    {
                        if (m_CurrentMatch < m_Matches.Count)
                        {
                            Console.WriteLine("Getting a Match");
                            if (HaveUnoccupiedArena())
                            {
                                MatchTimer bout = new MatchTimer(this, m_NextArena, m_Matches[m_CurrentMatch]);
                                m_CurrentMatch += 1;
                            }
                        }
                        else if (AllArenasUnoccupied() && m_CurrentMatch >= m_Matches.Count && m_CurrentRound < (m_Bracket.TotalRounds-1))
                        {
                            m_CurrentRound += 1;
                            m_CurrentMatch = 0;
                            m_Matches = m_Bracket.SEGetRound(m_CurrentRound);
                            RoundDelayTimer delay = new RoundDelayTimer(this, m_DelayStart);
                            delay.Start();
                        }
                        else if (m_CurrentRound >= (m_Bracket.TotalRounds - 1))
                        {
                            m_Bracket.SEGetWinners(m_Tournament);
                        }
                    }
                    break;
                }
                #endregion

                #region Double Elimnination Progression
                case TournamentType.DoubleElimination:
                {
                    break;
                }
                #endregion

                case TournamentType.Hybrid:
                {
                    // TODO: Hybrid Logic
                    break;
                }
            }
        }

        #region Misc
        /// <summary>
        /// A method that simulates the logarithm of base 2.
        /// </summary>
        /// <param name="var">the variable</param>
        /// <returns>the logarithmic base two value of the variable</returns>
        private double log2(double var)
        {
            return (Math.Log(var) / Math.Log(2));
        }

        /// <summary>
        /// Moves the contestants so that they can leave the tournament grounds.
        /// Clears the Contestant list for the next tournament.
        /// </summary>
        public void MoveToLeave()
        {
            //MoveTeam(loser, arena.Map, arena.ExitArea, "", false);
        }

        /// <summary>
        /// Determines if there is an unoccupied arena and sets it to be
        /// the next arena to be used
        /// </summary>
        /// <returns></returns>
        public bool HaveUnoccupiedArena()
        {
            m_NextArena = null;

            foreach (ArenaControl arena in m_Arenas)
            {
                if (!arena.Occupied)
                {
                    m_NextArena = arena;
                    break;
                }
            }

            return m_NextArena != null;
        }

        /// <summary>
        /// Returns whether all of the arenas are unoccupied
        /// </summary>
        /// <returns>the arenas are all unoccupied</returns>
        public bool AllArenasUnoccupied()
        {
            bool AllUnoccupied = true;
            foreach (ArenaControl arena in m_Arenas)
            {
                if (arena.Occupied)
                {
                    AllUnoccupied = false;
                }
            }

            return AllUnoccupied;
        }

        /// <summary>
        /// Shuffles the teams
        /// </summary>
        public void ShuffleTeams()
        {
            Random rng = new Random();
            int n = m_Contestants.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Teams value = m_Contestants[k];
                m_Contestants[k] = m_Contestants[n];
                m_Contestants[n] = value;
            }
        }

        /// <summary>
        /// Move the selected team
        /// </summary>
        /// <param name="team">Team to be moved</param>
        /// <param name="map">Map to be moved to</param>
        /// <param name="location">Point3D Location to be moved to</param>
        /// <param name="msg">Message to tell the players being moved</param>
        /// <param name="paralyze">Should they be paralyzed after the move</param>
        public void MoveTeam(Teams team, Map map, Point3D location, string msg, bool paralyze)
        {
            Mobile m;
            for (int i = 0; i < team.getOwners().Count; i++)
            {
                m = team.getOwners()[i];
                m.MoveToWorld(location, map); //Arena Area
                Manager.RefreshPlayer((PlayerMobile)m);
                if (msg.Length > 0 && (Manager.IsOnline((PlayerMobile)m)))
                    m.SendMessage(msg);
                if (paralyze)
                    m.Paralyzed = true;
            }
        }
        #endregion

        private class DelayTimer : Timer
        {
            private TournamentTimer t;

            public DelayTimer(TournamentTimer tournament, TimeSpan delay)
                : base(delay, delay)
            {
                Priority = TimerPriority.OneSecond;
                t = tournament;
            }

            protected override void OnTick()
            {
                t.Start();
                this.Stop();
            }
        }

        private class RoundDelayTimer : Timer
        {
            private TournamentTimer t;

            public RoundDelayTimer(TournamentTimer tournament, TimeSpan delay)
                : base(delay, delay)
            {
                Priority = TimerPriority.OneSecond;
                t = tournament;
            }

            protected override void OnTick()
            {
                t.Progress();
                this.Stop();
            }
        }

        private class MatchTimer : Timer
        {
            public TournamentTimer t;
            public Match b;
            public ArenaControl a;
            public Announcer announcer;
            private Teams m_Team1, m_Team2;
            private int countdown;
            private bool started;
            private bool cfinished;
            private TimeSpan staging;
            private DateTime stagedelay;
            private AnnouncerTimer timer;

            public MatchTimer(TournamentTimer tournament, ArenaControl arena, Match bout)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                Priority = TimerPriority.OneSecond;
                t = tournament;
                b = bout;
                a = arena;
                a.Occupied = true;
                countdown = 6;
                started = false;
                cfinished = false;
                staging = TimeSpan.FromSeconds(30.0);
                announcer = getAnn();
                BeginStage();
            }

            /// <summary>
            /// Get the announcer on the map so as
            /// to not create a new mobile
            /// </summary>
            /// <returns></returns>
            private Announcer getAnn()
            {
                Announcer Ann = null;

                foreach (Mobile mob in Map.Felucca.GetMobilesInRange(a.AnnouncerSpot, 10))
                {
                    if (mob is Announcer && !mob.Deleted)
                    {
                        Ann = (Announcer)mob;
                    }
                }

                if(Ann == null)
                    Ann = new Announcer();
                Ann.CantWalk = true;
                Ann.Direction = Direction.South;
                Ann.MoveToWorld(a.AnnouncerSpot, a.Map);
                return Ann;
            }

            protected override void OnTick()
            {
                if (DateTime.Now > stagedelay && !started)
                {
                    BeginFight();
                }
                else if ((started && (m_Team1 != null && !m_Team1.allAlive()) || (m_Team2 != null && !m_Team2.allAlive())))
                {
                    FinishFight();
                }
                else if (started && !cfinished)
                {
                    timer.Start();
                }
            }

            #region Tournament Methods
            /// <summary>
            /// Grab the fighters and place them into
            /// the staging area
            /// </summary>
            public void BeginStage()
            {
                if (b.Team1 != null && b.Team2 != null)
                {
                    m_Team1 = b.Team1;
                    m_Team2 = b.Team2;
                }
                else if (b.Team1 != null && b.refWinners)
                {
                    m_Team1 = b.Team1;
                    m_Team2 = b.refMatch1.Winner;
                }
                else if (b.Team1 != null && !b.refWinners)
                {
                    m_Team1 = b.Team1;
                    m_Team2 = b.refMatch1.Loser;
                }
                else if (b.refWinners)
                {
                    m_Team1 = b.refMatch1.Winner;
                    m_Team2 = b.refMatch2.Winner;
                }
                else
                {
                    m_Team1 = b.refMatch1.Loser;
                    m_Team2 = b.refMatch2.Loser;
                }

                MoveTeam(m_Team1, a.Map, a.StagingAreaOne, String.Format("You will have {0} seconds to suit up.", staging.Seconds), false);
                MoveTeam(m_Team2, a.Map, a.StagingAreaTwo, String.Format("You will have {0} seconds to suit up.", staging.Seconds), false);
                stagedelay = DateTime.Now + staging;
                timer = new AnnouncerTimer(this, m_Team1, m_Team2);

                this.Start();
            }

            /// <summary>
            /// Move the fighters to the fighting grounds
            /// </summary>
            public void BeginFight()
            {
                MoveTeam(m_Team1, a.Map, a.FightingAreaOne, "Your fight will begin shortly.", true);
                MoveTeam(m_Team2, a.Map, a.FightingAreaTwo, "Your fight will begin shortly.", true);
                started = true;
            }

            /// <summary>
            /// Move the finished fighters to the appropriate areas.
            /// </summary>
            public void FinishFight()
            {
                this.Stop();

                if (m_Team1.allAlive())
                {
                    b.Winner = m_Team1;
                    b.Loser = m_Team2;
                }
                else
                {
                    b.Winner = m_Team2;
                    b.Loser = m_Team1;
                }

                b.Winner.addWin(b.Loser);
                b.Loser.addLoss(b.Winner);
                switch (t.m_Tournament.Type)
                {
                    case TournamentType.RoundRobin:
                        {
                            MoveTeam(b.Winner, a.Map, a.HoldingArea, "", false);
                            MoveTeam(b.Loser, a.Map, a.HoldingArea, "", false);
                            break;
                        }
                    case TournamentType.SingleElimination:
                        {
                            if (t.m_CurrentRound >= (t.m_Bracket.TotalRounds - 1)) //Final Round
                            {
                                MoveTeam(b.Winner, a.Map, a.ExitArea, "", false);
                                MoveTeam(b.Loser, a.Map, a.ExitArea, "", false);
                            }
                            else if (t.m_CurrentRound >= (t.m_Bracket.TotalRounds - 2)) //Final Four
                            {
                                MoveTeam(b.Winner, a.Map, a.HoldingArea, "", false);
                                MoveTeam(b.Loser, a.Map, a.HoldingArea, "", false);
                            }
                            else
                            {
                                MoveTeam(b.Winner, a.Map, a.HoldingArea, "", false);
                                MoveTeam(b.Loser, a.Map, a.ExitArea, "", false);
                                if (t.m_Tournament.Prizes.ContainsKey("consolation"))
                                    Manager.TournamentReward(t.m_Tournament, "consolation", b.Loser);
                            }                            
                            break;
                        }
                    case TournamentType.DoubleElimination:
                        {
                            break;
                        }
                }

                if (b.Winner.getOwners().Count > 1)
                    announcer.Say(String.Format("Team {0} has defeated Team {1}", b.Winner.getOwners()[0].Name, b.Loser.getOwners()[0].Name));
                else
                    announcer.Say(String.Format("{0} has defeated {1}", b.Winner.getOwners()[0].Name, b.Loser.getOwners()[0].Name));


                a.Occupied = false;
                t.Progress();
            }

            /// <summary>
            /// Move the selected team
            /// </summary>
            /// <param name="team">Team to be moved</param>
            /// <param name="map">Map to be moved to</param>
            /// <param name="location">Point3D Location to be moved to</param>
            /// <param name="msg">Message to tell the players being moved</param>
            /// <param name="paralyze">Should they be paralyzed after the move</param>
            public void MoveTeam(Teams team, Map map, Point3D location, string msg, bool paralyze)
            {
                Mobile m;
                for (int i = 0; i < team.getOwners().Count; i++)
                {
                    m = team.getOwners()[i];
                    m.MoveToWorld(location, map); //Arena Area
                    Manager.RefreshPlayer((PlayerMobile)m);
                    if (msg.Length > 0 && (Manager.IsOnline((PlayerMobile)m)))
                        m.SendMessage(msg);
                    if (paralyze)
                        m.Paralyzed = true;
                }
            }

            /// <summary>
            /// Move the selected team
            /// </summary>
            /// <param name="team">Team to be moved</param>
            /// <param name="map">Map to be moved to</param>
            /// <param name="location">List of Point3D Locations to be moved to</param>
            /// <param name="msg">Message to tell the players being moved</param>
            /// <param name="paralyze">Should they be paralyzed after the move</param>
            public void MoveTeam(Teams team, Map map, List<Point3D> location, string msg, bool paralyze)
            {
                Mobile m;
                for (int i = 0; i < team.getOwners().Count; i++)
                {
                    m = team.getOwners()[i];
                    m.MoveToWorld(location[i], map); //Arena Area
                    Manager.RefreshPlayer((PlayerMobile)m);
                    if (msg.Length > 0 && (Manager.IsOnline((PlayerMobile)m)))
                        m.SendMessage(msg);
                    if (paralyze)
                        m.Paralyzed = true;
                }
            }
            #endregion

            private class AnnouncerTimer : Timer
            {
                private MatchTimer t;
                private Teams Team1, Team2;
                private int countdown;

                public AnnouncerTimer(MatchTimer timer, Teams team1, Teams team2)
                    : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
                {
                    Priority = TimerPriority.OneSecond;
                    t = timer;
                    Team1 = team1;
                    Team2 = team2;
                    countdown = 6;
                }

                protected override void OnTick()
                {
                    if (countdown == 6)
                    {
                        //announcer.Say( String.Format( "The match is {0} v.s. {1}.", m1.Name, m2.Name ) );
                        countdown -= 1;
                    }
                    else if (countdown == 5)
                    {
                        t.announcer.Say(String.Format("The Match begins in {0}", countdown));
                        countdown -= 1;
                    }
                    else if (countdown == 0)
                    {
                        t.announcer.Say("FIGHT!");
                        foreach (Mobile m in Team1.getOwners())
                        {
                            m.Paralyzed = false;
                        }
                        foreach (Mobile m in Team2.getOwners())
                        {
                            m.Paralyzed = false;
                        }
                        //announcer.Delete();
                    }
                    else
                    {
                        t.announcer.Say(String.Format("{0}", countdown));
                        countdown -= 1;
                    }
                    this.Stop();
                }
            }
        }
    }
}