using Sandbox.ModAPI;
using System;

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
        }
    }
}