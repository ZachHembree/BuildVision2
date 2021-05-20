using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using System;
using System.Text;
using VRage;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block power information, if defined.
        /// </summary>
        public PowerAccessor Power  { get { return _power; } private set { _power = value; } }

        private PowerAccessor _power;

        public class PowerAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Indicates whether or not block power is enabled (if it can be disabled).
            /// </summary>
            public bool? Enabled 
            { 
                get { return functionalBlock?.Enabled; } 
                set { if (functionalBlock != null) functionalBlock.Enabled = value.Value; } 
            }

            /// <summary>
            /// Returns true if the block can draw power from the grid.
            /// </summary>
            public bool IsPowerSink => sink != null && sink.MaxRequiredInputByType(resourceId) > 0f;

            /// <summary>
            /// Returns true if the block can contribute power to the grid.
            /// </summary>
            public bool IsPowerProducer => powerProducer != null;

            /// <summary>
            /// Returns current block power draw in megawatts, if defined.
            /// </summary>
            public float? Input => IsPowerSink ? sink?.CurrentInputByType(resourceId) : null;

            /// <summary>
            /// Returns power input required for the block to function, if defined. Units in megawatts.
            /// </summary>
            public float? RequiredInput => IsPowerSink ? sink?.RequiredInputByType(resourceId) : null;

            /// <summary>
            /// Returns the blocks maximum possible power draw, if defined. Units in megawatts.
            /// </summary>
            public float? MaxInput => IsPowerSink ? sink?.MaxRequiredInputByType(resourceId) : null;

            /// <summary>
            /// Returns block power output in megawatts if the underlying fat block implements <see cref="IMyPowerProducer"/>.
            /// </summary>
            public float? Output => powerProducer?.CurrentOutput;

            /// <summary>
            /// Returns maximum block power output in megawatts if the underlying fat block implements <see cref="IMyPowerProducer"/>.
            /// </summary>
            public float? MaxOutput => powerProducer?.MaxOutput;

            private MyDefinitionId resourceId;
            private MyResourceSinkComponentBase sink;
            private IMyPowerProducer powerProducer;
            private IMyFunctionalBlock functionalBlock;

            public override void SetBlock(SuperBlock block)
            {
                IMyTerminalBlock tblock = block.TBlock;
                this.block = block;

                if (tblock.ResourceSink != null || tblock is IMyPowerProducer || tblock is IMyFunctionalBlock)
                {
                    resourceId = MyDefinitionId.FromContent(block.TBlock.SlimBlock.GetObjectBuilder());

                    sink = tblock.ResourceSink;
                    powerProducer = tblock as IMyPowerProducer;
                    functionalBlock = tblock as IMyFunctionalBlock;

                    if (sink != null || powerProducer != null)
                        SubtypeId |= TBlockSubtypes.Powered;

                    if (functionalBlock != null)
                        SubtypeId |= TBlockSubtypes.Functional;

                    block.SubtypeId |= SubtypeId;
                    block.subtypeAccessors.Add(this);
                }
            }

            public override void Reset()
            {
                base.Reset();
                resourceId = default(MyDefinitionId);
                sink = null;
                powerProducer = null;
                functionalBlock = null;
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (IsPowerSink || IsPowerProducer) // not functional, but powered
                {
                    var buf = block.textBuffer;

                    if (functionalBlock != null) // functional w/ measurable power input/output
                    {
                        builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower), nameFormat);
                        builder.Add(": ", nameFormat);

                        // Functional status (on/off)
                        builder.Add(MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off), valueFormat);
                        builder.Add(" ", valueFormat);

                        // Power input/output
                        buf.Clear();
                        buf.Append('(');
                        GetPowerDisplay(Input, Output, buf);
                        buf.Append(")\n");

                        builder.Add(buf, nameFormat);
                    }
                    else // not functional
                    {
                        builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower), nameFormat);
                        builder.Add(": ", nameFormat);

                        buf.Clear();
                        GetPowerDisplay(Input, Output, buf);
                        buf.Append('\n');

                        builder.Add(buf, valueFormat);
                    }

                    // Max in/out
                    builder.Add(MyTexts.TrySubstitute("Max Power"), nameFormat);
                    builder.Add(": ", nameFormat);

                    buf.Clear();
                    GetPowerDisplay(MaxInput, MaxOutput, buf);
                    buf.Append('\n');

                    builder.Add(buf, valueFormat);
                }
                else if (functionalBlock != null) // not a sink or producer, but functional
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower), nameFormat);
                    builder.Add(": ", nameFormat);

                    // Functional status (on/off)
                    builder.Add(MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off), valueFormat);
                    builder.Add(" \n", valueFormat);
                }
            }

            /// <summary>
            /// Returns a one-line summary of the power input/output in the format -Input/+Output
            /// </summary>
            public static void GetPowerDisplay(float? input, float? output, StringBuilder dst)
            {
                float scale, total = 0f;

                if (input != null)
                    total += input.Value;

                if (output != null)
                    total += output.Value;

                string suffix;
                TerminalUtilities.GetPowerScale(total / 10f, out scale, out suffix);

                if (input != null)
                {
                    dst.Append("-");
                    dst.AppendFormat("{0:G4}", (input * scale).Value);
                }
                if (output != null)
                {
                    if (input != null)
                        dst.Append(" / ");

                    dst.Append("+");
                    dst.AppendFormat("{0:G4}", (output * scale).Value);
                }

                dst.Append(' ');
                dst.Append(suffix);
            }
        }
    }
}