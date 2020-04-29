﻿using Sandbox.ModAPI;
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
            /// Returns block power input in megawatts if it has a <see cref="MyResourceSinkComponentBase"/>.
            /// </summary>
            public float? Input => sink?.CurrentInputByType(resourceId);

            /// <summary>
            /// Returns block power output in megawatts if the underlying fat block implements <see cref="IMyPowerProducer"/>.
            /// </summary>
            public float? Output => powerProducer?.CurrentOutput;

            private readonly MyDefinitionId resourceId;
            private readonly MyResourceSinkComponentBase sink;
            private readonly IMyPowerProducer powerProducer;
            private readonly IMyFunctionalBlock functionalBlock;

            public PowerAccessor(SuperBlock block) : base(block, TBlockSubtypes.None)
            {
                resourceId = MyDefinitionId.FromContent(block.TBlock.SlimBlock.GetObjectBuilder());
                sink = block.TBlock.ResourceSink;
                powerProducer = block.TBlock as IMyPowerProducer;
                functionalBlock = block.TBlock as IMyFunctionalBlock;

                if (sink != null || powerProducer != null)
                    block.SubtypeId |= TBlockSubtypes.Powered;

                if (functionalBlock != null)
                    block.SubtypeId |= TBlockSubtypes.Functional;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                RichText summary;

                if (Enabled != null && (Input != null || Output != null))
                {
                    summary = new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                        { $"{MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)} ", valueFormat },
                        { $"({GetPowerDisplay()})\n", nameFormat },
                    };
                }
                else if (Enabled != null)
                {
                    summary = new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                        { $"{MyTexts.GetString(Enabled.Value ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)}\n", valueFormat },
                    };
                }
                else
                {
                    summary = new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                        { $"{GetPowerDisplay()}\n", valueFormat },
                    };
                }

                return summary;
            }

            /// <summary>
            /// Returns a one-line summary of the power input/output in the format -Input/+Output
            /// </summary>
            public string GetPowerDisplay()
            {
                float scale, total = 0f;
                string suffix, disp = "";

                if (Input != null)
                    total += Input.Value;

                if (Output != null)
                    total += Output.Value;

                TerminalExtensions.GetPowerScale(total, out scale, out suffix);

                if (Input != null)
                    disp += "-" + (Input * scale).Value.ToString("G4");

                if (Output != null)
                {
                    if (Input != null)
                        disp += " / ";

                    disp += "+" + (Output * scale).Value.ToString("G4");
                }

                return $"{disp} {suffix}";
            }
        }
    }
}