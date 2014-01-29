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

using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;

namespace Tournaments.Gumps
{
    public class TournamentTimeGump : Gump
    {
        Mobile caller;
        Tournament t;

        public TournamentTimeGump(Mobile from, Tournament tournament) : this()
        {
            caller = from;
            t = tournament;
            TimeInfo();
        }

        public TournamentTimeGump() : base( 200, 200 )
        {
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
        }

        public void TimeInfo()
        {
            AddPage(0);
            AddBackground(0, 44, 260, 144, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(78, 14, 0, @"Tournament Time");

            AddButton(52, 75, 5600, 5604, 2, GumpButtonType.Reply, 0);
            AddLabel(51, 94, 0, t.Date.ToString("hh"));
            AddButton(52, 120, 5602, 5606, 3, GumpButtonType.Reply, 0);

            AddLabel(78, 94, 0, @":");

            AddButton(95, 55, 5600, 5604, 7, GumpButtonType.Reply, 0);
            AddButton(95, 75, 5600, 5604, 4, GumpButtonType.Reply, 0);
            AddLabel(94, 94, 0, t.Date.ToString("mm"));            
            AddButton(95, 120, 5602, 5606, 5, GumpButtonType.Reply, 0);
            AddButton(95, 140, 5602, 5606, 8, GumpButtonType.Reply, 0);
            
           
            AddLabel(129, 94, 0, @":");
            AddLabel(142, 94, 0, t.Date.ToString("ss"));
            
            AddButton(187, 75, 5600, 5604, 6, GumpButtonType.Reply, 0);
            AddLabel(180, 94, 0, t.Date.ToString("tt"));
            AddButton(187, 120, 5602, 5606, 6, GumpButtonType.Reply, 0);

            AddButton(218, 154, 4005, 4007, 1, GumpButtonType.Reply, 0);
        }        

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch(info.ButtonID)
            {
                case 0:
				{

					break;
				}
				case 1:
				{
                    from.SendGump(new TournamentInfoGump(from, t));
					break;
				}
				case 2:
				{
                    t.Date = t.Date.AddHours(1);
                    from.SendGump(new TournamentTimeGump(from, t));
					break;
				}
				case 3:
				{
                    t.Date = t.Date.AddHours(-1);
                    from.SendGump(new TournamentTimeGump(from, t));
                    break;
				}
				case 4:
				{
                    t.Date = t.Date.AddMinutes(1);
                    from.SendGump(new TournamentTimeGump(from, t));
                    break;
				}
				case 5:
				{
                    t.Date = t.Date.AddMinutes(-1);
                    from.SendGump(new TournamentTimeGump(from, t));
					break;
				}
				case 6:
				{
                    t.Date = t.Date.AddHours(12);
                    from.SendGump(new TournamentTimeGump(from, t));
					break;
				}
                case 7:
                {
                    t.Date = t.Date.AddMinutes(15);
                    from.SendGump(new TournamentTimeGump(from, t));
                    break;
                }
                case 8:
                {
                    t.Date = t.Date.AddMinutes(-15);
                    from.SendGump(new TournamentTimeGump(from, t));
                    break;
                }
            }
        }
    }
}