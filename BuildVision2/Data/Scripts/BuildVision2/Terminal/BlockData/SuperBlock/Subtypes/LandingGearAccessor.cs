using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using VRage;
using RichHudFramework.UI;
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

        public class LandingGearAccessor : SubtypeAccessor<IMyLandingGear>
        {
            /// <summary>
            /// Returns the status of the landing gear (unlocked/ready/locked).
            /// </summary>
            public LandingGearMode Status => subtype.LockMode;

            public bool AutoLock { get { return subtype.AutoLock; } set { subtype.AutoLock = value; } }

            public LandingGearAccessor(SuperBlock block) : base(block, TBlockSubtypes.LandingGear)
            { }

            /// <summary>
            /// Toggles the landing gear's lock.
            /// </summary>
            public void ToggleLock() =>
                subtype.ToggleLock();

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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedStatus()}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LandGearAutoLock)}: ", nameFormat },
                    { $"{(AutoLock ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat },
                };
            }
        }
    }
}