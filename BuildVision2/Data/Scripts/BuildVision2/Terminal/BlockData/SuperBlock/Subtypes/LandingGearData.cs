using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class LandingGearData
        {
            /// <summary>
            /// Returns the status of the landing gear (unlocked/ready/locked).
            /// </summary>
            public LandingGearMode Status => landingGear.LockMode;

            /// <summary>
            /// Toggles the landing gear lock.
            /// </summary>
            public readonly Action ToggleLock;

            private readonly IMyLandingGear landingGear;

            public LandingGearData(IMyTerminalBlock tBlock)
            {
                landingGear = tBlock as IMyLandingGear;
                ToggleLock = landingGear.ToggleLock;
            }
        }
    }
}