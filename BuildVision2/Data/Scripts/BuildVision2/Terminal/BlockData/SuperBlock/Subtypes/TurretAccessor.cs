﻿using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to turret members, if defined.
        /// </summary>
        public TurretAccessor Turret { get; private set; }

        public class TurretAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Indicates the maximum targeting range for the turret.
            /// </summary>
            public float Range => turret.Range;

            /// <summary>
            /// Lists the supported ammo types.
            /// </summary>
            public IReadOnlyList<MyItemType> AmmoTypes { get; private set; }

            private readonly IMyLargeTurretBase turret;

            public TurretAccessor(SuperBlock block) : base(block, TBlockSubtypes.Turret)
            {
                turret = block.TBlock as IMyLargeTurretBase;
                
                var acceptedItems = new List<MyItemType>();
                AmmoTypes = acceptedItems;
                block.Inventory.Inventory.GetAcceptedItems(acceptedItems);
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LaserRange)}: ", nameFormat },
                    { $"{Range.Round(2)}m", valueFormat },
                };
            }
        }
    }
}