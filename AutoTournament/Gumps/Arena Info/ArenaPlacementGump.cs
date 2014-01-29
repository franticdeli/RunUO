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
    public class ArenaPlacementGump : Gump
    {
        Mobile caller;
        ArenaControl a;
        List<Point3D> points;

        public ArenaPlacementGump(Mobile from, ArenaControl arena, List<Point3D> list)
            : this()
        {
            caller = from;
            a = arena;
            points = list;
            PlacementInfo();
        }

        public ArenaPlacementGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
        }

        public void PlacementInfo()
        {
            int locs;
            if (a.Type == ArenaType.FiveVsFive)
                locs = 5;
            else if (a.Type == ArenaType.FourVsFour)
                locs = 4;
            else if (a.Type == ArenaType.ThreeVsThree)
                locs = 3;
            else if (a.Type == ArenaType.TwoVsTwo)
                locs = 2;
            else
                locs = 1;

            AddPage(0);
            AddBackground(0, 44, 380, 50+locs*30, 9250);
            AddBackground(0, 0, 380, 47, 9250);
            AddLabel(125, 14, 0, @"Arena Placement");

            for (int i = 0; i < locs; i++)
            {
                AddLabel(25, 60+i*30, 0, String.Format("Location {0}:", i + 1));
                AddLabel(100, 60 + i * 30, 0, points.Count > i ? points[i].ToString() : "");
                AddButton(338, 60 + i * 30, 4005, 4007, 2 + i * 2, GumpButtonType.Reply, 0); // Add Button
                AddButton(308, 60 + i * 30, 4017, 4019, 3 + i * 2, GumpButtonType.Reply, 0); // Remove Button
            }

            AddButton(337, 58 + locs * 30, 4023, 4025, 1, GumpButtonType.Reply, 0); // OK Button
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
                    from.SendGump(new ArenaInfoGump(from,a));
                    break;
                }
                default:
                {
                    if (info.ButtonID % 2 == 0)
                        from.Target = new SingleTarget(a, points, ((info.ButtonID / 2)-1));
                    else if (points.Count >= ((info.ButtonID - 1) / 2))
                    {
                        points.RemoveAt(((info.ButtonID - 1) / 2) - 1);
                        from.SendGump(new ArenaPlacementGump(from, a, points));
                    }
                    break;
                }
            }
        }

        private class SingleTarget : Target
        {
            private ArenaControl a;
            List<Point3D> l;
            private int loc;

            public SingleTarget(ArenaControl arena, List<Point3D> list, int location)
                : base(10, true, TargetFlags.None)
            {
                a = arena;
                loc = location;
                l = list;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;

                if (l.Count > loc)
                    l[loc] = new Point3D(p);
                else
                    l.Add(new Point3D(p));

                from.SendGump(new ArenaPlacementGump(from, a, l));
            }
        }
    }
}