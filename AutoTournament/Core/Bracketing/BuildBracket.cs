/***************************************************************\
* File Name : BuildBracket.cs                                   *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 5/18/2010                                         *
* Desc.     : Sorts and sets up the various teams and places    *
*             them into a bracket.                              *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
|  02/17/13  | Rework Round Creation                             |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;

namespace Tournaments
{
    public class BuildBracket
    {
        public int TotalRounds;
        public Dictionary<int, List<Match>> Bracket;

        #region Variables and Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="teams">the list of the teams involved in the tourney</param>
        public BuildBracket()
        {
            TotalRounds = 0;
            Bracket = new Dictionary<int, List<Match>>();
        }
        #endregion

        #region Round Robin
        /*
		 * Description:
		 * 		Sets up a Round Robin Tournament.
		 * Each contestant or team of contestants will fight each
		 * of the other enlisted teams at least once.
		 */
        /// <summary>
        /// Constructs a list of bouts for a Round Robin style
        /// tournament. Meant to be used with a low number of
        /// contestants or teams.
        /// </summary>
        /// <returns> A list of bouts to be done</returns>
        public List<Match> RoundRobin(List<Teams> teams)
        {
            int rounds = this.countdownSum(teams.Count - 1);
            Teams team1, team2;
            List<Match> allRounds = new List<Match>();
            int MatchesPerRound;
            if (teams.Count % 2 == 0)
                MatchesPerRound = teams.Count/2;
            else
                MatchesPerRound = (teams.Count-1) / 2;

            for (int i = 0; i < rounds; i++)
            {
                team1 = teams[i];

                for (int j = i + 1; j < teams.Count; j++)
                {
                    team2 = teams[j];

                    allRounds.Add(new Match(team1, team2));
                }
            }

            return allRounds;
        }
        #endregion

        #region Single Elimination
        /// <summary>
        /// Creates a Single Elimination Bracket given
        /// a list of teams that are participating
        /// </summary>
        /// <param name="teams">Participants</param>
        public void SingleElimination(List<Teams> teams)
        {
            int prelims, ideal;
            Match refMatch1;
            Match refMatch2;
            List<Teams> prelimteams = new List<Teams>();
            List<Teams> contestants = new List<Teams>();

            contestants.AddRange(teams);
            TotalRounds = (int)Math.Ceiling(log2(contestants.Count));
            ideal = (int)Math.Pow(2, TotalRounds);

            if (ideal != contestants.Count)
                prelims = ideal - contestants.Count;
            else
                prelims = 0;

            // Get first round
            Bracket.Add(0, new List<Match>()); // Initialize the First Round
            for (int i = 0; i < contestants.Count - prelims; i += 2)
            {
                Bracket[0].Add(new Match(contestants[i], contestants[i + 1]));
                prelimteams.Add(contestants[i]);
                prelimteams.Add(contestants[i + 1]);
            }

            if (contestants.Count > 3)
            {
                if (prelims == 0)
                {
                    // Build the Rest of the Rounds
                    for (int i = 1; i < TotalRounds - 1; i++)
                    {
                        Bracket.Add(i, new List<Match>()); // Initialize the Next Round
                        for (int j = 0; j < ideal / (2 * i); j += 2)
                        {
                            refMatch1 = Bracket[i - 1][j];
                            refMatch2 = Bracket[i - 1][j + 1];
                            Bracket[i].Add(new Match(ref refMatch1, ref refMatch2, true));
                        }
                    }
                }
                else
                {
                    // Build Round 2
                    Bracket.Add(1, new List<Match>()); // Initialize the second Round
                    int index = 0;

                    foreach (Teams t in prelimteams)
                        contestants.Remove(t);
                    foreach (Match m in Bracket[0])
                    {
                        if (index % 2 == 0)
                            contestants.Insert(index, m.Winner);
                        else
                        {
                            contestants.Reverse();
                            contestants.Insert(index, m.Winner);
                            contestants.Reverse();
                        }
                        index++;
                    }

                    index = 0;
                    for (int i = 0; i < contestants.Count; i+=2 )
                    {
                        if (contestants[i] != null && contestants[i+1] != null)
                            Bracket[1].Add(new Match(contestants[i], contestants[i + 1]));
                        else if (contestants[i] == null && contestants[i + 1] != null)
                        {
                            refMatch1 = Bracket[0][index];
                            Bracket[1].Add(new Match(contestants[i + 1], ref refMatch1, true));
                            index++;
                        }
                        else if (contestants[i] != null && contestants[i + 1] == null)
                        {
                            refMatch1 = Bracket[0][index];
                            Bracket[1].Add(new Match(contestants[i], ref refMatch1, true));
                            index++;
                        }
                        else if (contestants[i] == null && contestants[i + 1] == null)
                        {
                            refMatch1 = Bracket[0][index];
                            refMatch2 = Bracket[0][index+1];
                            Bracket[1].Add(new Match(ref refMatch1, ref refMatch2, true));
                            index += 2;
                        }
                    }

                    // Build the Rest of the Rounds
                    for (int i = 2; i < TotalRounds - 1; i++)
                    {
                        Bracket.Add(i, new List<Match>()); // Initialize the Next Round
                        for (int j = 0; j < ideal / (2 * i); j += 2)
                        {
                            refMatch1 = Bracket[i - 1][j];
                            refMatch2 = Bracket[i - 1][j + 1];
                            Bracket[i].Add(new Match(ref refMatch1, ref refMatch2, true));
                        }
                    }
                }

                // Build Final Round
                int round = TotalRounds - 1;
                if (!Bracket.ContainsKey(round))
                    Bracket.Add(round, new List<Match>()); // Initialize the final round
                refMatch1 = Bracket[round - 1][0];
                refMatch2 = Bracket[round - 1][1];
                Bracket[round].Add(new Match(ref refMatch1, ref refMatch2, false)); // for third
                Bracket[round].Add(new Match(ref refMatch1, ref refMatch2, true)); // for first
            }
            else if (TotalRounds > 1) // 3 Team Tournament
            {
                // Build Final Round
                int round = TotalRounds - 1;
                Bracket.Add(round, new List<Match>()); // Initialize the final round
                refMatch1 = Bracket[round - 1][0];
                foreach (Teams t in prelimteams)
                    contestants.Remove(t);

                Bracket[round].Add(new Match(contestants[0], ref refMatch1, true)); // for first
            }
        }

        /// <summary>
        /// Gets the Round specified from the Bracket
        /// </summary>
        /// <param name="currentround">Round to get</param>
        /// <returns></returns>
        public List<Match> SEGetRound(int currentround)
        {
            return Bracket[currentround];
        }

        /// <summary>
        /// Gets the winners for the Single Elimination Bracket
        /// </summary>
        /// <param name="t"></param>
        public void SEGetWinners(Tournament t)
        {
            List<Match> Finals = Bracket[TotalRounds - 1];

            if (Finals.Count == 2)
            {
                if (t.Prizes.ContainsKey("third"))
                    Manager.TournamentReward(t, "third", Finals[0].Winner);
                if (t.Prizes.ContainsKey("second"))
                    Manager.TournamentReward(t, "second", Finals[1].Loser);
                if (t.Prizes.ContainsKey("first"))
                    Manager.TournamentReward(t, "first", Finals[1].Winner);
            }
            else if (Finals.Count == 1)// Fix for 3 Team Tournament
            {
                if (t.Prizes.ContainsKey("third"))
                    Manager.TournamentReward(t, "third", Bracket[TotalRounds-2][0].Loser);
                if (t.Prizes.ContainsKey("second"))
                    Manager.TournamentReward(t, "second", Finals[0].Loser);
                if (t.Prizes.ContainsKey("first"))
                    Manager.TournamentReward(t, "first", Finals[0].Winner);
            }
            else // Why were there only 2 Teams?? this was a bit overkill...
            {
                if (t.Prizes.ContainsKey("second"))
                    Manager.TournamentReward(t, "second", Finals[0].Loser);
                if (t.Prizes.ContainsKey("first"))
                    Manager.TournamentReward(t, "first", Finals[0].Winner);
            }
        }
        #endregion

        #region Double Elimination
        public void DoubleElimination(List<Teams> teams)
        {
        }
        #endregion

        #region Math Methods
        /// <summary>
        /// Countdown from a designated number while
        /// each and every one starting with the highest
        /// designated number.
        /// </summary>
        /// <param name="num">the highest designated number</param>
        /// <returns>the sum of the countdown</returns>
        public int countdownSum(int num)
        {
            int sum = 0;

            for (int i = num; i > 0; i--)
            {
                sum += i;
            }

            return sum;
        }

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
        /// Extracts the numbers after the decimal point.
        /// Then checks if they are greater than 0.
        /// If so, the whole number before the decimal point is rounded up.
        /// If 0, the whole number stays the same.
        /// </summary>
        /// <param name="var">the variable to be rounded up</param>
        /// <returns>the value that was forcefully rounded up</returns>
        private int ForceRoundUp(double var)
        {
            double excess;
            double num = (int)var;

            excess = var - num;

            if (excess > 0)
            {
                num += 1;
            }

            return (int)num;
        }
        #endregion
    }
}
