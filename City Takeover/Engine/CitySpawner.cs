using System;
using System.Collections;

using Server;
using Server.Mobiles;

namespace CityTakeover
{
    public class CitySpawner
    {
        public CitySpawnList m_SpawnList;
        private BarrierCrystal m_Crystal;

        public CitySpawner(BarrierCrystal crystal)
        {
            m_Crystal = crystal;
            GetSpawnList();
        }

        public void GetSpawnList()
        {
            m_SpawnList = CitySpawnList.GetRandomSpawnList(m_Crystal.SpawnDifficulty);
        }

        public ArrayList SpawnMinions(int amount, BarrierCrystal crystal)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < amount; i++)
            {
                try
                {
                    Mobile m = Activator.CreateInstance(m_SpawnList.GetRandomMinion()) as Mobile;
                    m.OnBeforeSpawn(crystal.Location, crystal.Map);
                    m.MoveToWorld(crystal.Location, crystal.Map);
                    m.OnAfterSpawn();
                    list.Add(m);
                }
                catch { }
            }

            return list;
        }

        public ArrayList SpawnMinions(int amount, MinorBarrierCrystal crystal)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < amount; i++)
            {
                try
                {
                    Mobile m = Activator.CreateInstance(m_SpawnList.GetRandomMinion()) as Mobile;
                    m.OnBeforeSpawn(crystal.Location, crystal.Map);
                    m.MoveToWorld(crystal.Location, crystal.Map);
                    m.OnAfterSpawn();
                    list.Add(m);
                }
                catch { }
            }
            return list;
        }

        public Mobile SpawnCaptain(MinorBarrierCrystal crystal)
        {
            try
            {
                Mobile m = Activator.CreateInstance(m_SpawnList.GetRandomCaptain()) as Mobile;
                m.OnBeforeSpawn(crystal.Location, crystal.Map);
                m.MoveToWorld(crystal.Location, crystal.Map);
                m.OnAfterSpawn();
                return m;
            }
            catch { }

            return null;
        }

        public Mobile SpawnGeneral(BarrierCrystal crystal)
        {
            try
            {
                Mobile m = Activator.CreateInstance(m_SpawnList.GetRandomGeneral()) as Mobile;
                m.OnBeforeSpawn(crystal.Location, crystal.Map);
                m.MoveToWorld(crystal.Location, crystal.Map);
                m.OnAfterSpawn();
                return m;
            }
            catch { }
            return null;
        }
    }
}
