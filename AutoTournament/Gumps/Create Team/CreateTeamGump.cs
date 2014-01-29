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

using EventScheduler;

using Tournaments;
using Tournaments.Items;

namespace Tournaments.Gumps
{
    public class CreateTeamGump : Gump
    {
        Mobile caller;
        Tournament t;
        List<Mobile> team;
        int size;

        public CreateTeamGump(Mobile from, Tournament tournament)
            : this()
        {
            caller = from;
            t = tournament;

            switch (t.TeamSize)
            {
                case ArenaType.TwoVsTwo:
                {
                    size = 1;
                    break;
                }
                case ArenaType.ThreeVsThree:
                {
                    size = 2;
                    break;
                }
                case ArenaType.FourVsFour:
                {
                    size = 3;
                    break;
                }
                case ArenaType.FiveVsFive:
                {
                    size = 4;
                    break;
                }
                default:
                {
                    size = 0;
                    break;
                }
            }
            
            team = new List<Mobile>();
            team.Add(from);
            TeamInfo();
        }

        public CreateTeamGump(Mobile from, Tournament tournament, List<Mobile> updatedteam)
            : this()
        {
            caller = from;
            t = tournament;

            switch (t.TeamSize)
            {
                case ArenaType.TwoVsTwo:
                {
                    size = 1;
                    break;
                }
                case ArenaType.ThreeVsThree:
                {
                    size = 2;
                    break;
                }
                case ArenaType.FourVsFour:
                {
                    size = 3;
                    break;
                }
                case ArenaType.FiveVsFive:
                {
                    size = 4;
                    break;
                }
                default:
                {
                    size = 0;
                    break;
                }
            }

            team = updatedteam;
            TeamInfo();
        }

        public CreateTeamGump()
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
            AddBackground(0, 44, 260, 80+size*30, 9250);
            AddBackground(0, 0, 260, 47, 9250);
            AddLabel(70, 14, 0, @"Team Configuration");

            AddLabel(25, 60, 0, @"Captain:");
            AddLabel(100, 60, 0, team[0].Name);

            for (int i = 0; i < size; i++)
            {
                AddLabel(25, 90 + i * 30, 0, @"Team Member:");
                AddLabel(120, 90 + i * 30, 0, team.Count > (i+1) ? team[i+1].Name : "");
                AddButton(218, 90 + i * 30, 4005, 4007, 2 + i * 2, GumpButtonType.Reply, 0); // Team Member Add Button
                AddButton(188, 90 + i * 30, 4017, 4019, 3 + i * 2, GumpButtonType.Reply, 0); // Team Member Remove Button
            }

            AddButton(217, 88 + size * 30, 4023, 4025, 1, GumpButtonType.Reply, 0); // OK Button
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
                    if (((size + 1) == team.Count) && (size == 0))
                    {
                        Teams fullteam = new Teams(team);
                        t.AddTeam(fullteam);
                        from.SendGump(new UpcomingEventsGump(from));
                    }
                    else if ((size + 1) == team.Count && size > 0)
                    {
                        Teams fullteam = new Teams(team);
                        for (int i = 1; i < team.Count; i++ )
                            team[i].SendGump(new TeamConfirmGump(team[i], t, fullteam));
                    }
                    else
                        from.SendMessage("There are not enough members on the team.");
                    break;
                }
                default:
                {
                    if (info.ButtonID % 2 == 0)
                    {
                        int position = info.ButtonID / 2;
                        from.Target = new InternalTarget(t, team, position);
                    }
                    else
                    {
                        int position = (info.ButtonID-1) / 2;
                        if ((position) < team.Count)
                            team.Remove(team[position]);
                        from.SendGump(new UpcomingEventsGump(from));
                    }
                    break;
                }
            }
        }

        private class InternalTarget : Target
        {
            private Tournament t;
            private List<Mobile> team;
            private int pos;

            public InternalTarget(Tournament tournament, List<Mobile> updateteam, int position)
                : base(10, true, TargetFlags.None)
            {
                t = tournament;
                team = updateteam;
                pos = position;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is PlayerMobile && !team.Contains((PlayerMobile)targeted))
                {
                    try
                    {
                        team[pos] = (PlayerMobile)targeted;
                    }
                    catch (Exception e)
                    {
                        team.Add((PlayerMobile)targeted);
                    }
                }
                else if (targeted is PlayerMobile)
                    from.SendMessage("That player is already listed as part of the team.");
                
                from.SendGump(new CreateTeamGump(from, t, team));
            }
        }
    }
}