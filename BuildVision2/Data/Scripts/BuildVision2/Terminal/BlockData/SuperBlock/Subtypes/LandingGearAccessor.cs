using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to landing gear members, if defined.
        /// </summary>
        public LandingGearAccessor LandingGear { get; private set; }

        public class LandingGearAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the status of the landing gear (unlocked/ready/locked).
            /// </summary>
            public LandingGearMode Status => landingGear.LockMode;

            private readonly IMyLandingGear landingGear;

            public LandingGearAccessor(SuperBlock block) : base(block, TBlockSubtypes.LandingGear)
            {
                landingGear = block.TBlock as IMyLandingGear;
            }

            /// <summary>
            /// Toggles the landing gear's lock.
            /// </summary>
            public void ToggleLock() =>
                landingGear.ToggleLock();

            /// <summary>
            /// Returns localized string representing the landing gear status.
            /// </summary>
            public string GetLocalizedStatus()
            {
                switch (Status)
                {
                    case LandingGearMode.Unlocked:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_Unlocked);
                    case LandingGearMode.ReadyToLock:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_ReadyToLock);
                    case LandingGearMode.Locked:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_Locked);
                    default:
                        return null;
                }
            }
        }
    }
}