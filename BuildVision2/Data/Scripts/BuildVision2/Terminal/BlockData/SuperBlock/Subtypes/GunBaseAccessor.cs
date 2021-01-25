using RichHudFramework.UI;
using System.Collections.Generic;
using System;
using System.Text;
using VRage.Game.ModAPI;
using VRage;
using IMyGunBaseUser = Sandbox.Game.Entities.IMyGunBaseUser;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public GunBaseAccessor Weapon { get; private set; }

        public class GunBaseAccessor : SubtypeAccessor<IMyGunBaseUser>
        {
            /// <summary>
            /// Lists the supported ammo types.
            /// </summary>
            public IReadOnlyList<MyItemType> AmmoTypes { get; private set; }

            public string AmmoName { get; }

            public GunBaseAccessor(SuperBlock block) : base(block, TBlockSubtypes.GunBase)
            {
                if (subtype != null)
                {
                    var acceptedItems = new List<MyItemType>();

                    AmmoTypes = acceptedItems;
                    (subtype.AmmoInventory as IMyInventory).GetAcceptedItems(acceptedItems);

                    AmmoName = CleanTypeId(AmmoTypes[0].SubtypeId);
                }
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.DisplayName_BlueprintClass_Ammo)}: ", nameFormat);
                builder.Add($"{AmmoName}\n", valueFormat);
            }

            private static string CleanTypeId(string subtypeId)
            {
                StringBuilder sb = new StringBuilder(subtypeId.Length + 4);
                char left = 'A';

                for (int n = 0; n < subtypeId.Length; n++)
                {
                    char right = subtypeId[n];
                    bool rightCapital = right >= 'A' && right <= 'Z',
                        rightNumber = right >= '0' && right <= '9',
                        leftNumber = left >= '0' && left <= '9',
                        leftCapital = left >= 'A' && left <= 'Z';

                    if (right == '_')
                        right = ' ';
                    else if (!leftCapital && ((rightCapital && left != ' ') || (rightNumber && !leftNumber)))
                        sb.Append(' ');

                    sb.Append(right);
                    left = right;
                }

                return sb.ToString();
            }
        }
    }
}