/***************************************************************\
* File Name : Teams.cs                                          *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 5/18/2010                                         *
* Desc.     : Team object to more easily sort the players and   *
*             to allow the tournament to support competitions   *
*             that involve more that 1v1.                       *
*             Saves the teams/participants record and is used   *
*             to determine placing in the tournaments.          *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Misc;

using EventScheduler;

using Tournaments.Gumps;

namespace Tournaments
{
    public class Teams
	{
		/// <summary>
		/// All of the teams this team has lost to
		/// </summary>
		private List<Teams> losses = new List<Teams>();
		
		/// <summary>
		/// All of the teams this team has beaten
		/// </summary>
		private List<Teams> wins = new List<Teams>();
		
		/// <summary>
		/// The losses in the current tournament
		/// </summary>
		private List<Teams> curLosses = new List<Teams>();
		
		/// <summary>
		/// The wins in the current tournament
		/// </summary>
		private List<Teams> curWins = new List<Teams>();
		
		/// <summary>
		/// A list of the owner(s) of this team
		/// </summary>
		private List<Mobile> owner = new List<Mobile>();

        /// <summary>
        /// List of invite accepts of the team
        /// </summary>
        private List<bool> Accepted = new List<bool>();
		
		/// <summary>
		/// Boolean that determines if a member is offline
		/// </summary>
		private bool offlineMembers = false;
		
		/// <summary>
		/// Constructor for a team with multiple players
		/// </summary>
		/// <param name="m">the team members </param>
		public Teams( List<Mobile> m )
		{
			this.owner = m;
		}
		
		/// <summary>
		/// Constructor for a single person team
		/// </summary>
		/// <param name="m">the contestant</param>
		public Teams( PlayerMobile m )
		{
				owner.Add(m);
		}
		
		/// <summary>
		/// Returns the amount of losses the team has
		/// </summary>
		/// <returns>the total amount of losses</returns>
		public int getLosses()
		{
			return losses.Count;
		}
		
		/// <summary>
		/// Returns the amount of wins the team has
		/// </summary>
		/// <returns>the total amount of the wins</returns>
		public int getWins()
		{
			return wins.Count;
		}
		
		/// <summary>
		/// Returns the current amount of losses the team has
		/// </summary>
		/// <returns>the current amount of losses</returns>
		public int getCurLosses()
		{
			return curLosses.Count;
		}
		
		/// <summary>
		/// Returns the current amount of wins the team has
		/// </summary>
		/// <returns>the current amount of wins</returns>
		public int getCurWins()
		{
			return curWins.Count;
		}
		
		/// <summary>
		/// Adds a team to this teams loss record
		/// </summary>
		/// <param name="team">the team that this team lost to</param>
		public void addLoss( Teams team )
		{
			this.losses.Add( team );
			this.curLosses.Add( team );
		}
		
		/// <summary>
		/// Adds a team that this team has beaten to the list
		/// </summary>
		/// <param name="team">the team that this team beat</param>
		public void addWin( Teams team )
		{
			this.wins.Add( team );
			this.curWins.Add( team );
		}
		
		/// <summary>
		/// Returns the list of the team members
		/// </summary>
		/// <returns>the team members</returns>
		public List<Mobile> getOwners()
		{
			return owner;
		}
		
		/// <summary>
		/// Sets if this team has some offline team members
		/// </summary>
		/// <param name="hasOffline">boolean determining if offline member exists</param>
		public void tagOffline( bool hasOffline )
		{
			this.offlineMembers = hasOffline;
		}
		
		/// <summary>
		/// Returns if this team has an offline member
		/// </summary>
		/// <returns>boolean stating if offline member exists</returns>
		public bool hasOffline()
		{
			return this.offlineMembers;
		}
		
		/// <summary>
		/// Returns of this team has fought the team to be fought
		/// </summary>
		/// <param name="team"> The team to be checked</param>
		/// <returns>boolean stating if the team has been fought</returns>
		public bool hasFought( Teams team )
		{
			if( this.curLosses.Contains( team ) || this.curWins.Contains( team ) )
				return true;
			else
				return false;
		}

		public bool hasLostTo( Teams team )
		{
			if( this.curLosses.Contains(team) )
				return true;
			else
				return false;
		}
		
		/// <summary>
		/// Returns if all members of the team are alive or not
		/// </summary>
		/// <returns>boolean that determines if all are alive or not</returns>
		public bool allAlive()
		{
			bool alive = true;
			Mobile m;
			
			for( int i = 0; i < this.owner.Count; i++ )
			{
				m = this.owner[i];
				
				if( !m.Alive )
					alive = false;
			}
			
			return alive;
		}

        public void IsFullTeam( Tournament t, bool accept)
        {
            Accepted.Add(accept);

            if (!Accepted.Contains(false) && (Accepted.Count == (getOwners().Count - 1)))
            {
                bool added = t.AddTeam(this);
                foreach (Mobile m in getOwners())
                {
                    if (added)
                    {
                        if (Manager.IsOnline((PlayerMobile)m))
                            m.SendMessage(String.Format("You have been registered for the {0} tournament on {1} at {2}.", t.TeamSize, t.Date.ToString("MM/dd/yy"), t.Date.ToString("hh:mm tt")));
                    }
                    else if (Manager.IsOnline((PlayerMobile)m))
                        m.SendMessage(String.Format("One or more members are already registered for the {0} tournament on {1} at {2}.", t.TeamSize, t.Date.ToString("MM/dd/yy"), t.Date.ToString("hh:mm tt")));

                    if (m.HasGump(typeof(UpcomingEventsGump)))
                    {
                        UpcomingEventsGump g = (UpcomingEventsGump)m.FindGump(typeof(UpcomingEventsGump));
                        int page = g.CurrentPage;
                        m.CloseGump(typeof(UpcomingEventsGump));
                        m.SendGump(new UpcomingEventsGump(m, page));
                    }
                }
            }
            else if (Accepted.Contains(false) && (Accepted.Count == (getOwners().Count - 1)))
            {
                foreach (Mobile m in getOwners())
                    if (Manager.IsOnline((PlayerMobile)m))
                        m.SendMessage(String.Format("One or more members declined the team invite for the {0} tournament on {1} at {2}.", t.TeamSize, t.Date.ToString("MM/dd/yy"), t.Date.ToString("hh:mm tt")));
            }
        }
		
	}
}