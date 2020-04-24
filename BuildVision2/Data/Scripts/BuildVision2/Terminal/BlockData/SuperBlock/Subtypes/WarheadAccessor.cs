using Sandbox.ModAPI;
using System;
using VRageMath;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block warhead members, if defined.
        /// </summary>
        public WarheadAccessor Warhead { get; private set; }

        public class WarheadAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Controls the warhead detonation countdown in seconds.
            /// </summary>
            public float CountdownTime { get { return warhead.DetonationTime; } set { warhead.DetonationTime = value; } }

            /// <summary>
            /// Controls warhead arming.
            /// </summary>
            public bool IsArmed { get { return warhead.IsArmed; } set { warhead.IsArmed = value; } }

            /// <summary>
            /// Indicates whether or not the warhead is counting down.
            /// </summary>
            public bool IsCountingDown => warhead.IsCountingDown;

            /// <summary>
            /// Detonates the warhead.
            /// </summary>
            public readonly Action Detonate;

            private readonly IMyWarhead warhead;

            public WarheadAccessor(SuperBlock block) : base(block, TBlockSubtypes.Warhead)
            {
                warhead = block.TBlock as IMyWarhead;
                Detonate = warhead.Detonate;
            }

            /// <summary>
            /// Starts the countdown.
            /// </summary>
            public void StartCountdown() =>
                warhead.StartCountdown();

            /// <summary>
            /// Stops the countdown.
            /// </summary>
            public void StopCountdown() =>
                warhead.StopCountdown();

            /// <summary>
            /// Returns the status of the warhead (armed/disarmed) as a localized string).
            /// </summary>
            public string GetLocalizedStatus()
            {
                if (IsArmed)
                    return MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_SwitchTextArmed);
                else
                    return MyTexts.GetString(MySpaceTexts.TerminalControlPanel_Warhead_SwitchTextDisarmed);
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedStatus()} ", valueFormat },
                    { $"({Math.Truncate(CountdownTime)}s)\n", nameFormat },
                };
            }
        }
    }
}