using Sandbox.ModAPI;
using System;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to mech block members, if defined.
        /// </summary>
        public MechConnectionAccessor MechConnection { get; private set; }

        public class MechConnectionAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Indicates whether or not a head has been attached to the mech block.
            /// </summary>
            public bool PartAttached => mechBlock.IsAttached;

            private readonly IMyMechanicalConnectionBlock mechBlock;

            public MechConnectionAccessor(SuperBlock block) : base(block, TBlockSubtypes.MechanicalConnection)
            {
                mechBlock = block.TBlock as IMyMechanicalConnectionBlock;
                
                if (block.TBlock is IMyMotorSuspension)
                    block.SubtypeId |= TBlockSubtypes.Suspension;
            }

            /// <summary>
            /// Attempts to attach a nearby head.
            /// </summary>
            public void AttachHead() =>
                mechBlock.Attach();

            /// <summary>
            /// Detaches the head.
            /// </summary>
            public void DetachHead() =>
                mechBlock.Detach();

            /// <summary>
            /// Returns head attachment status as a localized string.
            /// </summary>
            public string GetLocalizedAttachStatus()
            {
                if (PartAttached)
                    return MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorAttached);
                else
                    return MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorDetached);
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.TrySubstitute(block.SubtypeId.UsesSubtype(TBlockSubtypes.Suspension) ? "Wheel" : "Head")}: ", nameFormat },
                    { $"{GetLocalizedAttachStatus()}\n", valueFormat },
                };
            }
        }
    }
}