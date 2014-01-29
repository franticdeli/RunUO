/***************************************************************\
* File Name : AutoTimer.cs                                      *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 5/18/2010                                         *
* Desc.     : A timing script that allows for an event to be    *   
*              started on a set date at a designated time.      *
\***************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
|  02/17/13  | Revamp to enable additional events                |
\****************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

using Server;
using Server.Commands;
using Server.Items;
using Server.Misc;

using Tournaments;
using Tournaments.Items;

namespace EventScheduler
{
    public class AutoTimer : Timer
    {
        private Tournament t;

        #region Command Registration
        // Auto Event Enabler - True = Enabled, False = Disabled
        public static bool m_EventsEnabled = true;

        /// <summary>
        /// Registers the Command [EnableEvents which allows 
        /// on the fly enabling and disabling of the Event Scheduler.
        /// </summary>
        public static void Initialize()
        {
            new AutoTimer().Start();
            CommandSystem.Register("EnableEvents", AccessLevel.Administrator, new CommandEventHandler(SetEvents_OnCommand));
        }

        /// <summary>
        /// Allows the on the fly enabling of the system.
        /// </summary>
        public static bool EventsEnabled
        {
            get { return m_EventsEnabled; }
            set { m_EventsEnabled = value; }
        }

        /// <summary>
        /// Allows administration to set if this is active
        /// </summary>
        /// <param name="e"></param>
        [Usage("SetAutoTourney <true | false>")]
        [Description("Enables or disables automatic tournaments.")]
        public static void SetEvents_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                m_EventsEnabled = e.GetBoolean(0);
                e.Mobile.SendMessage("Automatic Events have been {0}.", m_EventsEnabled ? "enabled" : "disabled");
            }
            else
            {
                e.Mobile.SendMessage("Format: EnableEvents <true | false>");
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public AutoTimer() : base(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0))
        {
            Priority = TimerPriority.FiveSeconds;
        }

        /// <summary>
        /// Provides notifications of approaching events and starts them
        /// </summary>
        protected override void OnTick()
        {
            if (!m_EventsEnabled || AutoRestart.Restarting)
                Stop();

            // Checks the next scheduled tournament and gives out notifications for the tournament
            if (XMLDates.Events.ContainsKey("tournament"))
            {
                if (XMLDates.Events["tournament"].Count > 0 && ArenaControl.Arenas.Count > 0)
                {
                    t = (Tournament)XMLDates.Events["tournament"][0];
                    TimeSpan date = t.Date - DateTime.Now;
                    // Give 15 minute warning for the tournament
                    if (date.Days == 0 && date.Hours == 0 && date.Minutes == 15 && date.Seconds < 10)
                    {
                        World.Broadcast(0, false, String.Format("The {0} tournament will commence in approximately 15 minutes.", t.TeamSize));
                        World.Broadcast(0, false, "If you have not registered, please do so at this time with [tournaments");
                    }
                    // Give 5 minute warning for the tournament
                    else if (date.Days == 0 && date.Hours == 0 && date.Minutes == 5 && date.Seconds < 10)
                    {
                        World.Broadcast(0, false, String.Format("The {0} tournament will commence in approximately 5 minutes.", t.TeamSize));
                        World.Broadcast(0, false, "If you have registered, please make your way to a safe location.");
                    }
                    // Give 2 minute warning for the tournament
                    else if (date.Days == 0 && date.Hours == 0 && date.Minutes == 2 && date.Seconds < 10)
                    {
                        World.Broadcast(0, false, String.Format("Two minute warning for the {0} tournament.", t.TeamSize));
                    }
                    // Begin the tournament
                    else if (date.Days == 0 && date.Hours == 0 && date.Minutes == 0 && date.Seconds < 10)
                    {
                        World.Broadcast(0, false, String.Format("The {0} tournament will begin shortly.", t.TeamSize));
                        TournamentTimer timer = new TournamentTimer(t);
                        timer.Prepare();
                        XMLDates.RemoveTournament(t);
                    }
                }
            }
        }
    }
}