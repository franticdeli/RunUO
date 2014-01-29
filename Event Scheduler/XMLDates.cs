/***************************************************************\
* File Name : XMLDates.cs                                       *
* Developer : Taryen(Mark S.)                                   *
* Orig. Date: 02/17/2013                                        *
* Desc.     : Saves and Loads the information for each event    *
*               that is scheduled. Contains the various methods *
*               to add to, remove from, and sort the lists of   *
*               of the various events.                          *
\***************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  02/17/13  | Initial File                                      |
\****************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;
using Server.Mobiles;

using Tournaments;
using Tournaments.Items;
using Tournaments.Gumps;

namespace EventScheduler
{
    public class XMLDates
    {
        #region Initialization
        /// <summary>
        /// File location for EventDates.xml
        /// </summary>
        private static string filePath = Path.Combine(Core.BaseDirectory, "Data/Events/EventDates.xml");

        /// <summary>
        /// Collection of the event lists
        /// </summary>
        private static Dictionary<string, List<object>> m_Events = new Dictionary<string, List<object>>();

        /// <summary>
        /// Used for the Tournaments list to remove the spevific type of tournament from the list
        /// </summary>
        private static ArenaType m_Type;
        #endregion Initialization

        /// <summary>
        /// Public Collection of the lists so they can be used
        /// </summary>
        public static Dictionary<string, List<object>> Events
        {
            get { return m_Events; }
        }

        #region Tournament Methods
        /// <summary>
        /// Adds the tournament to the "tournament" list
        /// and sorts the list by date
        /// </summary>
        /// <param name="tournament">Tournament to be added</param>
        public static void AddTournament(Tournament tournament)
        {
            if (!m_Events.ContainsKey("tournament"))
            {
                List<object> list = new List<object>();
                list.Add(tournament);
                m_Events.Add("tournament", list);
            }
            else
            {
                m_Events["tournament"].Add(tournament);
            }

            if (m_Events.ContainsKey("tournament"))
            {
                List<Tournament> list = new List<Tournament>();
                foreach (object obj in m_Events["tournament"])
                {
                    Tournament t = (Tournament)obj;
                    list.Add(t);
                }

                list.Sort(delegate(Tournament t1, Tournament t2) { return t1.Date.CompareTo(t2.Date); });
                m_Events["tournament"].Clear();
                foreach (object obj in list)
                {
                    m_Events["tournament"].Add(obj);
                }
            }
            Save();
        }

        /// <summary>
        /// Removes the tournament from the "tournament" list
        /// and sorts the list by date
        /// </summary>
        /// <param name="tournament">Tournament to be removed</param>
        public static void RemoveTournament(Tournament tournament)
        {
            List<Mobile> hasGump = new List<Mobile>();

            if (m_Events.ContainsKey("tournament"))
            {
                if (m_Events["tournament"].Contains(tournament))
                    m_Events["tournament"].Remove(tournament);

                List<Tournament> list = new List<Tournament>();
                foreach (object obj in m_Events["tournament"])
                {
                    Tournament t = (Tournament)obj;
                    list.Add(t);
                }

                list.Sort(delegate(Tournament t1, Tournament t2) { return t1.Date.CompareTo(t2.Date); });
                m_Events["tournament"].Clear();
                foreach (object obj in list)
                {
                    m_Events["tournament"].Add(obj);
                }
            }

            foreach (UpcomingEventsGump gump in UpcomingEventsGump.OpenGumpList)
                hasGump.Add(gump.caller);
            foreach(Mobile m in hasGump)
            {
                if (m.HasGump(typeof(UpcomingEventsGump)))
                    m.CloseGump(typeof(UpcomingEventsGump));
                m.SendGump(new UpcomingEventsGump(m));
            }

            Save();
        }

        /// <summary>
        /// Cleans the "tournament" list by removing tournaments
        /// that do not have an arena. Sorts the list by date
        /// after the removal
        /// </summary>
        /// <param name="type"></param>
        public static void CleanTournamentList(ArenaType type)
        {
            m_Type = type;
            if(m_Events.ContainsKey("tournament"))
            {
                List<Tournament> list = new List<Tournament>();
                foreach (object obj in m_Events["tournament"])
                {
                    Tournament t = (Tournament)obj;
                    list.Add(t);
                }

                list.RemoveAll(RemoveType);
                m_Events["tournament"].Clear();
                foreach (object obj in list)
                {
                    m_Events["tournament"].Add(obj);
                }            
            }
        }

        /// <summary>
        /// Gets if the tournament is the type that is to be
        /// removed
        /// </summary>
        /// <param name="t">tournament to check</param>
        /// <returns></returns>
        private static bool RemoveType(Tournament t)
        {
            return t.TeamSize == m_Type;
        }
        #endregion

        /// <summary>
        /// Consctructor
        /// </summary>
        static XMLDates()
        {

            if (!Directory.Exists("Data/Events") ||!File.Exists(filePath))
                return;

            try
            {
                Load();
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: Exception caught loading tournament data");
                Console.WriteLine(e);
            }
        }

        #region Save/Load
        /// <summary>
        /// Saves the various events to EventDates.xml
        /// </summary>
        public static void Save()
        {
            if (!Directory.Exists("Data/Events"))
                Directory.CreateDirectory("Data/Events");

            using (StreamWriter op = new StreamWriter(filePath))
            {
                XmlTextWriter xml = new XmlTextWriter(op);
                xml.Formatting = Formatting.Indented;
                xml.IndentChar = '\t';
                xml.Indentation = 1;

                xml.WriteStartDocument(true);
                xml.WriteStartElement("eventdates");

                #region Save Tournaments
                if (m_Events.ContainsKey("tournament"))
                {
                    xml.WriteStartElement("tournaments");
                    foreach (Tournament tournament in m_Events["tournament"])
                    {
                        xml.WriteStartElement("tournament");
                        xml.WriteAttributeString("supplied", "true");
                        xml.WriteStartElement("date");
                        xml.WriteString(tournament.Date.ToString());
                        xml.WriteEndElement();
                        switch (tournament.Type)
                        {
                            case TournamentType.RoundRobin:
                            {
                                xml.WriteStartElement("type");
                                xml.WriteString("RoundRobin");
                                xml.WriteEndElement();
                                break;
                            }
                            case TournamentType.DoubleElimination:
                            {
                                xml.WriteStartElement("type");
                                xml.WriteString("DoubleElimination");
                                xml.WriteEndElement();
                                break;
                            }
                            case TournamentType.Hybrid:
                            {
                                xml.WriteStartElement("type");
                                xml.WriteString("Hybrid");
                                xml.WriteEndElement();
                                break;
                            }
                            default:
                            {
                                xml.WriteStartElement("type");
                                xml.WriteString("SingleElimination");
                                xml.WriteEndElement();
                                break;
                            }
                        }
                        switch (tournament.TeamSize)
                        {
                            case ArenaType.TwoVsTwo:
                            {
                                xml.WriteStartElement("teamsize");
                                xml.WriteString("TwoVsTwo");
                                xml.WriteEndElement();
                                break;
                            }
                            case ArenaType.ThreeVsThree:
                            {
                                xml.WriteStartElement("teamsize");
                                xml.WriteString("ThreeVsThree");
                                xml.WriteEndElement();
                                break;
                            }
                            case ArenaType.FourVsFour:
                            {
                                xml.WriteStartElement("teamsize");
                                xml.WriteString("FourVsFour");
                                xml.WriteEndElement();
                                break;
                            }
                            case ArenaType.FiveVsFive:
                            {
                                xml.WriteStartElement("teamsize");
                                xml.WriteString("FiveVsFive");
                                xml.WriteEndElement();
                                break;
                            }
                            default:
                            {
                                xml.WriteStartElement("teamsize");
                                xml.WriteString("OneVsOne");
                                xml.WriteEndElement();
                                break;
                            }
                        }
                        xml.WriteStartElement("teams");
                        string t = "";
                        Mobile m;
                        foreach (Teams team in tournament.Teams)
                        {
                            for (int i = 0; i < team.getOwners().Count; i++)
                            {
                                m = team.getOwners()[i];
                                if (m != null)
                                {
                                    if ((i + 1) == team.getOwners().Count)
                                        t += (((Int32)m.Serial).ToString() + '/');
                                    else
                                        t += (((Int32)m.Serial).ToString() + ',');
                                }
                            }

                        }
                        xml.WriteString(t);
                        xml.WriteEndElement();

                        xml.WriteStartElement("prizes");
                        Item item;
                        foreach (string place in tournament.Prizes.Keys)
                        {
                            item = tournament.Prizes[place];
                            if (item != null)
                            {
                                xml.WriteStartElement("item");
                                xml.WriteAttributeString("place", place);
                                xml.WriteAttributeString("type", item.GetType().ToString().Split('.')[item.GetType().ToString().Split('.').Length - 1]);
                                xml.WriteString((item.Hue).ToString() + ',' + (item.Amount).ToString());
                                xml.WriteEndElement();
                            }
                        }
                        xml.WriteEndElement();
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                }
                #endregion

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }

        /// <summary>
        /// Loads the various events to EventDates.xml
        /// </summary>
        private static void Load()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlElement root = doc["eventdates"];

            #region Load Tournaments
            foreach (XmlElement ts in root.GetElementsByTagName("tournaments"))
            {
                foreach (XmlElement element in ts.GetElementsByTagName("tournament"))
                {
                    Tournament tournament = new Tournament();
                    bool supplied;
                    if (!bool.TryParse(element.GetAttribute("supplied"), out supplied))
                        supplied = false;
                    tournament.Supplied = supplied;

                    foreach (XmlElement e in element.GetElementsByTagName("date"))
                    {
                        string typestring = e.InnerText;
                        string[] split = typestring.Split(' ');
                        if (split.Length == 3)
                        {
                            string date = split[0];
                            string time = split[1];
                            string pm = split[2];
                            tournament.Date = ValidDateTime(date, time, pm);
                        }
                    }

                    foreach (XmlElement e in element.GetElementsByTagName("type"))
                    {
                        string typestring = e.InnerText;
                        typestring = typestring.ToLower();

                        switch (typestring)
                        {
                            case "roundrobin":
                            {
                                tournament.Type = TournamentType.RoundRobin;
                                break;
                            }
                            case "singleelimination":
                            {
                                tournament.Type = TournamentType.SingleElimination;
                                break;
                            }
                            case "doubleeleimination":
                            {
                                tournament.Type = TournamentType.DoubleElimination;
                                break;
                            }
                            case "hybrid":
                            {
                                tournament.Type = TournamentType.Hybrid;
                                break;
                            }
                        }
                    }
                    foreach (XmlElement e in element.GetElementsByTagName("teamsize"))
                    {
                        string typestring = e.InnerText;
                        typestring = typestring.ToLower();

                        switch (typestring)
                        {
                            case "onevsone":
                            {
                                tournament.TeamSize = ArenaType.OneVsOne;
                                break;
                            }
                            case "twovstwo":
                            {
                                tournament.TeamSize = ArenaType.TwoVsTwo;
                                break;
                            }
                            case "threevsthree":
                            {
                                tournament.TeamSize = ArenaType.ThreeVsThree;
                                break;
                            }
                            case "fourvsfour":
                            {
                                tournament.TeamSize = ArenaType.FourVsFour;
                                break;
                            }
                            case "fivevsfive":
                            {
                                tournament.TeamSize = ArenaType.FiveVsFive;
                                break;
                            }
                        }
                    }

                    foreach (XmlElement e in element.GetElementsByTagName("teams"))
                    {
                        string typestring = e.InnerText;
                        Mobile m;
                        string[] list = typestring.Split('/');
                        foreach (string t in list)
                        {
                            if (t.Length > 0)
                            {
                                List<Mobile> team = new List<Mobile>();
                                string[] mobiles = t.Split(',');
                                foreach (string s in mobiles)
                                {
                                    try
                                    {
                                        Serial serial = Utility.ToInt32(s);
                                        World.Mobiles.TryGetValue(serial, out m);
                                        team.Add(m);
                                    }
                                    catch (Exception exception)
                                    {
                                        Console.WriteLine("Error in XMLDates: " + exception.Message);
                                    }
                                }
                                tournament.Teams.Add(new Teams(team));
                            }
                        }
                    }

                    foreach (XmlElement e in element.GetElementsByTagName("prizes"))
                    {
                        string typestring = e.InnerText;
                        string[] list = typestring.Split('/');
                        foreach (XmlElement i in e.GetElementsByTagName("item"))
                        {
                            Type type = null;
                            Item item;
                            string place = i.GetAttribute("place");
                            int hue, amount;
                            string itemProps = i.InnerText;
                            string[] props = itemProps.Split(',');

                            if (Region.ReadType(i, "type", ref type))
                            {
                                item = Activator.CreateInstance(type) as Item;
                                if (int.TryParse(props[0], out hue))
                                    item.Hue = hue;
                                if (int.TryParse(props[1], out amount))
                                    item.Amount = amount;
                                tournament.AddPrize(place, item);
                            }
                        }
                    }

                    if (tournament.Date > DateTime.Now && ArenaControl.Arenas.ContainsKey(tournament.TeamSize) &&
                        ArenaControl.Arenas[tournament.TeamSize] != null && ArenaControl.Arenas[tournament.TeamSize].Count > 0)
                        AddTournament(tournament);
                }
            }
            #endregion

            Save();
        }
        #endregion

        /// <summary>
        /// Creates a Valid DateTime object given the strings that define it
        /// </summary>
        /// <param name="date">defines the date</param>
        /// <param name="time">defines the time</param>
        /// <param name="pm">is it AM or PM?</param>
        /// <returns></returns>
        public static DateTime ValidDateTime(string date, string time, string pm)
        {
            string[] fulldate = date.Split('/');
            string[] fulltime = time.Split(':');
            int month, day, year, hour, minute, second;
            bool monthValid = false, dayValid = false, yearValid = false, hourValid = false, minuteValid = false, secondValid = false;
            bool isPM = false;
            pm = pm.ToLower();
            if (pm.Contains("pm"))
                isPM = true;

            if (fulldate.Length == 3 && fulltime.Length == 3)
            {
                if (int.TryParse(fulldate[0], out month) && int.TryParse(fulldate[1], out day) && int.TryParse(fulldate[2], out year) &&
                    int.TryParse(fulltime[0], out hour) && int.TryParse(fulltime[1], out minute) && int.TryParse(fulltime[2], out second))
                {
                    if (year > 0 && year < 10000)
                        yearValid = true;
                    if (month > 0 && month < 13)
                        monthValid = true;
                    if (day >= 0 && yearValid && monthValid && day <= DateTime.DaysInMonth(year, month))
                        dayValid = true;

                    if (isPM)
                        hour += 12;
                    if (hour >= 0 && hour < 24)
                        hourValid = true;
                    if (minute >= 0 && minute < 60)
                        minuteValid = true;
                    if (second >= 0 && second < 60)
                        secondValid = true;

                    if (monthValid && dayValid && yearValid && hourValid && minuteValid && secondValid)
                        return new DateTime(year, month, day, hour, minute, second);
                }
            }

            return DateTime.Now;
        }
    }
}
