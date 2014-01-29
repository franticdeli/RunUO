/***************************************************************\
* File Name : Tournament.cs                                     *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 02/17/2013                                        *
* Desc.     : Defines a Tournament                              *
\***************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  02/17/13  | Initial File                                      |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Items;

using EventScheduler;

using Tournaments.Items;

namespace Tournaments
{
    public enum TournamentType
    {
        RoundRobin,
        SingleElimination,
        DoubleElimination,
        Hybrid
    }

    public class Tournament
    {
        #region Initialization
        /// <summary>
        /// List of teams registered for the tournament
        /// </summary>
        private List<Teams> m_Teams;

        /// <summary>
        /// Date the tournament is to be on
        /// </summary>
        private DateTime m_Date;

        /// <summary>
        /// The sets of arenas that may be used by this tournament
        /// </summary>
        private List<string> m_ArenaSets;

        /// <summary>
        /// What type of tournament is it?
        /// Round Robin, Single Elimination, etc
        /// </summary>
        private TournamentType m_Type;

        /// <summary>
        /// What is the size of the teams?
        /// 1v1, 2v2, etc
        /// </summary>
        private ArenaType m_TeamSize;

        /// <summary>
        /// Determines if the players are meant to be supplied to compete
        /// </summary>
        private bool m_Supplied;

        /// <summary>
        /// Collection of the prizes for the tournament
        /// </summary>
        private Dictionary<string,Item> m_Prizes;
        #endregion Initialization

        #region Gets/Sets
        /// <summary>
        /// Gets and sets the tournament's registered teams
        /// </summary>
        public List<Teams> Teams
        {
            get { return m_Teams; }
            set { m_Teams = value; }
        }

        /// <summary>
        /// Gets and sets the tournament date
        /// Removes all of the teams registered if the team size is changed
        /// </summary>
        public DateTime Date
        {
            get { return m_Date; }
            set
            {
                foreach (Teams team in m_Teams)
                {
                    foreach (PlayerMobile pm in team.getOwners())
                    {
                        if (Manager.IsOnline(pm))
                            pm.SendMessage(String.Format("The {0} tournament on {1} at {2} has been removed.", TeamSize, Date.ToString("MM/dd/yy"), Date.ToString("hh:mm tt")));
                    }
                }
                m_Teams.Clear();
                m_Date = value;
            }
        }

        /// <summary>
        /// Gets and sets the sets the tournament is intended to use
        /// </summary>
        public List<string> ArenaSets
        {
            get { return m_ArenaSets; }
            set { m_ArenaSets = value; }
        }

        /// <summary>
        /// Gets and sets the tournament type
        /// Removes all of the teams registered if the team size is changed
        /// </summary>
        public TournamentType Type
        {
            get { return m_Type; }
            set 
            {
                foreach (Teams team in m_Teams)
                {
                    foreach (PlayerMobile pm in team.getOwners())
                    {
                        if (Manager.IsOnline(pm))
                            pm.SendMessage(String.Format("The {0} tournament on {1} at {2} has been removed.", TeamSize, Date.ToString("MM/dd/yy"), Date.ToString("hh:mm tt")));
                    }
                }
                m_Teams.Clear();
                m_Type = value; 
            }
        }

        /// <summary>
        /// Gets and sets the tournament team size
        /// Removes all of the teams registered if the team size is changed
        /// </summary>
        public ArenaType TeamSize
        {
            get { return m_TeamSize; }
            set 
            {
                foreach (Teams team in m_Teams)
                {
                    foreach (PlayerMobile pm in team.getOwners())
                    {
                        if(Manager.IsOnline(pm))
                            pm.SendMessage(String.Format("The {0} tournament on {1} at {2} has been removed.", TeamSize, Date.ToString("MM/dd/yy"), Date.ToString("hh:mm tt")));
                    }
                    Teams.Remove(team);
                }
                m_Teams.Clear();
                m_TeamSize = value; 
            }
        }

        /// <summary>
        /// Gets and sets whether the tournament is supplied
        /// </summary>
        public bool Supplied
        {
            get { return m_Supplied; }
            set { m_Supplied = value; }
        }

        /// <summary>
        /// Gets and sets the tournament's prize list
        /// </summary>
        public Dictionary<string, Item> Prizes
        {
            get { return m_Prizes; }
            set { m_Prizes = value; }
        }
        #endregion Gets/Sets

        /// <summary>
        /// Main Constructor
        /// </summary>
        public Tournament()
        {
            m_Teams = new List<Teams>();
            m_Date = DateTime.Now;
            m_ArenaSets = new List<string>();
            m_Type = TournamentType.SingleElimination;
            m_TeamSize = ArenaType.OneVsOne;
            m_Supplied = true;
            m_Prizes = new Dictionary<string,Item>();
        }

        /// <summary>
        /// Adds a team to the tournament list
        /// </summary>
        /// <param name="m">team to add</param>
        /// <returns>did the team add?</returns>
        public bool AddTeam(Teams m)
        {
            bool Add = true;
            
            foreach(PlayerMobile pm in m.getOwners())
                if (IsRegistered(pm))
                    Add = false;

            if (Add)
            {
                Teams.Add(m);
                XMLDates.Save();
            }

            return Add;
        }

        /// <summary>
        /// Removes a team from the tournament list
        /// </summary>
        /// <param name="m">team to be removed</param>
        public void RemoveTeam(Teams m)
        {
            List<Teams> remove = new List<Teams>();
            foreach (PlayerMobile pm in m.getOwners())
            {
                remove.Add(GetTeam(pm));
            }

            foreach (Teams team in remove)
            {
                foreach(PlayerMobile pm in team.getOwners())
                {
                    if (Manager.IsOnline(pm))
                        pm.SendMessage(String.Format("You have been removed from the {0} tournament on {1} at {2}.", TeamSize, Date.ToString("MM/dd/yy"), Date.ToString("hh:mm tt")));
                }
                Teams.Remove(team);
            }

            XMLDates.Save();
        }

        public bool IsRegistered(PlayerMobile pm)
        {
            bool IsRegistered = false;
            foreach (Teams team in Teams)
                if(team.getOwners().Contains(pm))
                    IsRegistered = true;

            return IsRegistered;
        }

        /// <summary>
        /// Gets the team the player is on
        /// </summary>
        /// <param name="pm">player to get the team of</param>
        /// <returns>team the player is on</returns>
        private Teams GetTeam(PlayerMobile pm)
        {
            Teams t = null;
            foreach (Teams team in Teams)
                if (team.getOwners().Contains(pm))
                    t = team;

            return t;
        }

        /// <summary>
        /// Adds a prize to the prizes collection
        /// </summary>
        /// <param name="place">place the prize is for</param>
        /// <param name="prize">prize to be given</param>
        /// <returns>is the prize added?</returns>
        public bool AddPrize(string place, Item prize)
        {
            bool Add = true;

            if(m_Prizes.ContainsKey(place) )
                Add = false;

            if (Add)
            {
                m_Prizes.Add(place, prize);
                XMLDates.Save();
            }
            else
            {
                m_Prizes[place] = prize;
                XMLDates.Save();
            }

            return Add;
        }

        /// <summary>
        /// Removes a prize from the prizes collection
        /// </summary>
        /// <param name="place">place the prize is for</param>
        public void RemovePrize(string place)
        {
            if (m_Prizes.ContainsKey(place))
            {
                m_Prizes.Remove(place);
                XMLDates.Save();
            }
        }
    }
}
