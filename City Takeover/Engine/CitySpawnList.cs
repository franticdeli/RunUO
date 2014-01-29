using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;

namespace CityTakeover
{
    public class CitySpawnList
    {
        #region Initialization
        private string m_Type;
        private List<Type> m_Minions = new List<Type>();
        private List<Type> m_Captains = new List<Type>();
        private List<Type> m_General = new List<Type>();

        public List<Type> Minions { get { return m_Minions; } }
        public List<Type> Captains { get { return m_Captains; } }
        public List<Type> General { get { return m_General; } }

        public string Type { get { return m_Type; } }

        private static Dictionary<string, CitySpawnList> m_Notset;
        private static Dictionary<string, CitySpawnList> m_Novice;
        private static Dictionary<string, CitySpawnList> m_Expert;
        private static Dictionary<string, CitySpawnList> m_Master;
        #endregion Initialization

        public CitySpawnList(string type, XmlElement xml)
        {
            m_Type = type;
            foreach (XmlElement element in xml.GetElementsByTagName("minions"))
            {
                foreach (XmlElement obj in element.GetElementsByTagName("object"))
                {
                    Type mobile = null;
                    if (!Region.ReadType(obj, "type", ref mobile)){
                    }
                    m_Minions.Add(mobile);
                }
            }
           
            foreach (XmlElement element in xml.GetElementsByTagName("captains"))
            {
                foreach (XmlElement obj in element.GetElementsByTagName("object"))
                {
                    Type mobile = null;
                    if (!Region.ReadType(obj, "type", ref mobile)){
                    }
                    m_Captains.Add(mobile);
                }
            }
            foreach (XmlElement element in xml.GetElementsByTagName("general"))
            {
                foreach (XmlElement obj in element.GetElementsByTagName("object"))
                {
                    Type mobile = null;
                    if (!Region.ReadType(obj, "type", ref mobile)){
                    }
                    m_General.Add(mobile);
                }
            }
        }

        #region RandomGets
        public static CitySpawnList GetRandomSpawnList(BarrierCrystal.Difficulty difficulty )
        {
            switch (difficulty)
            {
                case BarrierCrystal.Difficulty.Novice:
                    {
                        if (m_Novice != null)
                        {
                            List<CitySpawnList> lists = new List<CitySpawnList>();
                            foreach (string list in m_Novice.Keys)
                            {
                                CitySpawnList n;
                                m_Novice.TryGetValue(list, out n);
                                lists.Add(n);
                            }

                            if (lists.Count > 0)
                                return lists[Utility.Random(lists.Count)];
                        }
                        break;
                    }
                case BarrierCrystal.Difficulty.Expert:
                    {
                        if (m_Expert != null)
                        {
                            List<CitySpawnList> lists = new List<CitySpawnList>();
                            foreach (string list in m_Expert.Keys)
                            {
                                CitySpawnList n;
                                m_Expert.TryGetValue(list, out n);
                                lists.Add(n);
                            }

                            if (lists.Count > 0)
                                return lists[Utility.Random(lists.Count)];
                        }
                        break;
                    }
                case BarrierCrystal.Difficulty.Master:
                    {
                        if (m_Master != null)
                        {
                            List<CitySpawnList> lists = new List<CitySpawnList>();
                            foreach (string list in m_Master.Keys)
                            {
                                CitySpawnList n;
                                m_Master.TryGetValue(list, out n);
                                lists.Add(n);
                            }

                            if (lists.Count > 0)
                                return lists[Utility.Random(lists.Count)];
                        }
                        break;
                    }
            }

            if (m_Notset != null)
            {
                List<CitySpawnList> lists = new List<CitySpawnList>();
                foreach (string list in m_Notset.Keys)
                {
                    CitySpawnList n;
                    m_Notset.TryGetValue(list, out n);
                    lists.Add(n);
                }

                if (lists.Count > 0)
                    return lists[Utility.Random(lists.Count)];
            }
            return null;
        }

        public Type GetRandomMinion()
        {
            if (m_Minions.Count > 0)
                return m_Minions[Utility.Random(m_Minions.Count)];

            return null;
        }

        public Type GetRandomCaptain()
        {
            if (m_Captains.Count > 0)
                return m_Captains[Utility.Random(m_Captains.Count)];

            return null;
        }

        public Type GetRandomGeneral()
        {
            if (m_General.Count > 0)
                return m_General[Utility.Random(m_General.Count)];

            return null;
        }
        #endregion RandomGets

        static CitySpawnList()
        {
            m_Notset = new Dictionary<string, CitySpawnList>(StringComparer.OrdinalIgnoreCase);
            m_Novice = new Dictionary<string, CitySpawnList>(StringComparer.OrdinalIgnoreCase);
            m_Expert = new Dictionary<string, CitySpawnList>(StringComparer.OrdinalIgnoreCase);
            m_Master = new Dictionary<string, CitySpawnList>(StringComparer.OrdinalIgnoreCase);

            string filePath = Path.Combine(Core.BaseDirectory, "Data/cityspawnsets.xml");

            if (!File.Exists(filePath))
                return;

            try
            {
                Load(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: Exception caught loading city spawn lists:");
                Console.WriteLine(e);
            }
        }

        private static void Load(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlElement root = doc["spawnsets"];

            foreach (XmlElement element in root.GetElementsByTagName("set"))
            {
                string type = element.GetAttribute("type");
                string difficulty = element.GetAttribute("difficulty");
                difficulty = difficulty.ToLower();

                if (String.IsNullOrEmpty(type))
                    continue;

                try
                {
                    CitySpawnList list = new CitySpawnList(type, element);
                    switch (difficulty)
                    {
                        case "novice":
                            {
                                m_Novice[type] = list;
                                break;
                            }
                        case "expert":
                            {
                                m_Expert[type] = list;
                                break;
                            }
                        case "master":
                            {
                                m_Master[type] = list;
                                break;
                            }
                        default:
                            {
                                m_Notset[type] = list;
                                break;
                            }
                    }
                }
                catch
                {
                    Console.WriteLine("Error: City Spawn List not created.");
                }
            }
        }
    }
}
