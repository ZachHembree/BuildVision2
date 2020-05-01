using RichHudFramework.UI;
using System.Collections.Generic;
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

        public class GunBaseAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Lists the supported ammo types.
            /// </summary>
            public IReadOnlyList<MyItemType> AmmoTypes { get; private set; }

            public string AmmoName { get; }

            private readonly IMyGunBaseUser gunbase;

            public GunBaseAccessor(SuperBlock block) : base(block, TBlockSubtypes.GunBase)
            {
                gunbase = block.TBlock as IMyGunBaseUser;
                var acceptedItems = new List<MyItemType>();

                AmmoTypes = acceptedItems;
                (gunbase.AmmoInventory as IMyInventory).GetAcceptedItems(acceptedItems);

                AmmoName = CleanTypeId(AmmoTypes[0].SubtypeId);
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.DisplayName_BlueprintClass_Ammo)}: ", nameFormat },
                    { $"{AmmoName}\n", valueFormat },
                };
            }

            private static string CleanTypeId(string subtypeId)
            {
                StringBuilder sb = new StringBuilder(subtypeId.Length + 4);
                char left = 'A';

                for (int n = 0; n < subtypeId.Length; n++)
                {
                    char right = subtypeId[n];

                    if (right == '_')
                        right = ' ';
                    else if ((right >= 'A' && right <= 'Z') && !(left >= 'A' && left <= 'Z'))
                        sb.Append(' ');

                    sb.Append(right);
                    left = right;
                }

                return sb.ToString();
            }
        }
    }
}