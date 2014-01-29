/****************************************************************\
* File Name : RewardToken.cs                                     *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 5/18/2010                                          *
* Desc.     : A reward token item. A gold coin object that is    *
*             hued white.                                        *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  03/19/12  | Logic Check                                       |
\****************************************************************/

using System;

using Server;
using Server.Items;

namespace Tournaments.Items
{
	public class RewardToken : Item
	{
		[Constructable]
		public RewardToken() : this( 1 ) { }

		[Constructable]
		public RewardToken( int amount )
			: base( 0xEEF )
		{
			Stackable = true;
			Amount = amount;

			Hue = 56;
			LootType = LootType.Regular;
			Name = String.Format( "reward token{0}", (amount > 1 ? "s" : "") );
			Weight = 0.1;
		}

		public RewardToken( Serial serial ) : base( serial ) { }

		public override int GetDropSound()
		{
			if( Amount <= 1 )
				return 0x2E4;
			else if( Amount <= 5 )
				return 0x2E5;

			return 0x2E6;
		}

		protected override void OnAmountChange( int oldValue )
		{
			base.OnAmountChange( oldValue );

			Name = String.Format( "reward token{0}", (Amount > 1 ? "s" : "") );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
		}
	}
}