using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProgramBlockAccessor Program { get; private set; }

        public class ProgramBlockAccessor : SubtypeAccessor<IMyProgrammableBlock>
        { 
            public string Program { get { return subtype.ProgramData; } set { subtype.ProgramData = value; } }

            public string Argument => subtype.TerminalRunArgument;

            public ProgramBlockAccessor(SuperBlock block) : base(block, TBlockSubtypes.Programmable)
            { }

            public void Run() =>
                subtype.Run();

            public void Run(string arguments) =>
                subtype.Run(arguments);

            public void Recompile() =>
                subtype.Recompile();

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalControlPanel_RunArgument)}: ", nameFormat },
                    { $"{Argument ?? ""}\n", valueFormat },
                };
            }
        }
    }
}