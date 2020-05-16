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
        public GeneralAccessor General { get; private set; }

        public class GeneralAccessor : SubtypeAccessorBase
        {
            public string CustomName { get { return block.TBlock.CustomName; } set { block.TBlock.CustomName = value; } }

            private readonly string groupString;

            public GeneralAccessor(SuperBlock block) : base(block)
            {
                block.SubtypeId |= SubtypeId;
                block.subtypeAccessors.Add(this);
                groupString = GetGroupString();
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText
                {
                    { $"{MyTexts.TrySubstitute("Name")}: ", nameFormat },
                    { $"{CustomName}\n", valueFormat },
                };

                if (groupString.Length > 0)
                {
                    summary.Add(new RichText 
                    {
                        { $"{MyTexts.TrySubstitute("Groups")}: ", nameFormat },
                        { $"{groupString}\n", valueFormat },
                    });
                }

                return summary;
            }

            private string GetGroupString()
            {
                var groupString = new StringBuilder();
                var groupNames = new List<string>();
                block.GetGroupNamesForBlock(groupNames);

                for(int n = 0; n < groupNames.Count; n++)
                {
                    if (n > 0)
                        groupString.Append($", {groupNames[n]}");
                    else
                        groupString.Append(groupNames[n]);
                }

                return groupString.ToString();
            }
        }
    }
}