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
using Server.Targeting;

using Tournaments;
using Tournaments.Items;

namespace Tournaments.Gumps
{
    public class TeamConfirmGump : Gump
    {
        Mobile caller;
        Tournament t;
        Teams team;
        int size;

        public TeamConfirmGump(Mobile from, Tournament tournament, Teams fullteam)
            : this()
        {
            caller = from;
            t = tournament;
            team = fullteam;
            size = team.getOwners().Count-1;
            TeamInfo();
        }

        public TeamConfirmGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
        }

        public void TeamInfo()
        {
            AddPage(0);
            AddBackground(0, 44, 260, 110 + size * 30, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(70, 14, 0, @"Team Configuration");

            AddLabel(25, 60, 0, @"Tournament:");
            AddLabel(120, 60, 0, String.Format("{0}", t.Date.ToString("MM/dd/yy")));
            AddButton(218, 60, 4011, 4013, 2, GumpButtonType.Reply, 0);

            AddLabel(25, 90, 0, @"Captain:");
            AddLabel(100, 90, 0, team.getOwners()[0].Name);

            for (int i = 0; i < size; i++)
            {
                AddLabel(25, 120 + i * 30, 0, @"Team Member:");
                AddLabel(120, 120 + i * 30, 0, team.getOwners().Count > (i + 1) ? team.getOwners()[i + 1].Name : "");
            }

            AddButton(217, 118 + size * 30, 4023, 4025, 1, GumpButtonType.Reply, 0); // OK Button
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                {
                    team.IsFullTeam(t, false);
                    break;
                }
                case 1:
                {
                    team.IsFullTeam(t, true);
                    break;
                }
                case 2:
                {
                    from.SendGump(new TournamentInfoGump(from, t));
                    break;
                }
            }
        }
    }
}