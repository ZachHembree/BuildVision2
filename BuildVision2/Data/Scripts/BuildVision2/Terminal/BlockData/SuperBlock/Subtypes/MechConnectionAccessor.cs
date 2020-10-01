﻿using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using RichHudFramework.Internal;
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

        public class MechConnectionAccessor : SubtypeAccessor<IMyMechanicalConnectionBlock>
        {
            /// <summary>
            /// Indicates whether or not a head has been attached to the mech block.
            /// </summary>
            public bool PartAttached => subtype.IsAttached;

            public MechConnectionAccessor(SuperBlock block) : base(block, TBlockSubtypes.MechanicalConnection)
            {                
                if (subtype != null && block.TBlock is IMyMotorSuspension)
                    block.SubtypeId |= TBlockSubtypes.Suspension;
            }

            /// <summary>
            /// Attempts to attach a nearby head.
            /// </summary>
            public void AttachHead()
            {
                //subtype.Attach();
                BvMain.SendBlockActionToServer(new BlockActionMsg(subtype.EntityId, EntitySubtypes.MyMechanicalConnection, MechBlockActions.Attach));
            }

            /// <summary>
            /// Detaches the head.
            /// </summary>
            public void DetachHead()
            {
                //subtype.Detach(); -- This isn't being synchronized with the DS, yet Attach() is.
                BvMain.SendBlockActionToServer(new BlockActionMsg(subtype.EntityId, EntitySubtypes.MyMechanicalConnection, MechBlockActions.Detach));
            }

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