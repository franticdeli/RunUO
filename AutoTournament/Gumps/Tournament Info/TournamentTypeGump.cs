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
    public class TournamentTypeGump : Gump
    {
        Mobile caller;
        Tournament t;

        public TournamentTypeGump(Mobile from, Tournament tournament) : this()
        {
            caller = from;
            t = tournament;
            TypeInfo();
        }

        public TournamentTypeGump() : base(200, 200)
        {
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;		
        }

        public void TypeInfo()
        {
            AddPage(0);
            AddBackground(0, 44, 260, 180, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(85, 14, 0, @"Tournament Type");

            AddLabel(60, 60, 0, @"Single Elmination");
            AddRadio(25, 60, 208, 209, t.Type == TournamentType.SingleElimination ? true : false, 0);

            AddLabel(60, 90, 0, @"Double Elmination");
            AddRadio(25, 90, 208, 209, t.Type == TournamentType.DoubleElimination ? true : false, 1);

            AddLabel(60, 120, 0, @"Round Robin");
            AddRadio(25, 120, 208, 209, t.Type == TournamentType.RoundRobin ? true : false, 2);

            AddLabel(60, 150, 0, @"Hybrid");
            AddRadio(25, 150, 208, 209, t.Type == TournamentType.Hybrid ? true : false, 3);

            AddButton(218, 188, 4005, 4007, 1, GumpButtonType.Reply, 0);
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
                    switch(info.Switches[0])
                    {
                        case 1:
                        {
                            //t.Type = TournamentType.DoubleElimination;
                            from.SendMessage("This option is currently disabled.");
                            break;
                        }
                        case 2:
                        {
                            //t.Type = TournamentType.RoundRobin;
                            from.SendMessage("This option is currently disabled.");
                            break;
                        }
                        case 3:
                        {
                            //t.Type = TournamentType.Hybrid;
                            from.SendMessage("This option is currently disabled.");
                            break;
                        }
                        default:
                        {
                            t.Type = TournamentType.SingleElimination;
                            break;
                        }
                    }

                    from.SendGump(new TournamentInfoGump(from, t));
                    break;
                }
            }
        }
    }
}