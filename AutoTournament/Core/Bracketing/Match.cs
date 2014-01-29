/****************************************************************\
* File Name : Match.cs                                           *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 2/21/2010                                          *
* Desc.     : Used by the tournament system to set up a bout     *
*             between two teams/players                          *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server.Mobiles;

namespace Tournaments
{
	public class Match
	{
        public bool refWinners;
        private Match m_refMatch1;
        private Match m_refMatch2;
        private Teams m_Team1;
        private Teams m_Team2;
        private Teams m_Winner;
        private Teams m_Loser;

        public Match refMatch1
        {
            get { return m_refMatch1; }
            set { m_refMatch1 = value; }
        }
        public Match refMatch2
        {
            get { return m_refMatch2; }
            set { m_refMatch2 = value; }
        }
        public Teams Team1
        {
            get { return m_Team1; }
            set { m_Team1 = value; }
        }
        public Teams Team2
        {
            get { return m_Team2; }
            set { m_Team2 = value; }
        }
        public Teams Winner
        {
            get { return m_Winner; }
            set { m_Winner = value; }
        }
        public Teams Loser
        {
            get { return m_Loser; }
            set { m_Loser = value; }
        }

        /// <summary>
        /// Sets a bout between two teams
        /// </summary>
        /// <param name="team1">the first team</param>
        /// <param name="team2">the second team</param>
		public Match( Teams team1, Teams team2)
		{
            m_Team1 = team1;
            m_Team2 = team2;
		}

        /// <summary>
        /// Sets a bout between two teams
        /// </summary>
        /// <param name="team1">the first team</param>
        /// <param name="team2">the second team</param>
        public Match(Teams team1, ref Match refmatch1, bool winners)
        {
            refWinners = winners;
            m_Team1 = team1;
            m_refMatch1 = refmatch1;
        }

        /// <summary>
        /// Sets a bout between two teams
        /// </summary>
        /// <param name="team1">the first team</param>
        /// <param name="team2">the second team</param>
        public Match(ref Match refmatch1, ref Match refmatch2, bool winners)
        {
            refWinners = winners;
            m_refMatch1 = refmatch1;
            m_refMatch2 = refmatch2;
        }
	}
}
