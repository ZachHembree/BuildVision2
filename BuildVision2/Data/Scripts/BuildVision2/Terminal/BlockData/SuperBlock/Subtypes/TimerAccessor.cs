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

        public class TimerAccessor : SubtypeAccessor<IMyTimerBlock>
        {
            public float Delay { get { return subtype.TriggerDelay; } set { subtype.TriggerDelay = value; } }

            public bool IsCountingDown => subtype.IsCountingDown;

            public bool Silent { get { return subtype.Silent; } set { subtype.Silent = value; } }

            public TimerAccessor(SuperBlock block) : base(block, TBlockSubtypes.Timer)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalControlPanel_TimerDelay)}: ", nameFormat);
                builder.Add($"{Math.Truncate(Delay)}s\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Silent)}: ", nameFormat);
                builder.Add($"{MyTexts.TrySubstitute(Silent.ToString())}\n", valueFormat);
            }

            public void StartCountdown() =>
                subtype.StartCountdown();

            public void StopCountdown() =>
                subtype.StopCountdown();

            public void Trigger() =>
                subtype.Trigger();
        }
    }
}