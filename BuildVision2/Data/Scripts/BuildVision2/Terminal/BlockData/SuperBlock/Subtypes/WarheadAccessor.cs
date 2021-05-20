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
        public WarheadAccessor Warhead  { get { return _warhead; } private set { _warhead = value; } }

        private WarheadAccessor _warhead;

        public class WarheadAccessor : SubtypeAccessor<IMyWarhead>
        {
            /// <summary>
            /// Controls the warhead detonation countdown in seconds.
            /// </summary>
            public float CountdownTime
            {
                get
                {
                    BvServer.SendEntityActionToServer
                    (
                        ServerBlockActions.Warhead | ServerBlockActions.GetTime,
                        subtype.EntityId,
                        DetonationTimeCallback
                    );

                    return _countdownTime;
                }
            }

            /// <summary>
            /// Controls warhead arming.
            /// </summary>
            public bool IsArmed { get { return subtype.IsArmed; } set { subtype.IsArmed = value; } }

            /// <summary>
            /// Indicates whether or not the warhead is counting down.
            /// </summary>
            public bool IsCountingDown => subtype.IsCountingDown;

            private float _countdownTime;
            private readonly Action<byte[]> DetonationTimeCallback;

            public WarheadAccessor()
            {
                DetonationTimeCallback = UpdateDetonationTime;
            }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Warhead);
            }

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

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalControlPanel_TimerDelay), nameFormat);
                builder.Add(": ", nameFormat);

                buf.Clear();
                buf.Append(Math.Truncate(CountdownTime));
                buf.Append("s\n");
                builder.Add(buf, valueFormat);

                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedStatus(), valueFormat);
            }

            private void UpdateDetonationTime(byte[] bin)
            {
                Utils.ProtoBuf.TryDeserialize(bin, out _countdownTime);
            }
        }
    }
}