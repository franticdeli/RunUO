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
    public class ArenaSetGump : Gump
    {
        Mobile caller;
        ArenaControl a;
        public int CurrentPage;
        private List<string> sets;

        public ArenaSetGump(Mobile from, ArenaControl arena, int page)
            : this()
        {
            caller = from;
            a = arena;
            TypeInfo(page);
        }

        public ArenaSetGump()
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
            sets = ArenaControl.GetSets(a.Type);
            int index = sets.Count / 5;
            int pages = (sets.Count % 5 == 0 ? index : (index + 1));
            int i;

            AddPage(0);
            AddBackground(0, 44, 260, 240, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(100, 14, 0, @"Arena Sets");

            if (sets.Count < ((page + 1) * 5))
                index = sets.Count - page * 5;
            else
                index = 5;

            for (i = 1; i < index+1; i++ )
            {
                AddLabel(60, 60 + i * 30, 0, sets[i-1]);
                AddRadio(25, 60 + i * 30, 208, 209, sets[i-1].Equals(a.Set) ? true : false, i);
            }

            AddLabel(60, 60, 0, String.Format("New Set: {0}",a.Name));
            AddRadio(25, 60, 208, 209, false, 0);

            AddButton(218, 248, 4005, 4007, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                {
                    break;
                }
                case 1:
                {
                    if (info.Switches != null && info.Switches.Length > 0)
                    {
                        int radio = info.Switches[0];
                        if (radio != 0 && sets != null && sets.Count > 0)
                            a.Set = sets[radio-1];
                        else
                            a.Set = a.Name;
                    }

                    from.SendGump(new ArenaInfoGump(from, a));
                    break;
                }
            }
        }
    }
}