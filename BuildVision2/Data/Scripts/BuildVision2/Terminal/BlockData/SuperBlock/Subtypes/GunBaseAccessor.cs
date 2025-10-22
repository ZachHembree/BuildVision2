using RichHudFramework.UI;
using Sandbox.Definitions;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using IMyGunBaseUser = Sandbox.Game.Entities.IMyGunBaseUser;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public GunBaseAccessor Weapon { get { return _weapon; } }

		private GunBaseAccessor _weapon;

		public class GunBaseAccessor : SubtypeAccessor<IMyGunBaseUser>
		{
			/// <summary>
			/// Definition for the required ammo
			/// </summary>
			public MyDefinitionBase AmmoDefinition => ammoDef;

			/// <summary>
			/// Retrieves the name of the ammo used in the GUI, localized if available.
			/// </summary>
			public string AmmoName => ammoDef.DisplayNameText;

			private readonly List<MyItemType> ammoTypes;
			private MyDefinitionBase ammoDef;

			public GunBaseAccessor()
			{
				ammoTypes = new List<MyItemType>();
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.GunBase);

				if (subtype != null)
				{
					ammoTypes.Clear();
					(subtype.AmmoInventory as IMyInventory)?.GetAcceptedItems(ammoTypes);
					ammoDef = MyDefinitionManager.Static.GetDefinition(ammoTypes[0]);
				}
			}

			public override void Reset()
			{
				base.Reset();
				ammoTypes.Clear();
				ammoDef = null;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				if (ammoDef != null)
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.DisplayName_BlueprintClass_Ammo), nameFormat);
					builder.Add(": ", nameFormat);

					builder.Add(ammoDef.DisplayNameText, valueFormat);
					builder.Add("\n", valueFormat);
				}
			}
		}
	}
}