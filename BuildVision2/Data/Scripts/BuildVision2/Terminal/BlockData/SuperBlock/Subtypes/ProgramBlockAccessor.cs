﻿using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProgramBlockAccessor Program { get; private set; }

        public class ProgramBlockAccessor : SubtypeAccessorBase
        { 
            public string Program { get { return programmableBlock.ProgramData; } set { programmableBlock.ProgramData = value; } }

            public bool HasCompileErrors => programmableBlock.HasCompileErrors;

            public string Argument => programmableBlock.TerminalRunArgument;


            private readonly IMyProgrammableBlock programmableBlock;

            public ProgramBlockAccessor(SuperBlock block) : base(block, TBlockSubtypes.Programmable)
            {
                programmableBlock = block.TBlock as IMyProgrammableBlock;
            }

            public void Run() =>
                programmableBlock.Run();

            public void Run(string arguments) =>
                programmableBlock.Run(arguments);

            public void Recompile() =>
                programmableBlock.Recompile();

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalControlPanel_RunArgument)}: ", nameFormat },
                    { $"{Argument ?? ""}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat},
                    { $"{GetLocalizedStatus()}\n", valueFormat },
                };
            }

            public string GetLocalizedStatus()
            {
                if (HasCompileErrors)
                    return MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_CompilationFailed);
                else if (Program != null && Program.Length > 0)
                    return MyTexts.GetString(MySpaceTexts.ProgrammableBlock_Editor_CompilationOk).TrimEnd('.');
                else
                    return MyTexts.TrySubstitute("N/A");
            }
        }
    }
}