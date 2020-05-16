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

        public class WarheadAccessor : SubtypeAccessor<IMyWarhead>
        {
            /// <summary>
            /// Controls the warhead detonation countdown in seconds.
            /// </summary>
            public float CountdownTime { get { return subtype.DetonationTime; } set { subtype.DetonationTime = value; } }

            /// <summary>
            /// Controls warhead arming.
            /// </summary>
            public bool IsArmed { get { return subtype.IsArmed; } set { subtype.IsArmed = value; } }

            /// <summary>
            /// Indicates whether or not the warhead is counting down.
            /// </summary>
            public bool IsCountingDown => subtype.IsCountingDown;

            public WarheadAccessor(SuperBlock block) : base(block, TBlockSubtypes.Warhead)
            { }

            /// <summary>
            /// Starts the countdown.
            /// </summary>
            public void StartCountdown() =>
                subtype.StartCountdown();

            /// <summary>
            /// Stops the countdown.
            /// </summary>
            public void StopCountdown() =>
                subtype.StopCountdown();

            /// <summary>
            /// Detonates the warhead.
            /// </summary>
            public void Detonate() =>
                subtype.Detonate();

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
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalControlPanel_TimerDelay)}: ", nameFormat },
                    { $"{Math.Truncate(CountdownTime)}s\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedStatus()} ", valueFormat },
                };
            }
        }
    }
}