using Sandbox.ModAPI;
using System;
using VRageMath;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class WarheadData
        {
            /// <summary>
            /// Returns the warhead detonation countdown in seconds.
            /// </summary>
            public float CountdownTime => warhead.DetonationTime;

            public bool IsArmed => warhead.IsArmed;

            public bool IsCountingDown => warhead.IsCountingDown;

            /// <summary>
            /// Starts warhead countdown
            /// </summary>
            public readonly Func<bool> StartCountdown;

            /// <summary>
            /// Stops warhead countdown
            /// </summary>
            public readonly Func<bool> StopCountdown;

            /// <summary>
            /// Detonates the warhead.
            /// </summary>
            public readonly Action Detonate;

            private readonly IMyWarhead warhead;

            public WarheadData(IMyTerminalBlock tBlock)
            {
                warhead = tBlock as IMyWarhead;
                StartCountdown = warhead.StartCountdown;
                StopCountdown = warhead.StopCountdown;
                Detonate = warhead.Detonate;
            }

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
        }
    }
}