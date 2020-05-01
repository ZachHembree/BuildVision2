using RichHudFramework.UI;
using System;
using VRage;
using IMyTimerBlock = SpaceEngineers.Game.ModAPI.IMyTimerBlock;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public TimerAccessor Timer { get; private set; }

        public class TimerAccessor : SubtypeAccessorBase
        {
            public float Delay { get { return timer.TriggerDelay; } set { timer.TriggerDelay = value; } }

            public bool IsCountingDown => timer.IsCountingDown;

            public bool Silent { get { return timer.Silent; } set { timer.Silent = value; } }

            private readonly IMyTimerBlock timer;

            public TimerAccessor(SuperBlock block) : base(block, TBlockSubtypes.Timer)
            {
                timer = block.TBlock as IMyTimerBlock;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalControlPanel_TimerDelay)}: ", nameFormat },
                    { $"{Math.Truncate(Delay)}s\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Silent)}: ", nameFormat },
                    { $"{MyTexts.TrySubstitute(Silent.ToString())}\n", valueFormat },
                };
            }

            public void StartCountdown() =>
                timer.StartCountdown();

            public void StopCountdown() =>
                timer.StopCountdown();

            public void Trigger() =>
                timer.Trigger();
        }
    }
}