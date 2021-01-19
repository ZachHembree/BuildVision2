using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using System;
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
        public PowerAccessor Power { get; private set; }

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

            private readonly MyDefinitionId resourceId;
            private readonly MyResourceSinkComponentBase sink;
            private readonly IMyPowerProducer powerProducer;
            private readonly IMyFunctionalBlock functionalBlock;

            public PowerAccessor(SuperBlock block) : base(block)
            {
                IMyTerminalBlock tblock = block.TBlock;

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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                RichText summary = null;

                if (IsPowerSink || IsPowerProducer) // not functional, but powered
                {
                    if (functionalBlock != null) // functional w/ measurable power input/output
                    {
                        summary = new RichText
                        {
                            { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                            { $"{MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)} ", valueFormat },
                            { $"({GetPowerDisplay(Input, Output)})\n", nameFormat },
                        };
                    }
                    else // not functional
                    {
                        summary = new RichText
                        {
                            { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                            { $"{GetPowerDisplay(Input, Output)}\n", valueFormat },
                        };
                    }

                    summary += new RichText()
                    {
                        { $"{MyTexts.TrySubstitute("Max Power:")} ", nameFormat },
                        { $"{GetPowerDisplay(MaxInput, MaxOutput)}\n", valueFormat },
                    };
                }
                else if (functionalBlock != null) // not a sink or producer, but functional
                {
                    summary = new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                        { $"{MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)}\n", valueFormat },
                    };
                }

                return summary;
            }

            /// <summary>
            /// Returns a one-line summary of the power input/output in the format -Input/+Output
            /// </summary>
            public static string GetPowerDisplay(float? input, float? output)
            {
                float scale, total = 0f;
                string suffix, disp = "";

                if (input != null)
                    total += input.Value;

                if (output != null)
                    total += output.Value;

                TerminalUtilities.GetPowerScale(total, out scale, out suffix);

                if (input != null)
                    disp += "-" + (input * scale).Value.ToString("G4");

                if (output != null)
                {
                    if (input != null)
                        disp += " / ";

                    disp += "+" + (output * scale).Value.ToString("G4");
                }

                return $"{disp} {suffix}";
            }
        }
    }
}