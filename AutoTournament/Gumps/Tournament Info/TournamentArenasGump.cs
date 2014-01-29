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
using Server.Network;
using Server.Commands;

using Tournaments.Items;

namespace Tournaments.Gumps
{
    public class TournamentArenasGump : Gump
    {
        Mobile caller;
        Tournament t;
        public int CurrentPage;
        private List<string> sets;

        public TournamentArenasGump(Mobile from, Tournament tournament, int page)
            : this()
        {
            caller = from;
            t = tournament;
            TypeInfo(page);
        }

        public TournamentArenasGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
        }

        public void TypeInfo(int page)
        {
            CurrentPage = page;
            sets = ArenaControl.GetSets(t.TeamSize);
            int index = sets.Count / 5;
            int pages = (sets.Count % 5 == 0 ? index : (index + 1));
            int i;

            AddPage(0);
            AddBackground(0, 44, 260, 110 + index * 30, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(100, 14, 0, @"Arena Sets");

            if (sets.Count < ((page + 1) * 5))
                index = sets.Count - page * 5;
            else
                index = 5;

            for (i = 1; i < index+1; i++ )
            {
                AddLabel(60, 30 + i * 30, 0, sets[i-1]);
                AddButton(218, 30 + i * 30, t.ArenaSets.Contains(sets[i-1]) ? 4017 : 4020,
                    t.ArenaSets.Contains(sets[i-1]) ? 4019 : 4022, i, GumpButtonType.Reply, 0);
            }

            AddButton(218, 58+index*30, 4023, 4025, 0, GumpButtonType.Reply, 0); // OK Button
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                {
                    from.SendGump(new TournamentInfoGump(from, t));
                    break;
                }
                default:
                {
                    if (t.ArenaSets.Contains(sets[info.ButtonID-1]))
                        t.ArenaSets.Remove(sets[info.ButtonID-1]);
                    else
                        t.ArenaSets.Add(sets[info.ButtonID-1]);

                    from.SendGump(new TournamentArenasGump(from, t, CurrentPage));
                    break;
                }
            }
        }
    }
}