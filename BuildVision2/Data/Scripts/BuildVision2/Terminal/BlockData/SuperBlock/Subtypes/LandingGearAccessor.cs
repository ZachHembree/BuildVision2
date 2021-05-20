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
        public LandingGearAccessor LandingGear  { get { return _landingGear; } private set { _landingGear = value; } }

        private LandingGearAccessor _landingGear;

        public class LandingGearAccessor : SubtypeAccessor<IMyLandingGear>
        {
            /// <summary>
            /// Returns the status of the landing gear (unlocked/ready/locked).
            /// </summary>
            public LandingGearMode Status => subtype.LockMode;

            public bool AutoLock { get { return subtype.AutoLock; } set { subtype.AutoLock = value; } }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.LandingGear);
            }

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

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                // Lock status
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedStatus(), valueFormat);
                builder.Add("\n", valueFormat);

                // Auto lock
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LandGearAutoLock), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(AutoLock ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff), valueFormat);
                builder.Add("\n", valueFormat);
            }
        }
    }
}