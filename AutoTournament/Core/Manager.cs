/****************************************************************\
* File Name : Manager.cs                                         *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 5/18/2010                                          *
* Desc.     : Contains many methods that setup, finish, and      *
*             record the events of the tournament.               *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
\****************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Accounting;
using Server.Misc;
using Server.Regions;
using Server.Commands;

using Tournaments.Items;

namespace Tournaments
{
	public class Manager
	{
        public static void FilterInactiveTeams(Tournament tourney)
        {
            List<Teams> inactive = new List<Teams>();
            foreach (Teams t in tourney.Teams)
            {
                foreach(Mobile m in t.getOwners())
                {
                    if ((!IsOnline((PlayerMobile)m) || (!(m.Region is GuardedRegion) && !(m.Region is HouseRegion)) || m.Criminal) && !inactive.Contains(t))
                        inactive.Add(t);
                }
            }

            foreach (Teams t in inactive)
                tourney.Teams.Remove(t);
        }

		/// <summary>
		/// Moves all the players items to thier bank.
		/// Notifies the player that thier belongings have been placed in thier bank.
		/// </summary>
		/// <param name="m"> The mobile to be have items moved</param>
		public static void ItemsToBank( PlayerMobile m )
		{
            try
            {
                if (m.Backpack != null && m.BankBox != null)
                {
                    Container bp = m.Backpack;
                    BankBox bank = m.BankBox;
                    Container pack = new Backpack();
                    List<Item> list = new List<Item>();
                    Item addItem;

                    foreach (Item item in m.Items)
                    {
                        if (item != bp && item != bank)
                            list.Add(item);
                    }
                    foreach (Item item in bp.Items)
                    {
                        list.Add(item);
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        addItem = list[i];
                        pack.AddItem(addItem);
                    }

                    if (pack.Items.Count > 0)
                        bank.AddItem(pack);
                }

                m.SendMessage("All of your items have been sent to your bankbox via a backpack.");

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : ItemsToBank() in Manager.cs");
            }
		}
		
		/// <summary>
		/// Checks to see if the player is online or not.
		/// </summary>
		/// <param name="m"> The person to be checked</param>
		/// <returns></returns>
		public static bool IsOnline( PlayerMobile m )
		{
            try
            {
                NetState ns = m.NetState;

                if (ns == null)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : IsOnline() in Manager.cs");

                return false;
            }
		}
		
		/// <summary>
		/// Fully refreshes the targetted player.
		/// Prevents any type of pre-casting or other advantages.
		/// </summary>
		/// <param name="targ"> The target to be refreshed</param>
		public static void RefreshPlayer( PlayerMobile targ )
		{
            try
            {
                if (!targ.Alive)
                {
                    Mobile m = (Mobile)targ;
                    m.Resurrect();
                }

                targ.Mana = targ.ManaMax;
                targ.Hits = targ.HitsMax;
                targ.Stam = targ.StamMax;
                targ.Poison = null;

                targ.Say("*Refreshed!*");
                targ.Say("*Debuffed!*");
                if (targ.Target != null)
                    targ.Say("I have pre-casted...");

                Server.Targeting.Target.Cancel(targ);

                if (targ.MeleeDamageAbsorb > 0)
                {
                    targ.MeleeDamageAbsorb = 0;
                    targ.EndAction(typeof(RechargeSpell));
                    targ.SendMessage("Reactive armor has been nullified.");
                }

                if (targ.MagicDamageAbsorb > 0)
                {
                    targ.MagicDamageAbsorb = 0;
                    targ.SendMessage("Magic Reflection has been nullified.");
                }

                StatMod mod;
                mod = targ.GetStatMod("[Magic] Str Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Str Offset");

                mod = targ.GetStatMod("[Magic] Dex Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Dex Offset");

                mod = targ.GetStatMod("[Magic] Int Offset");
                if (mod != null)
                    targ.RemoveStatMod("[Magic] Int Offset");

                targ.Paralyzed = false;

                BuffInfo.RemoveBuff(targ, BuffIcon.Clumsy);
                BuffInfo.RemoveBuff(targ, BuffIcon.FeebleMind);
                BuffInfo.RemoveBuff(targ, BuffIcon.Weaken);
                BuffInfo.RemoveBuff(targ, BuffIcon.MassCurse);
                BuffInfo.RemoveBuff(targ, BuffIcon.Agility);
                BuffInfo.RemoveBuff(targ, BuffIcon.Cunning);
                BuffInfo.RemoveBuff(targ, BuffIcon.Strength);
                BuffInfo.RemoveBuff(targ, BuffIcon.Bless);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : refresh() in Manager.cs");
            }
		}

		#region Tournament Rewards
		/// <summary>
		/// Rewards the top 3 place winners by giving the specified item(s) and
		/// placing them in thier banks.Notifies the winners that thier winnings
		/// have been placed in thier bank.
		/// Loads the previous tournaments.xml file
		/// Saves the results to the tournaments.xml file
		/// Removes the spectator gates.
		/// Moves the winners so they may leave.
		/// Stops the tournament timer to officially end the tournament.
		/// </summary>
		/// <param name="first"> First place contestant</param>
		/// <param name="second"> Second place contestant</param>
		/// <param name="third"> Third place contestant</param>
        public static void TournamentReward(Tournament t, string place, Teams team)
        {
            BankBox bank;
            Item prize = (Item)Activator.CreateInstance(t.Prizes[place].GetType(), false);
            Dupe.CopyProperties(prize,t.Prizes[place]);

            foreach(Mobile m in team.getOwners())
            {
                bank = m.BankBox;
                bank.AddItem(prize);
                m.SendMessage("Your winnings have been sent to your bankbox.");
                if (place.Contains("first"))
                    World.Broadcast(0,false, m.Name+" has won the " + t.TeamSize + " tournament.");
            }
        }
		#endregion

		#region XML File Manipulation
		/// <summary>
		/// Loads the preivous tournaments.xml file
		/// </summary>
		/*public static void Load()
		{
            try
            {
                string[] names = null;

                string title;
                Teams team = Contestants[0];
                List<PlayerMobile> players = team.getOwners();
                title = players.Count + "v" + players.Count + " - ";

                string filePath = Path.Combine("TournamentInfo", title + "Tournaments.xml");

                if (!File.Exists(filePath))
                    return;

                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlElement root = doc["tourneys"];

                foreach (XmlElement tourneys in root.GetElementsByTagName("tournament"))
                {
                    List<string> temp = new List<string>();
                    List<string> contests = new List<string>();

                    try
                    {
                        foreach (XmlElement wins in tourneys.GetElementsByTagName("winners"))
                        {
                            try
                            {
                                temp.Add(wins.InnerText);
                            }
                            catch
                            {
                                Console.WriteLine("Warning: Load failed");
                            }
                        }
                        foreach (XmlElement cots in tourneys.GetElementsByTagName("contestants"))
                        {
                            try
                            {
                                names = cots.InnerText.Split(',');

                                for (int ps = 0; ps < names.Length; ps++)
                                {
                                    contests.Add(names[ps]);
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Warning: Load failed");
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Warning: Load failed");
                    }

                    m_Tourneys.Add(tourneys.Attributes["Date"].Value.ToString(), temp);
                    Players.Add(tourneys.Attributes["Date"].Value.ToString(), contests);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : Load() in Manager.cs");
            }
		}
		
		/// <summary>
		/// Saves the inputted results to the tournaments.xml file.
		/// </summary>
		/// <param name="first"> First place contestant</param>
		/// <param name="second"> Second place contestant</param>
		/// <param name="third"> Third place contestant</param>
		public static void Save( Teams first, Teams second, Teams third )
		{
            try
            {
                if (!Directory.Exists("TournamentInfo"))
                    Directory.CreateDirectory("TournamentInfo");

                string title;
                Teams team = Contestants[0];
                List<PlayerMobile> players = team.getOwners();
                title = players.Count + "v" + players.Count + " - ";

                string filePath = Path.Combine("TournamentInfo", title + "tournaments.xml");

                using (StreamWriter op = new StreamWriter(filePath))
                {
                    XmlTextWriter xml = new XmlTextWriter(op);

                    xml.Formatting = Formatting.Indented;
                    xml.IndentChar = '\t';
                    xml.Indentation = 1;

                    xml.WriteStartDocument(true);

                    xml.WriteStartElement("tourneys");

                    xml.WriteStartElement("tournament");
                    xml.WriteAttributeString("Date", DateTime.Now.ToString());

                    //Writing First Place to xml
                    xml.WriteStartElement("winners");
                    xml.WriteAttributeString("Place", "First");
                    players = first.getOwners();
                    for (int i = 0; i < players.Count; i++)
                    {
                        if ((i + 1) == players.Count)
                            xml.WriteString(String.Format("{0}", players[i].Name));
                        else
                            xml.WriteString(String.Format("{0}, ", players[i].Name));
                    }
                    xml.WriteEndElement();

                    // Writing Second Place to xml
                    xml.WriteStartElement("winners");
                    xml.WriteAttributeString("Place", "Second");
                    players = second.getOwners();
                    for (int i = 0; i < players.Count; i++)
                    {
                        if ((i + 1) == players.Count)
                            xml.WriteString(String.Format("{0}", players[i].Name));
                        else
                            xml.WriteString(String.Format("{0}, ", players[i].Name));
                    }
                    xml.WriteEndElement();

                    // Writing Third Place to xml
                    xml.WriteStartElement("winners");
                    xml.WriteAttributeString("Place", "Third");
                    players = third.getOwners();
                    for (int i = 0; i < players.Count; i++)
                    {
                        if ((i + 1) == players.Count)
                            xml.WriteString(String.Format("{0}", players[i].Name));
                        else
                            xml.WriteString(String.Format("{0}, ", players[i].Name));
                    }
                    xml.WriteEndElement();

                    xml.WriteStartElement("contestants");

                    for (int i = 0; i < Contestants.Count; i++)
                    {
                        team = Contestants[i];
                        players = team.getOwners();

                        for (int j = 0; j < players.Count; j++)
                        {
                            if ((i + 1) < Contestants.Count && (j + 1) < players.Count)
                                xml.WriteString(String.Format("{0}, ", players[j].Name));
                            else
                                xml.WriteString(String.Format("{0}", players[j].Name));
                        }
                    }

                    xml.WriteEndElement();

                    xml.WriteEndElement();

                    foreach (string s in m_Tourneys.Keys)
                    {
                        List<string> temp = new List<string>();
                        List<string> contests = new List<string>();

                        if (m_Tourneys.TryGetValue(s, out temp))
                        {
                        }

                        xml.WriteStartElement("tournament");
                        xml.WriteAttributeString("Date", s);

                        for (int i = 0; i < temp.Count; i++)
                        {
                            xml.WriteStartElement("winners");

                            if (i == 0)
                                xml.WriteAttributeString("Place", "First");
                            else if (i == 1)
                                xml.WriteAttributeString("Place", "Second");
                            else
                                xml.WriteAttributeString("Place", "Third");

                            xml.WriteString(temp[i]);
                            xml.WriteEndElement();
                        }

                        foreach (string q in Players.Keys)
                        {
                            if (q == s)
                            {
                                if (Players.TryGetValue(s, out contests))
                                {
                                }

                                xml.WriteStartElement("contestants");

                                for (int j = 0; j < contests.Count; j++)
                                {
                                    if ((j + 1) < Contestants.Count)
                                        xml.WriteString(String.Format("{0}, ", contests[j].Trim()));
                                    else
                                        xml.WriteString(String.Format("{0}", contests[j].Trim()));
                                }

                                xml.WriteEndElement();

                                break;
                            }
                        }

                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                    xml.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
                Console.WriteLine("Location : Save() in Manager.cs");
            }
		}*/
		#endregion
	}
}