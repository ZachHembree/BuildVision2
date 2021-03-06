﻿using RichHudFramework.UI;
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
        public GunBaseAccessor Weapon  { get { return _weapon; } private set { _weapon = value; } }

        private GunBaseAccessor _weapon;

        public class GunBaseAccessor : SubtypeAccessor<IMyGunBaseUser>
        {
            /// <summary>
            /// Lists the supported ammo types.
            /// </summary>
            public IReadOnlyList<MyItemType> AmmoTypes => ammoTypes;

            public string AmmoName { get; private set; }

            private readonly List<MyItemType> ammoTypes;
            private readonly StringBuilder nameBuilder;

            public GunBaseAccessor()
            {
                ammoTypes = new List<MyItemType>();
                nameBuilder = new StringBuilder();
            }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.GunBase);

                if (subtype != null)
                {
                    ammoTypes.Clear();
                    (subtype.AmmoInventory as IMyInventory).GetAcceptedItems(ammoTypes);

                    AmmoName = CleanTypeId(AmmoTypes[0].SubtypeId);
                }
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.DisplayName_BlueprintClass_Ammo)}: ", nameFormat);
                builder.Add($"{AmmoName}\n", valueFormat);
            }

            private string CleanTypeId(string subtypeId)
            {
                nameBuilder.Clear();
                nameBuilder.EnsureCapacity(subtypeId.Length + 4);
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
                        nameBuilder.Append(' ');

                    nameBuilder.Append(right);
                    left = right;
                }

                return nameBuilder.ToString();
            }
        }
    }
}