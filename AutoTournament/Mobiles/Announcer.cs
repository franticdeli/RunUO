/****************************************************************\
* File Name : Announcer.cs                                       *
* Developer : Taryen(Mark S.)                                    *
* Orig. Date: 02/17/2013                                         *
* Desc.     : A mobile with an interesting look                  *
\****************************************************************/

/************************  Changelog  ***************************\
|    Date    |                    Changes                        |
------------------------------------------------------------------
|  02/17/13  | Initial File                                      |
\****************************************************************/

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Tournaments.Mobiles
{
	public class Announcer : SeekerOfAdventure
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
		[Constructable]
		public Announcer()
		{
            Title = "the announcer";
		}
        #endregion Initializations and Constructor

        #region Overrides
        /// <summary>
        /// Initialize the body of the Announcer
        /// </summary>
		public override void InitBody()
		{
			InitStats( 100, 100, 25 );
			
			int i;
			
			switch( i = Utility.Random( 3 ) )
			{
				case 1:
				{
					Body = 0x190;
					Hue = 2212;
					SpeechHue = 2212;
					Name = "Tonberry";
					AddItem( new Robe( 544 ) );
					AddItem( new Lantern() );
					AddItem( new ButcherKnife() );
					AddItem( new Shoes( 544 ) );
					break;
				}
				default:
				{
					Body = 0x190;
					Hue = 4;
					HairItemID = 8251; // Short Hair
					FacialHairItemID = 8267; //Short Full Beard
					FacialHairHue = 1150;
					HairHue = 1150;
					SpeechHue = 37;
					Name = "Papa Smurf";
					AddItem( new Server.Items.FloppyHat( 37 ) );
					AddItem( new Server.Items.ShortPants( 37 ) );
					break;
				}
			}
		}
		
        /// <summary>
        /// Initializes the outfit of the Announcer
        /// </summary>
		public override void InitOutfit()
		{
		}
        #endregion Overrides

        #region Serialization
        /// <summary>
        /// Allows the Announcer mobile to be saved and loaded
        /// </summary>
        /// <param name="serial"></param>
        public Announcer( Serial serial ) : base( serial )
		{
		}

        /// <summary>
        /// Saves the Announcer mobile
        /// </summary>
        /// <param name="writer"></param>
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

        /// <summary>
        /// Loaads the Announcer mobile
        /// </summary>
        /// <param name="reader"></param>
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
        }
        #endregion Serialization
    }
}