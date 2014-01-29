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

using Tournaments;
using Tournaments.Gumps;
using Tournaments.Items;

namespace EventScheduler
{
    public class UpcomingEventsGump : Gump
    {
        /// <summary>
        /// List of all of the Event gumps opened
        /// Used for a global refresh upon event list update
        /// </summary>
        public static List<UpcomingEventsGump> OpenGumpList = new List<UpcomingEventsGump>();

        /// <summary>
        /// Mobile the gump is attached to
        /// </summary>
        public Mobile caller;

        /// <summary>
        /// The current page of the gump that is opened
        /// </summary>
        public int CurrentPage;

        /// <summary>
        /// A list of the tournaments
        /// </summary>
        private List<object> tournaments;

        #region Command Registration
        /// <summary>
        /// Registers the [Events command that brings the gump up
        /// </summary>
        public static void Initialize()
        {
            CommandSystem.Register("Events", AccessLevel.Player, new CommandEventHandler(Events_OnCommand));
        }

        /// <summary>
        /// Brings up the gump if the event scheduler is active or the access level is as high as a GameMaster
        /// </summary>
        /// <param name="e"></param>
        [Usage("Tournaments")]
        [Description("Allows players to view details of, register for,and unregister for a tournament.")]
        public static void Events_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            if (AutoTimer.EventsEnabled || m.AccessLevel >= AccessLevel.GameMaster)
            {
                if (m.HasGump(typeof(UpcomingEventsGump)))
                    m.CloseGump(typeof(UpcomingEventsGump));
                m.SendGump(new UpcomingEventsGump(m));
            }
            else
                m.SendMessage("Currently Disabled");
        }
        #endregion

        /// <summary>
        /// Main Contructor
        /// Loads upon Server Startup
        /// </summary>
        public UpcomingEventsGump()
            : base(200, 200)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            tournaments = new List<object>();
        }

        /// <summary>
        /// Initial Contructor
        /// Always shows the first page
        /// </summary>
        /// <param name="from">mobile calling the gump</param>
        public UpcomingEventsGump(Mobile from) : this()
        {
            caller = from;
            if (XMLDates.Events.ContainsKey("tournament"))
            {
                tournaments = XMLDates.Events["tournament"];
            }
            PageInfo(0);
            if (!OpenGumpList.Contains(this))
                OpenGumpList.Add(this);
        }

        /// <summary>
        /// Constructor
        /// Shows the gump at the selected page
        /// </summary>
        /// <param name="from">mobile calling the gump</param>
        /// <param name="page">page to be displayed</param>
        public UpcomingEventsGump(Mobile from, int page) : this()
        {
            caller = from;
            if (XMLDates.Events.ContainsKey("tournament"))
            {
                tournaments = XMLDates.Events["tournament"];
            }
            PageInfo(page);
            if (!OpenGumpList.Contains(this))
                OpenGumpList.Add(this);
        }

        /// <summary>
        /// Contains the gump design and contents
        /// </summary>
        /// <param name="i">page to be displayed</param>
        public void PageInfo(int i)
        {
            CurrentPage = i;
            int index = tournaments.Count / 5;
            int pages = (tournaments.Count % 5 == 0 ? index : (index + 1));

            AddPage(i);
            // Background and Title
            AddBackground(0, 44, 280, 210, 9250);
            AddBackground(0, 0, 280, 47, 9250);
            AddLabel(85, 14, 0, @"Upcoming Events");

            if (tournaments.Count < ((i + 1) * 5))
                index = tournaments.Count - i * 5;
            else
                index = 5;

            // Tournaments
            for (int j = 0; j < index; j++)
            {
                Tournament t = (Tournament)tournaments[i * 5 + j];

                AddLabel(18, 60 + j * 30, 0, String.Format("{0}-{1}/{2}/{3}", t.TeamSize,t.Date.Month, t.Date.Day, t.Date.Year));
                AddButton(178, 60 + j * 30, 4011, 4013, 3 + j * 3, GumpButtonType.Reply, 0);
                AddButton(208, 60 + j * 30, t.IsRegistered((PlayerMobile)caller) ? 4002 : 4003, 4004, 4 + j * 3, GumpButtonType.Reply, 0);
                AddButton(238, 60 + j * 30, t.IsRegistered((PlayerMobile)caller) ? 4009 : 4008, 4010, 5 + j * 3, GumpButtonType.Reply, 0);
            }

            if (i + 1 != pages && tournaments.Count > 0)
                AddButton(237, 218, 4005, 4007, 1, GumpButtonType.Reply, 0);

            if (i != 0)
                AddButton(208, 218, 4014, 4016, 2, GumpButtonType.Reply, 0);
        }      

        /// <summary>
        /// Defines the actions to be taken upon gump response
        /// </summary>
        /// <param name="sender">player that responded</param>
        /// <param name="info">gump information</param>
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
                    this.PageInfo(CurrentPage+1);
                    from.SendGump(this);
                    break;
                }
                case 2:
                {
                    this.PageInfo(CurrentPage-1);
                    from.SendGump(this);
                    break;
                }
                default:
                {
                    Tournament t;
                    if ((info.ButtonID % 3) == 0)
                    {
                        t = (Tournament)tournaments[(((info.ButtonID / 3) - 1) + ((CurrentPage) * 5))];
                        from.SendGump(new UpcomingEventsGump(from));
                        from.SendGump( new TournamentInfoGump(from,t));
                    }
                    else if (((info.ButtonID - 1) % 3) == 0)
                    {
                        t = (Tournament)tournaments[((((info.ButtonID - 1) / 3) - 1) + ((CurrentPage) * 5))];
                        if (t.IsRegistered((PlayerMobile)caller))
                            t.RemoveTeam(new Teams((PlayerMobile)caller));
                        from.SendGump(new UpcomingEventsGump(from));
                    }
                    else if (((info.ButtonID - 2) % 3) == 0)
                    {
                        t = (Tournament)tournaments[((((info.ButtonID - 2) / 3) - 1) + ((CurrentPage) * 5))];
                        if (!t.IsRegistered((PlayerMobile)caller) )
                        {
                            if (t.TeamSize != ArenaType.OneVsOne)
                                from.SendGump(new CreateTeamGump(from, t));
                            else
                            {
                                t.AddTeam(new Teams((PlayerMobile)from));
                                from.SendGump(new UpcomingEventsGump(from));
                            }
                        }
                        else
                            from.SendGump(new UpcomingEventsGump(from));

                    }
                    break;
                }
            }

            if (OpenGumpList.Contains(this))
                OpenGumpList.Remove(this);
        }
    }
}