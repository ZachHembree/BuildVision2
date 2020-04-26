using Sandbox.ModAPI;
using System.Text;
using System.Collections.Generic;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using VRage.Utils;
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

            private readonly IMyLargeTurretBase turret;

            public TurretAccessor(SuperBlock block) : base(block, TBlockSubtypes.Turret)
            {
                turret = block.TBlock as IMyLargeTurretBase;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LargeTurretRadius)}: ", nameFormat },
                    { $"{TerminalExtensions.GetDistanceString(Range)}\n", valueFormat },
                };
            }
        }
    }
}