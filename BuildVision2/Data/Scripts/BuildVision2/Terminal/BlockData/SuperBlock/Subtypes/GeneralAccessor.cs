using RichHudFramework.UI;
using Sandbox.ModAPI;
using System.Text;
using System.Collections.Generic;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        private GeneralAccessor _general;

        public GeneralAccessor General
        {
            get
            {
                return _general;
            }
            private set
            {
                _general = value;
            }
        }

        public class GeneralAccessor : SubtypeAccessorBase
        {
            public string CustomName { get { return block.TBlock.CustomName; } set { block.TBlock.CustomName = value; } }

            private readonly StringBuilder groupString;
            private readonly List<string> groupNames;

            public GeneralAccessor()
            {
                groupString = new StringBuilder();
                groupNames = new List<string>();
            }

            public override void SetBlock(SuperBlock block)
            {
                this.block = block;
                block.SubtypeId |= SubtypeId;
                block.subtypeAccessors.Add(this);

                GetGroupString();
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add(MyTexts.TrySubstitute("Name"), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(CustomName, valueFormat);
                builder.Add("\n", valueFormat);

                if (groupString.Length > 0)
                {
                    builder.Add(MyTexts.TrySubstitute("Groups"), nameFormat);
                    builder.Add(": ", nameFormat);

                    builder.Add(groupString, valueFormat);
                    builder.Add('\n');
                }
            }

            private void GetGroupString()
            {
                groupString.Clear();
                groupNames.Clear();
                block.GetGroupNamesForBlock(groupNames);

                for (int n = 0; n < groupNames.Count; n++)
                {
                    if (n > 0)
                        groupString.Append($", {groupNames[n]}");
                    else
                        groupString.Append(groupNames[n]);
                }
            }
        }
    }
}