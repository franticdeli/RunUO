/***************************************************************\
* File Name : UpcomingEventsGump.cs                             *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 02/17/2013                                        *
* Desc.     : A gump that lists the upcoming events that are    *
*               scheduled. Allows the viewing of event details  *
*               and registering for the event.                  *
\***************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  02/17/13  | Initial File                                      |
\****************************************************************/

using System;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Commands;

using EventScheduler;

using Tournaments;
using Tournaments.Items;

namespace Tournaments.Gumps
{
    public class TournamentInfoGump : Gump
    {
        Mobile caller;
        Tournament t;

        public static void Initialize()
        {
            CommandSystem.Register("NewTournament", AccessLevel.GameMaster, new CommandEventHandler(NewTournament_OnCommand));
        }

        [Usage("NewTournament")]
        [Description("Begins creating a new tournament date.")]
        public static void NewTournament_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            m.SendGump(new TournamentInfoGump(m, new Tournament()));
        }

        public TournamentInfoGump(Mobile from, Tournament tournament) : this()
        {
            caller = from;
            t = tournament;
            TournamentInfo();
        }

        public TournamentInfoGump() : base( 200, 200 )
        {
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
        }

        public void TournamentInfo()
        {
            AddPage(0);

            if (caller.AccessLevel >= AccessLevel.GameMaster)// && (DateTime.Now + TimeSpan.FromMinutes(30)) < t.Date)
            {
                AddBackground(0, 44, 260, 270, 9250);
            }
            else
                AddBackground(0, 44, 260, 210, 9250);

            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(85, 14, 0, @"Tournament Info");

            AddLabel(25, 60, 0, @"Type:");
            AddLabel(100, 60, 0, t.Type.ToString());

            AddLabel(25, 90, 0, @"Team Size:");
            AddLabel(100, 90, 0, t.TeamSize.ToString());

            AddLabel(25, 120, 0, @"Supplied:");
            AddLabel(100, 120, 0, t.Supplied.ToString().ToUpper());

            AddLabel(25, 150, 0, @"Date:");
            AddLabel(100, 150, 0, String.Format("{0}/{1}/{2}", t.Date.Month, t.Date.Day, t.Date.Year));

            AddLabel(25, 180, 0, @"Time:");
            AddLabel(100, 180, 0, t.Date.ToString("hh:mm tt"));

            AddLabel(25, 210, 0, @"Prizes:");
            string prizes = "";
            int linech = 0;
            foreach (string s in t.Prizes.Keys)
            {
                if ((linech+s.Length) > 19)
                {
                    prizes += '\n' + s;
                    linech = s.Length;
                }
                else
                {
                    prizes += s + ',';
                    linech += s.Length+1;
                }
            }
            AddHtml(100, 210, 105, 60, prizes.TrimEnd(','), false, false);
            AddButton(218, 210, 4005, 4007, 6, GumpButtonType.Reply, 0); // Prizes Button

            string arenas = "";
            foreach (string s in t.ArenaSets)
            {
                if (arenas.Equals(""))
                    arenas += s;
                else
                    arenas += "," + s;
            }
            AddLabel(25, 240, 0, @"Arena Sets:");
            AddLabel(100, 240, 0, (arenas.Length < 14)? arenas : ((t.ArenaSets.Count == 1) ? "Arena Set" : "Multiple Arenas" ));

            if (caller.AccessLevel >= AccessLevel.GameMaster)// && (DateTime.Now + TimeSpan.FromMinutes(30)) < t.Date)
            {
                AddButton(218, 60, 4005, 4007, 1, GumpButtonType.Reply, 0); // Type Button
                AddButton(218, 90, 4005, 4007, 2, GumpButtonType.Reply, 0); // Team Size Button
                AddButton(218, 120, 4005, 4007, 3, GumpButtonType.Reply, 0); // Supplied Button
                AddButton(218, 150, 4005, 4007, 4, GumpButtonType.Reply, 0); // Date Button
                AddButton(218, 180, 4005, 4007, 5, GumpButtonType.Reply, 0); // Time Button
                AddButton(218, 240, 4005, 4007, 7, GumpButtonType.Reply, 0); // Arena Sets Button
                AddButton(217, 268, 4023, 4025, 8, GumpButtonType.Reply, 0); // OK Button
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch(info.ButtonID)
            {
                case 0:
					break;
                case 1:
                {
                    from.SendGump(new TournamentTypeGump(from, t));
                    break;
                }
                case 2:
                {
                    from.SendGump(new TournamentTeamSizeGump(from, t));
                    break;
                }
                case 3:
                {
                    t.Supplied = !t.Supplied;
                    from.SendGump(new TournamentInfoGump(from, t));
                    break;
                }
                case 4:
                {
                    from.SendGump(new TournamentDateGump(from, t));
                    break;
                }
                case 5:
                {
                    from.SendGump(new TournamentTimeGump(from, t));
                    break;
                }
                case 6:
                {
                    from.SendGump(new TournamentPrizeGump(from, t));
                    break;
                }
                case 7:
                {
                    from.SendGump(new TournamentArenasGump(from, t, 0));
                    break;
                }
                case 8:
                {
                    if (t.Date > DateTime.Now && ArenaControl.Arenas.ContainsKey(t.TeamSize) &&
                        ArenaControl.Arenas[t.TeamSize] != null && ArenaControl.Arenas[t.TeamSize].Count > 0)
                    {
                        XMLDates.RemoveTournament(t); // Ensure no duplicates
                        XMLDates.AddTournament(t);
                        XMLDates.Save();
                        from.SendMessage("The tournament has been added.");
                    }
                    else if (!(t.Date > DateTime.Now))
                        from.SendMessage("The date has already passed.");
                    else
                        from.SendMessage("There are no arenas for " + t.TeamSize.ToString() +".");
                    break;
                }
            }
        }
    }
}