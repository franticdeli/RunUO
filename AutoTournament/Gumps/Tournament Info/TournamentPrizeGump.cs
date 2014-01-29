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
    public class TournamentPrizeGump : Gump
    {
        Mobile caller;
        Tournament t;

        public TournamentPrizeGump(Mobile from, Tournament tournament)
            : this()
        {
            caller = from;
            t = tournament;
            PrizeInfo();
        }

        public TournamentPrizeGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
        }

        public void PrizeInfo()
        {
            AddPage(0);
            Item first,second,third,consolation;

            if (caller.AccessLevel >= AccessLevel.GameMaster)
            {
                AddBackground(0, 44, 380, 210, 9250);
            }
            else
                AddBackground(0, 44, 380, 180, 9250);

            AddBackground(0, 0, 380, 47, 9250);
            AddLabel(125, 14, 0, @"Tournament Prizes");

            AddLabel(25, 60, 0, @"First:");
            AddLabel(100, 60, 0, t.Prizes.TryGetValue("first", out first) ? 
                first.GetType().ToString().Split('.')[first.GetType().ToString().Split('.').Length - 1]
                + " Hue:0x" + first.Hue.ToString("X").PadLeft(3,'0') + " Amount:" + first.Amount : "");

            AddLabel(25, 90, 0, @"Second:");
            AddLabel(100, 90, 0, t.Prizes.TryGetValue("second", out second) ? 
                second.GetType().ToString().Split('.')[second.GetType().ToString().Split('.').Length - 1]
                + " Hue:0x" + second.Hue.ToString("X").PadLeft(3, '0') + " Amount:" + second.Amount : "");

            AddLabel(25, 120, 0, @"Third:");
            AddLabel(100, 120, 0, t.Prizes.TryGetValue("third", out third) ? 
                third.GetType().ToString().Split('.')[third.GetType().ToString().Split('.').Length - 1]
                + " Hue:0x" + third.Hue.ToString("X").PadLeft(3, '0') + " Amount:" + third.Amount : "");

            AddLabel(25, 150, 0, @"Consolation:");
            AddLabel(100, 150, 0, t.Prizes.TryGetValue("consolation", out consolation) ? 
                consolation.GetType().ToString().Split('.')[consolation.GetType().ToString().Split('.').Length - 1]
                + " Hue:0x" + consolation.Hue.ToString("X").PadLeft(3, '0') + " Amount:" + consolation.Amount : "");

            if (caller.AccessLevel >= AccessLevel.GameMaster)
            {
                AddButton(338, 60, 4005, 4007, 1, GumpButtonType.Reply, 0); // First Add Button
                AddButton(308, 60, 4017, 4019, 6, GumpButtonType.Reply, 0); // First Remove Button

                AddButton(338, 90, 4005, 4007, 2, GumpButtonType.Reply, 0); // Second Add Button
                AddButton(308, 90, 4017, 4019, 7, GumpButtonType.Reply, 0); // Second Remove Button

                AddButton(338, 120, 4005, 4007, 3, GumpButtonType.Reply, 0); // Third Add Button
                AddButton(308, 120, 4017, 4019, 8, GumpButtonType.Reply, 0); // Third Remove Button

                AddButton(338, 150, 4005, 4007, 4, GumpButtonType.Reply, 0); // Consolation Add Button
                AddButton(308, 150, 4017, 4019, 9, GumpButtonType.Reply, 0); // Consolation Remove Button

                AddButton(337, 218, 4023, 4025, 5, GumpButtonType.Reply, 0); // OK Button
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            Item previous;

            switch (info.ButtonID)
            {
                case 0:
                {
                    break;
                }
                case 1:
                {
                    from.Target = new InternalTarget(t, "first");
                    break;
                }
                case 2:
                {
                    if (t.Prizes.TryGetValue("first", out previous))
                        from.Target = new InternalTarget(t, "second");
                    else
                    {
                        from.SendMessage("You must set the first place prize");
                        from.SendGump(new TournamentPrizeGump(from, t));
                    }
                    break;
                }
                case 3:
                {
                    if (t.Prizes.TryGetValue("second", out previous))
                        from.Target = new InternalTarget(t, "third");
                    else
                    {
                        from.SendMessage("You must set the second place prize");
                        from.SendGump(new TournamentPrizeGump(from, t));
                    }
                    break;
                }
                case 4:
                {
                    if (t.Prizes.TryGetValue("first", out previous))
                        from.Target = new InternalTarget(t, "consolation");
                    else
                    {
                        from.SendMessage("You must set the first place prize");
                        from.SendGump(new TournamentPrizeGump(from, t));
                    }
                    break;
                }
                case 5:
                {
                    from.SendGump(new TournamentInfoGump(from,t));
                    break;
                }
                case 6:
                {
                    t.RemovePrize("first");
                    from.SendGump(new TournamentPrizeGump(from, t));
                    break;
                }
                case 7:
                {
                    t.RemovePrize("second");
                    from.SendGump(new TournamentPrizeGump(from, t));
                    break;
                }
                case 8:
                {
                    t.RemovePrize("third");
                    from.SendGump(new TournamentPrizeGump(from, t));
                    break;
                }
                case 9:
                {
                    t.RemovePrize("consolation");
                    from.SendGump(new TournamentPrizeGump(from, t));
                    break;
                }
            }
        }

        private class InternalTarget : Target
        {
            private Tournament t;
            private string p;

            public InternalTarget(Tournament tournament, string place) : base(10, true, TargetFlags.None)
            {
                t = tournament;
                p = place;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    Item award = (Item)targeted;
                    t.AddPrize(p, award);
                    from.SendGump(new TournamentPrizeGump(from, t));
                }
                else
                    from.SendGump(new TournamentPrizeGump(from, t));
            }
        }
    }
}