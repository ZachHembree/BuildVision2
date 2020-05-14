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

            public GeneralAccessor(SuperBlock block) : base(block, TBlockSubtypes.None)
            {
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
                var groups = new List<IMyBlockGroup>();
                block.GetGroupsForBlock(groups);

                // Im getting duplicates for some reason
                var groupNames = new List<string>(groups.Count);

                for (int n = 0; n < groups.Count; n++)
                {
                    if (!groupNames.Contains(groups[n].Name))
                        groupNames.Add(groups[n].Name);
                }

                int count = 0;

                foreach (string name in groupNames)
                {
                    if (count > 0)
                        groupString.Append($", {name}");
                    else
                        groupString.Append(name);

                    count++;
                }

                return groupString.ToString();
            }
        }
    }
}