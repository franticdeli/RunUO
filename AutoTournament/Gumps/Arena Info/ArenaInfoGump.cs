/***************************************************************\
* File Name : ArenaInfoGump.cs                                  *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 02/17/2013                                        *
* Desc.     : A gump that displays the arena settings           *
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
using Server.Targeting;

using Tournaments;
using Tournaments.Items;

namespace Tournaments.Gumps
{
    public class ArenaInfoGump : Gump
    {
        /// <summary>
        /// Mobile that called the gump
        /// </summary>
        Mobile caller;

        /// <summary>
        /// Arena to edit
        /// </summary>
        ArenaControl a;

        public ArenaInfoGump(Mobile from, ArenaControl arena)
            : this()
        {
            caller = from;
            a = arena;
            ArenaInfo();
        }

        public ArenaInfoGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
        }

        public void ArenaInfo()
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

            if (!a.AnnouncerSpot.Equals(new Point3D(0, 0, 0)) && !a.HoldingArea.Equals(new Point3D(0, 0, 0)) &&
                        !a.ExitArea.Equals(new Point3D(0, 0, 0)) && a.StagingAreaOne.Count == locs && a.StagingAreaTwo.Count == locs &&
                        a.FightingAreaOne.Count == locs && a.FightingAreaTwo.Count == locs)
            {
                a.Active = true;
            }
            else
            {
                a.Active = false;
            }

            AddPage(0);
            AddBackground(0, 44, 340, 440, 9250);

            AddBackground(0, 0, 340, 47, 9250);
            AddLabel(145, 14, 0, @"Arena Settings");

            AddLabel(25, 60, 0, @"Active:");
            AddLabel(180, 60, 0, a.Active.ToString().ToUpper());

            AddLabel(25, 90, 0, @"Supplied:");
            AddLabel(180, 90, 0, a.Supplied.ToString().ToUpper());
            AddButton(298, 90, 4005, 4007, 2, GumpButtonType.Reply, 0);

            AddLabel(25, 120, 0, @"Set:");
            AddLabel(180, 120, 0, a.Set);
            AddButton(298, 120, 4005, 4007, 3, GumpButtonType.Reply, 0);

            AddLabel(25, 150, 0, @"Type:");
            AddLabel(180, 150, 0, a.Type.ToString());
            AddButton(298, 150, 4005, 4007, 4, GumpButtonType.Reply, 0);

            AddLabel(25, 180, 0, @"Announcer Location:");
            AddLabel(180, 180, 0, a.AnnouncerSpot.ToString());
            AddButton(298, 180, 4005, 4007, 5, GumpButtonType.Reply, 0);

            AddLabel(25, 210, 0, @"Holding Location:");
            AddLabel(180, 210, 0, a.HoldingArea.ToString());
            AddButton(298, 210, 4005, 4007, 6, GumpButtonType.Reply, 0);

            AddLabel(25, 240, 0, @"Exit Location:");
            AddLabel(180, 240, 0, a.ExitArea.ToString());
            AddButton(298, 240, 4005, 4007, 7, GumpButtonType.Reply, 0);

            AddLabel(25, 270, 0, @"Red Staging Location(s):");
            AddLabel(180, 270, 0, a.StagingAreaOne.Count == locs ? "Set":"Not Set");
            AddButton(298, 270, 4005, 4007, 8, GumpButtonType.Reply, 0);

            AddLabel(25, 300, 0, @"Blue Staging Location(s):");
            AddLabel(180, 300, 0, a.StagingAreaTwo.Count == locs ? "Set" : "Not Set");
            AddButton(298, 300, 4005, 4007, 9, GumpButtonType.Reply, 0);

            AddLabel(25, 330, 0, @"Red Fighting Location(s):");
            AddLabel(180, 330, 0, a.FightingAreaOne.Count == locs ? "Set" : "Not Set");
            AddButton(298, 330, 4005, 4007, 10, GumpButtonType.Reply, 0);

            AddLabel(25, 360, 0, @"Blue Fighting Location(s):");
            AddLabel(180, 360, 0, a.FightingAreaTwo.Count == locs ? "Set" : "Not Set");
            AddButton(298, 360, 4005, 4007, 11, GumpButtonType.Reply, 0);

            AddLabel(25, 390, 0, @"Arena Area(s):");
            AddLabel(180, 390, 0, (a.ArenaArea != null && a.ArenaArea.Length != 0) ? "Set" : "Not Set");
            AddButton(298, 390, 4005, 4007, 12, GumpButtonType.Reply, 0);

            AddLabel(25, 420, 0, @"Spectator Area(s):");
            AddLabel(180, 420, 0, a.SpectatorArea != null ? "Set" : "Not Set");
            AddButton(298, 420, 4005, 4007, 13, GumpButtonType.Reply, 0);

            AddButton(297, 448, 4023, 4025, 1, GumpButtonType.Reply, 0); // OK Button
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
                    break;
                }
                case 2:
                {
                    a.Supplied = !a.Supplied;
                    from.SendGump(new ArenaInfoGump(from, a));
                    break;
                }
                case 3:
                {
                    from.SendGump(new ArenaSetGump(from, a, 0));
                    break;
                }
                case 4:
                {
                    a.StagingAreaOne.Clear();
                    a.StagingAreaTwo.Clear();
                    a.FightingAreaOne.Clear();
                    a.FightingAreaTwo.Clear();
                    from.SendGump(new ArenaTypeGump(from, a));
                    break;
                }
                case 5:
                {
                    from.Target = new SingleTarget(a, 0);
                    break;
                }
                case 6:
                {
                    from.Target = new SingleTarget(a, 1);
                    break;
                }
                case 7:
                {
                    from.Target = new SingleTarget(a, 2);
                    break;
                }
                case 8:
                {
                    from.SendGump(new ArenaPlacementGump(from, a, a.StagingAreaOne));
                    break;
                }
                case 9:
                {
                    from.SendGump(new ArenaPlacementGump(from, a, a.StagingAreaTwo));
                    break;
                }
                case 10:
                {
                    from.SendGump(new ArenaPlacementGump(from, a, a.FightingAreaOne));
                    break;
                }
                case 11:
                {
                    from.SendGump(new ArenaPlacementGump(from,a,a.FightingAreaTwo));
                    break;
                }
                case 12:
                {
                    a.ChooseArenaArea(from);
                    break;
                }
                case 13:
                {
                    a.ChooseSpectatorArea(from);
                    break;
                }
            }
        }

        private class SingleTarget : Target
        {
            private ArenaControl a;
            private int loc;

            public SingleTarget(ArenaControl arena, int location)
                : base(10, true, TargetFlags.None)
            {
                a = arena;
                loc = location;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;

                if (loc == 0)
                    a.AnnouncerSpot = new Point3D(p);
                else if (loc == 1)
                    a.HoldingArea = new Point3D(p);
                else if (loc == 2)
                    a.ExitArea = new Point3D(p);
                else if (loc == 3)
                    a.StagingAreaOne.Add(new Point3D(p));
                else if (loc == 4)
                    a.StagingAreaTwo.Add(new Point3D(p));
                else if (loc == 5)
                    a.FightingAreaOne.Add(new Point3D(p));
                else if (loc == 6)
                    a.FightingAreaTwo.Add(new Point3D(p));

                from.SendGump(new ArenaInfoGump(from, a));
            }
        }
    }
}