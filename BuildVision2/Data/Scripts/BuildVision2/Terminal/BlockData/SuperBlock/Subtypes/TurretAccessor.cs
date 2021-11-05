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
        public TurretAccessor Turret  { get { return _turret; } private set { _turret = value; } }

        private TurretAccessor _turret;

        public class TurretAccessor : SubtypeAccessor<IMyLargeTurretBase>
        {
            /// <summary>
            /// Indicates the maximum targeting range for the turret.
            /// </summary>
            public float Range => subtype.Range;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Turret);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                // Targeting radius
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LargeTurretRadius), nameFormat);
                builder.Add(": ", nameFormat);

                block.textBuffer.Clear();
                TerminalUtilities.GetDistanceDisplay(Range, block.textBuffer);
                block.textBuffer.Append('\n');

                builder.Add(block.textBuffer, valueFormat);
            }
        }
    }
}