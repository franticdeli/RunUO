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
using Server.Misc;
using Server.Targeting;

using EventScheduler;

using Tournaments.Gumps;
using Tournaments.Regions;

namespace Tournaments.Items
{
    public enum ArenaType
    {
        OneVsOne,
        TwoVsTwo,
        ThreeVsThree,
        FourVsFour,
        FiveVsFive
    }

    public class ArenaControl : Item
    {
        public static Dictionary<ArenaType, List<ArenaControl>> Arenas = new Dictionary<ArenaType, List<ArenaControl>>();

        #region Initialization
        private bool m_Active;
        private bool m_Occupied;
        private bool m_Supplied;
        private string m_Set;
        private ArenaType m_Type;
        private Point3D m_AnnouncerSpot;
        private Point3D m_ExitArea;
        private List<Point3D> m_FightingAreaOne;
        private List<Point3D> m_FightingAreaTwo;
        private Point3D m_HoldingArea;
        private List<Point3D> m_StagingAreaOne;
        private List<Point3D> m_StagingAreaTwo;
        private Rectangle3D[] m_ArenaArea;
        private Rectangle3D[] m_SpectatorArea;
        public ArenaRegion m_Arena;
        public SpectatorRegion m_Spectator;
        
        #endregion Initialization

        #region Gets/Sets
        public bool Active
        {
            get { return m_Active; }
            set 
            { 
                m_Active = value;

                if (m_Active)
                {
                    foreach (ArenaType type in Arenas.Keys)
                    {
                        List<ArenaControl> arenas = Arenas[type];
                        if (arenas == null)
                            arenas = new List<ArenaControl>();
                        if (!arenas.Contains(this) && Type == type)
                            arenas.Add(this);
                        else if (arenas.Contains(this))
                            arenas.Remove(this);
                    }

                    if (!Arenas.ContainsKey(Type))
                    {
                        List<ArenaControl> list = new List<ArenaControl>();
                        list.Add(this);
                        Arenas.Add(Type, list);
                    }
                }
                else
                {
                    foreach (ArenaType type in Arenas.Keys)
                    {
                        List<ArenaControl> arenas = Arenas[type];
                        if (arenas == null)
                            arenas = new List<ArenaControl>();
                        else if (arenas.Contains(this))
                            arenas.Remove(this);
                    }
                }

                UpdateRegions();
                CleanArenaList();
            }
        }

        public bool Occupied
        {
            get { return m_Occupied; }
            set { m_Occupied = value; }
        }

        public bool Supplied
        {
            get { return m_Supplied; }
            set { m_Supplied = value; }
        }

        public string Set
        {
            get { return m_Set; }
            set { m_Set = value; }
        }

        public ArenaType Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public Point3D AnnouncerSpot
        {
            get { return m_AnnouncerSpot; }
            set { m_AnnouncerSpot = value; }
        }

        public Point3D ExitArea
        {
            get { return m_ExitArea; }
            set { m_ExitArea = value; }
        }

        public List<Point3D> FightingAreaOne
        {
            get { return m_FightingAreaOne; }
            set { m_FightingAreaOne = value; }
        }

        public List<Point3D> FightingAreaTwo
        {
            get { return m_FightingAreaTwo; }
            set { m_FightingAreaTwo = value; }
        }

        public Point3D HoldingArea
        {
            get { return m_HoldingArea; }
            set { m_HoldingArea = value; }
        }

        public List<Point3D> StagingAreaOne
        {
            get { return m_StagingAreaOne; }
            set { m_StagingAreaOne = value; }
        }

        public List<Point3D> StagingAreaTwo
        {
            get { return m_StagingAreaTwo; }
            set { m_StagingAreaTwo = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle3D[] ArenaArea
        {
            get { return m_ArenaArea; }
            set { m_ArenaArea = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle3D[] SpectatorArea
        {
            get { return m_SpectatorArea; }
            set { m_SpectatorArea = value; }
        }

        public ArenaRegion Arena
        {
            get { return m_Arena; }
        }

        public SpectatorRegion Spectator
        {
            get { return m_Spectator; }
        }
        #endregion Gets/Sets

        [Constructable]
        public ArenaControl()
        {
            ItemID = 0x1f13;
            Visible = false;

            Name = "General Arena";
            m_Active = false;
            m_Occupied = false;
            m_Supplied = false;
            m_Set = "General";
            m_Type = ArenaType.OneVsOne;
            m_AnnouncerSpot = new Point3D(0,0,0);
            m_HoldingArea = new Point3D(0, 0, 0);
            m_ExitArea = new Point3D(0,0,0);
            m_FightingAreaOne = new List<Point3D>();
            m_FightingAreaTwo = new List<Point3D>();
            m_StagingAreaOne = new List<Point3D>();
            m_StagingAreaTwo = new List<Point3D>();
            m_Arena = new ArenaRegion(this);
            m_Spectator = new SpectatorRegion(this);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.GameMaster && !NameTaken(Name))
            {
                from.SendGump(new ArenaInfoGump(from, this));
            }
            else if (from.AccessLevel > AccessLevel.GameMaster && NameTaken(Name))
                from.SendMessage("You must set unique name for the arena first.");
        }

        public override void OnDelete()
        {
            foreach (ArenaType type in Arenas.Keys)
            {
                List<ArenaControl> arenas = Arenas[type];
                if (arenas == null)
                    arenas = new List<ArenaControl>();
                else if (arenas.Contains(this))
                    arenas.Remove(this);
            }
            CleanArenaList();

            if (m_Arena != null)
                m_Arena.Unregister();
            if (m_Spectator != null)
                m_Spectator.Unregister();

            base.OnDelete();
        }

        public ArenaControl(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((bool)m_Occupied);
            writer.Write((bool)m_Supplied);
            writer.Write((string)m_Set);
            writer.Write((int)m_Type);
            writer.Write((Point3D)m_AnnouncerSpot);
            writer.Write((Point3D)m_ExitArea);
            writer.Write((int)m_FightingAreaOne.Count);
            foreach (Point3D fAreaOne in m_FightingAreaOne)
                writer.Write((Point3D)fAreaOne);
            writer.Write((int)m_FightingAreaTwo.Count);
            foreach (Point3D fAreaTwo in m_FightingAreaTwo)
                writer.Write((Point3D)fAreaTwo);
            writer.Write((Point3D)m_HoldingArea);
            writer.Write((int)m_StagingAreaOne.Count);
            foreach (Point3D sAreaOne in m_StagingAreaOne)
                writer.Write((Point3D)sAreaOne);
            writer.Write((int)m_StagingAreaTwo.Count);
            foreach (Point3D sAreaTwo in m_StagingAreaTwo)
                writer.Write((Point3D)sAreaTwo);

            WriteRect3DArray(writer, m_ArenaArea);
            WriteRect3DArray(writer, m_SpectatorArea);

            writer.Write((bool)m_Active);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int index;
            m_FightingAreaOne = new List<Point3D>();
            m_FightingAreaTwo = new List<Point3D>();
            m_StagingAreaOne = new List<Point3D>();
            m_StagingAreaTwo = new List<Point3D>();

            m_Occupied = false;
            reader.ReadBool();
            m_Supplied = reader.ReadBool();
            m_Set = reader.ReadString();
            m_Type = (ArenaType)reader.ReadInt();
            m_AnnouncerSpot = reader.ReadPoint3D();
            m_ExitArea = reader.ReadPoint3D();
            index = reader.ReadInt();
            for (int i = 0; i < index; i++)
                m_FightingAreaOne.Add(reader.ReadPoint3D());
            index = reader.ReadInt();
            for (int i = 0; i < index; i++)
                m_FightingAreaTwo.Add(reader.ReadPoint3D());
            m_HoldingArea = reader.ReadPoint3D();
            index = reader.ReadInt();
            for (int i = 0; i < index; i++)
                m_StagingAreaOne.Add(reader.ReadPoint3D());
            index = reader.ReadInt();
            for (int i = 0; i < index; i++)
                m_StagingAreaTwo.Add(reader.ReadPoint3D());

            m_ArenaArea = ReadRect3DArray(reader);
            m_SpectatorArea = ReadRect3DArray(reader);

            Active = reader.ReadBool();

            UpdateRegions();
        }

        public static List<string> GetSets(ArenaType type)
        {
            List<string> sets = new List<string>();
            if (Arenas.ContainsKey(type))
            {
                List<ArenaControl> list = Arenas[type];
                foreach (ArenaControl a in list)
                    if (!sets.Contains(a.Set))
                        sets.Add(a.Set);
            }

            return sets;
        }

        public static List<ArenaControl> GetArenaSet(ArenaType type, string set)
        {
            List<ArenaControl> arenaset = new List<ArenaControl>();
            if (Arenas.ContainsKey(type))
            {
                arenaset.AddRange(Arenas[type]);
                arenaset.RemoveAll(delegate(ArenaControl a) { return a.Set == set; });
            }

            return arenaset;
        }

        public static List<ArenaControl> RandomArenaSet(ArenaType type)
        {
            List<ArenaControl> arenaset = new List<ArenaControl>();
            List<string> settypes = new List<string>();
            Random rand = new Random();
            string set = "";

            if (Arenas.ContainsKey(type))
            {
                arenaset.AddRange(Arenas[type]);
                foreach (ArenaControl a in arenaset)
                    if (!settypes.Contains(a.Set))
                        settypes.Add(a.Set);

                set = settypes[rand.Next(settypes.Count)];

                arenaset.RemoveAll(delegate(ArenaControl a) { return a.Set != set; });
            }

            return arenaset;
        }

        public static void CleanArenaList()
        {
            if (Arenas.ContainsKey(ArenaType.OneVsOne))
            {
                if (Arenas[ArenaType.OneVsOne] == null || Arenas[ArenaType.OneVsOne].Count == 0)
                {
                    Arenas.Remove(ArenaType.OneVsOne);
                    XMLDates.CleanTournamentList(ArenaType.OneVsOne);
                }
            }

            if (Arenas.ContainsKey(ArenaType.TwoVsTwo))
            {
                if (Arenas[ArenaType.TwoVsTwo] == null || Arenas[ArenaType.TwoVsTwo].Count == 0)
                {
                    Arenas.Remove(ArenaType.TwoVsTwo);
                    XMLDates.CleanTournamentList(ArenaType.TwoVsTwo);
                }
            }

            if (Arenas.ContainsKey(ArenaType.ThreeVsThree))
            {
                if (Arenas[ArenaType.ThreeVsThree] == null || Arenas[ArenaType.ThreeVsThree].Count == 0)
                {
                    Arenas.Remove(ArenaType.ThreeVsThree);
                    XMLDates.CleanTournamentList(ArenaType.ThreeVsThree);
                }
            }

            if (Arenas.ContainsKey(ArenaType.FourVsFour))
            {
                if (Arenas[ArenaType.FourVsFour] == null || Arenas[ArenaType.FourVsFour].Count == 0)
                {
                    Arenas.Remove(ArenaType.FourVsFour);
                    XMLDates.CleanTournamentList(ArenaType.FourVsFour);
                }
            }
            if (Arenas.ContainsKey(ArenaType.FiveVsFive))
            {
                if (Arenas[ArenaType.FiveVsFive] == null || Arenas[ArenaType.FiveVsFive].Count == 0)
                {
                    Arenas.Remove(ArenaType.FiveVsFive);
                    XMLDates.CleanTournamentList(ArenaType.FiveVsFive);
                }
            }
        }

        #region Modified and taken from Custom Regions in a Box
        public static Rectangle3D[] ReadRect3DArray(GenericReader reader)
        {
            int size = reader.ReadInt();
            List<Rectangle3D> newAry = new List<Rectangle3D>();

            for (int i = 0; i < size; i++)
            {
                Point3D start = reader.ReadPoint3D();
                Point3D end = reader.ReadPoint3D();
                newAry.Add(new Rectangle3D(start, end));
            }

            return newAry.ToArray();
        }

        public static void WriteRect3DArray(GenericWriter writer, Rectangle3D[] ary)
        {
            if (ary == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(ary.Length);

            for (int i = 0; i < ary.Length; i++)
            {
                Rectangle3D rect = ((Rectangle3D)ary[i]);
                writer.Write((Point3D)rect.Start);
                writer.Write((Point3D)rect.End);
            }
            return;
        }

        public bool NameTaken(string testName)
        {
            if (Arenas != null)
            {
                foreach (List<ArenaControl> list in Arenas.Values)
                {
                    foreach(ArenaControl control in list)
                        if (control.Name == testName && control != this)
                            return true;
                }
            }

            return false;
        }

        public void RemoveArenaArea(int index, Mobile from)
        {
            try
            {
                List<Rectangle3D> rects = new List<Rectangle3D>();
                foreach (Rectangle3D rect in m_ArenaArea)
                    rects.Add(rect);

                rects.RemoveAt(index);
                m_ArenaArea = rects.ToArray();

                UpdateRegions();
                from.SendMessage("Area Removed!");
            }
            catch
            {
                from.SendMessage("Removing of Area Failed!");
            }
        }

        public void ChooseArenaArea(Mobile m)
        {
            BoundingBoxPicker.Begin(m, new BoundingBoxCallback(ArenaRegion_Callback), this);
        }

        public void ArenaRegion_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
        {
            DoChooseArenaArea(from, map, start, end, state);
        }

        public void DoChooseArenaArea(Mobile from, Map map, Point3D start, Point3D end, object control)
        {
            if (this != null)
            {
                List<Rectangle3D> areas = new List<Rectangle3D>();

                if (this.m_ArenaArea != null)
                {
                    foreach (Rectangle3D rect in this.m_ArenaArea)
                        areas.Add(rect);
                }

                // Fix for incorrect X Coordinates...
                if (start.X < end.X)
                    end.X += 1;
                else
                    start.X += 1;
                // Fix for incorrect Y Coordinates...
                if (start.Y < end.Y)
                    end.Y += 1;
                else
                    start.Y += 1;

                // Added Lord Dio's Z Value Fix
                if (start.Z == end.Z || start.Z < end.Z)
                {
                    if (start.Z != Server.Region.MinZ)
                        start.Z = Server.Region.MinZ;
                    if (end.Z != Server.Region.MaxZ)
                        end.Z = Server.Region.MaxZ;
                }
                else
                {
                    if (start.Z != Server.Region.MaxZ)
                        start.Z = Server.Region.MaxZ;
                    if (end.Z != Server.Region.MinZ)
                        end.Z = Server.Region.MinZ;
                }

                Rectangle3D newrect = new Rectangle3D(start, end);
                areas.Add(newrect);

                this.m_ArenaArea = areas.ToArray();

                this.UpdateRegions();
                from.CloseGump(typeof(ArenaInfoGump));
                from.SendGump(new ArenaInfoGump());
                from.CloseGump(typeof(ArenaAreaGump));
                from.SendGump(new ArenaAreaGump(this));
            }
        }

        public void RemoveSpectatorArea(int index, Mobile from)
        {
            try
            {
                List<Rectangle3D> rects = new List<Rectangle3D>();
                foreach (Rectangle3D rect in m_SpectatorArea)
                    rects.Add(rect);

                rects.RemoveAt(index);
                m_SpectatorArea = rects.ToArray();

                UpdateRegions();
                from.SendMessage("Area Removed!");
            }
            catch
            {
                from.SendMessage("Removing of Area Failed!");
            }
        }

        public void ChooseSpectatorArea(Mobile m)
        {
            BoundingBoxPicker.Begin(m, new BoundingBoxCallback(SpectatorRegion_Callback), this);
        }

        public void SpectatorRegion_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
        {
            DoChooseSpectatorArea(from, map, start, end, state);
        }

        public void DoChooseSpectatorArea(Mobile from, Map map, Point3D start, Point3D end, object control)
        {
            if (this != null)
            {
                List<Rectangle3D> areas = new List<Rectangle3D>();

                if (this.m_SpectatorArea != null)
                {
                    foreach (Rectangle3D rect in this.m_SpectatorArea)
                        areas.Add(rect);
                }

                // Fix for incorrect X Coordinates...
                if (start.X < end.X)
                    end.X += 1;
                else
                    start.X += 1;
                // Fix for incorrect Y Coordinates...
                if (start.Y < end.Y)
                    end.Y += 1;
                else
                    start.Y += 1;

                // Added Lord Dio's Z Value Fix
                if (start.Z == end.Z || start.Z < end.Z)
                {
                    if (start.Z != Server.Region.MinZ)
                        start.Z = Server.Region.MinZ;
                    if (end.Z != Server.Region.MaxZ)
                        end.Z = Server.Region.MaxZ;
                }
                else
                {
                    if (start.Z != Server.Region.MaxZ)
                        start.Z = Server.Region.MaxZ;
                    if (end.Z != Server.Region.MinZ)
                        end.Z = Server.Region.MinZ;
                }

                Rectangle3D newrect = new Rectangle3D(start, end);
                areas.Add(newrect);

                this.m_SpectatorArea = areas.ToArray();

                this.UpdateRegions();
                from.CloseGump(typeof(ArenaInfoGump));
                from.SendGump(new ArenaInfoGump());
                from.CloseGump(typeof(SpectatorAreaGump));
                from.SendGump(new SpectatorAreaGump(this));
            }
        }

        public void UpdateRegions()
        {
            if (m_Arena != null)
                m_Arena.Unregister();

            if (this.Map != null && this.Active)
            {
                if (this != null && this.ArenaArea != null && this.ArenaArea.Length > 0)
                {
                    m_Arena = new ArenaRegion(this);
                    // m_Region.GoLocation = m_CustomGoLocation;  // REMOVED
                    m_Arena.Register();
                }
                else
                    m_Arena = null;
            }
            else
                m_Arena = null;

            if (m_Spectator != null)
                m_Spectator.Unregister();

            if (this.Map != null && this.Active)
            {
                if (this != null && this.SpectatorArea != null && this.SpectatorArea.Length > 0)
                {
                    m_Spectator = new SpectatorRegion(this);
                    // m_Region.GoLocation = m_CustomGoLocation;  // REMOVED
                    m_Spectator.Register();
                }
                else
                    m_Spectator = null;
            }
            else
                m_Spectator = null;
        }
        #endregion
    }
}
