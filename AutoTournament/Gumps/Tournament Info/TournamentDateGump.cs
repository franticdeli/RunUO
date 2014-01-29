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
    public class TournamentDateGump : Gump
    {
        Mobile caller;
        Tournament t;

        public TournamentDateGump(Mobile from, Tournament tournament) : this()
        {
            caller = from;
            t = tournament;
            DateInfo();
        }

        public TournamentDateGump() : base( 200, 200 )
        {
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;
        }

        public void DateInfo()
        {
            // Background
            AddPage(0);
            AddBackground(0, 44, 385, 391, 9250);
            AddBackground(0, 0, 385, 47, 9250);
            AddLabel(137, 15, 0, @"Tournament Date");

            // Year
            AddLabel(171, 60, 0, t.Date.Year.ToString());
            AddButton(222, 60, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddButton(122, 60, 4014, 4016, 2, GumpButtonType.Reply, 0);

            // Months
            AddLabel(25, 90, 0, @"Jan");
            AddButton(50, 90, t.Date.Month == 1 ? 211 : 210, 210, 4, GumpButtonType.Reply, 0);
            AddLabel(80, 90, 0, @"Feb");
            AddButton(105, 90, t.Date.Month == 2 ? 211 : 210, 210, 5, GumpButtonType.Reply, 0);
            AddLabel(135, 90, 0, @"Mar");
            AddButton(160, 90, t.Date.Month == 3 ? 211 : 210, 210, 6, GumpButtonType.Reply, 0);
            AddLabel(190, 90, 0, @"Apr");
            AddButton(215, 90, t.Date.Month == 4 ? 211 : 210, 210, 7, GumpButtonType.Reply, 0);
            AddLabel(245, 90, 0, @"May");
            AddButton(272, 90, t.Date.Month == 5 ? 211 : 210, 210, 8, GumpButtonType.Reply, 0);
            AddLabel(300, 90, 0, @"June");
            AddButton(334, 90, t.Date.Month == 6 ? 211 : 210, 210, 9, GumpButtonType.Reply, 0);
            AddLabel(22, 120, 0, @"July");
            AddButton(50, 120, t.Date.Month == 7 ? 211 : 210, 210, 10, GumpButtonType.Reply, 0);
            AddLabel(79, 120, 0, @"Aug");
            AddButton(105, 120, t.Date.Month == 8 ? 211 : 210, 210, 11, GumpButtonType.Reply, 0);
            AddLabel(129, 119, 0, @"Sept");
            AddButton(160, 120, t.Date.Month == 9 ? 211 : 210, 210, 12, GumpButtonType.Reply, 0);
            AddLabel(189, 120, 0, @"Oct");
            AddButton(215, 120, t.Date.Month == 10 ? 211 : 210, 210, 13, GumpButtonType.Reply, 0);
            AddLabel(244, 120, 0, @"Nov");
            AddButton(272, 120, t.Date.Month == 11 ? 211 : 210, 210, 14, GumpButtonType.Reply, 0);
            AddLabel(308, 120, 0, @"Dec");
            AddButton(334, 120, t.Date.Month == 12 ? 211 : 210, 210, 15, GumpButtonType.Reply, 0);

            // Days
            AddLabel(35, 160, 0, @"Sun");
            AddLabel(80, 160, 0, @"Mon");
            AddLabel(125, 160, 0, @"Tue");
            AddLabel(165, 160, 0, @"Wed");
            AddLabel(215, 160, 0, @"Th");
            AddLabel(260, 160, 0, @"Fri");
            AddLabel(310, 160, 0, @"Sat");

            int days = DateTime.DaysInMonth(t.Date.Year, t.Date.Month);
            DayOfWeek d = t.Date.DayOfWeek;
            int first = t.Date.Day;
            int week = 0;
            d = t.Date.DayOfWeek;
            while (first > 1)
            {
                switch (d)
                {
                    case DayOfWeek.Sunday:
                    {
                        d = DayOfWeek.Saturday;
                        break;
                    }
                    default:
                    {
                        d -= 1;
                        break;
                    }
                }
                first--;
            }

            for (int i = 1; i <= days; i++)
            {
                switch (d)
                {
                    case DayOfWeek.Sunday:
                    {
                        AddRadio(37, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Sunday Row
                        if (i > 19)
                            AddHtml(40, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(42, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(44, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Monday:
                    {
                        AddRadio(82, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Monday Row
                        if (i > 19)
                            AddHtml(85, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(87, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(89, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Tuesday:
                    {
                        AddRadio(127, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Tuesday Row
                        if (i > 19)
                            AddHtml(130, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(132, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(134, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Wednesday:
                    {
                        AddRadio(167, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Wednesday Row
                        if (i > 19)
                            AddHtml(170, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(172, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(174, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Thursday:
                    {
                        AddRadio(213, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Thursday Row
                        if (i > 19)
                            AddHtml(216, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(218, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(220, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Friday:
                    {
                        AddRadio(260, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Friday Row
                        if (i > 19)
                            AddHtml(263, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(265, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(267, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d += 1;
                        break;
                    }
                    case DayOfWeek.Saturday:
                    {
                        AddRadio(310, 185 + (week * 30), 210, 211, i == t.Date.Day, i - 1); // Saturday Row
                        if (i > 19)
                            AddHtml(313, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else if (i > 9 && i <= 19)
                            AddHtml(315, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        else
                            AddHtml(317, 184 + (week * 30), 25, 20, i.ToString(), (bool)false, (bool)false);
                        d = DayOfWeek.Sunday;
                        week += 1;
                        break;
                    }
                }
            }

            // Finalize
            AddButton(343, 399, 4023, 4025, 1, GumpButtonType.Reply, 0);
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
                    int day = info.Switches[0] + 1;
                    if (day == t.Date.Day)
                        from.SendGump(new TournamentInfoGump(from, t));
                    else
                    {
                        while (day != t.Date.Day)
                        {
                            if (day < t.Date.Day)
                                t.Date = t.Date.AddDays(-1);
                            else
                                t.Date = t.Date.AddDays(1);
                        }
                        from.SendGump(new TournamentInfoGump(from, t));
                    }
					break;
				}
				case 2:
				{
                    t.Date = t.Date.AddYears(-1);
                    from.SendGump(new TournamentDateGump(from, t));
					break;
				}
				case 3:
				{
                    t.Date = t.Date.AddYears(1);
                    from.SendGump(new TournamentDateGump(from, t));
					break;
				}
				default:
				{
                    int month = info.ButtonID - 3;
                    if (month == t.Date.Month)
                        from.SendGump(new TournamentDateGump(from, t));
                    else
                    {
                        while (month != t.Date.Month)
                        {
                            if (month < t.Date.Month)
                                t.Date = t.Date.AddMonths(-1);
                            else
                                t.Date = t.Date.AddMonths(1);
                        }
                        from.SendGump(new TournamentDateGump(from, t));
                    }
					break;
				}
            }
        }
    }
}