using System;
using System.Collections.Generic;

using Server.Items;

namespace CityTakeover
{
    public class BarrierCrystalRechargeInfo
    {
        public static readonly BarrierCrystalRechargeInfo[] Sanctify = new BarrierCrystalRechargeInfo[]
			{
				new BarrierCrystalRechargeInfo( typeof( Citrine ), 2 ),
				new BarrierCrystalRechargeInfo( typeof( Amber ), 2 ),
				new BarrierCrystalRechargeInfo( typeof( Tourmaline ), 4 ),
				new BarrierCrystalRechargeInfo( typeof( Emerald ), 6 ),
				new BarrierCrystalRechargeInfo( typeof( Sapphire ), 6 ),
				new BarrierCrystalRechargeInfo( typeof( Amethyst ), 6 ),
				new BarrierCrystalRechargeInfo( typeof( StarSapphire ), 8 ),
				new BarrierCrystalRechargeInfo( typeof( Diamond ), 12 ),
                new BarrierCrystalRechargeInfo( typeof( MinorRechargeGem ), 50 ),
                new BarrierCrystalRechargeInfo( typeof( RechargeGem ), 100 ),
                new BarrierCrystalRechargeInfo( typeof( MajorRechargeGem ), 250 ),
                new BarrierCrystalRechargeInfo( typeof( MassiveRechargeGem ), 500 )
			};

        public static readonly BarrierCrystalRechargeInfo[] Desecrate = new BarrierCrystalRechargeInfo[]
			{
                new BarrierCrystalRechargeInfo( typeof( Skull ), 6 ),
                new BarrierCrystalRechargeInfo( typeof( HugeSkull ), 12 ),
                new BarrierCrystalRechargeInfo( typeof( DarkMinorRechargeGem ), 50 ),
                new BarrierCrystalRechargeInfo( typeof( DarkRechargeGem ), 100 ),
                new BarrierCrystalRechargeInfo( typeof( DarkMajorRechargeGem ), 250 ),
                new BarrierCrystalRechargeInfo( typeof( DarkMassiveRechargeGem ), 500 )
			};

        public static BarrierCrystalRechargeInfo Get(Type type, bool Desecrated)
        {
            if (Desecrated)
            {
                foreach (BarrierCrystalRechargeInfo info in Desecrate)
                {
                    if (info.Type == type)
                        return info;
                }
            }
            else
            {
                foreach (BarrierCrystalRechargeInfo info in Sanctify)
                {
                    if (info.Type == type)
                        return info;
                }
            }

            return null;
        }

        private Type m_Type;
        private int m_Amount;

        public Type Type { get { return m_Type; } }
        public int Amount { get { return m_Amount; } }

        private BarrierCrystalRechargeInfo(Type type, int amount)
        {
            m_Type = type;
            m_Amount = amount;
        }
    }
}
