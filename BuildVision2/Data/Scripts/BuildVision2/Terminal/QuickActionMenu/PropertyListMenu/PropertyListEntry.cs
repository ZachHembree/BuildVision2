using RichHudFramework.UI;
using System.Text;
using RichHudFramework.UI.Rendering;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private sealed class PropertyListEntry : ListBoxEntry<IBlockMember>
        {
            /// <summary>
            /// Text rendered by the label.
            /// </summary>
            public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard TextBoard => Element.TextBoard;

            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && AssocMember != null && AssocMember.Enabled; }
            }

            /// <summary>
            /// Flag used to indicate entries selected for duplication
            /// </summary>
            public bool IsSelectedForCopy { get; set; }

            private readonly RichText textBuf;

            public PropertyListEntry()
            {
                textBuf = new RichText();
                Element.ParentAlignment = ParentAlignments.Left;
                Element.VertCenterText = true;
                Element.BuilderMode = TextBuilderModes.Unlined;
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(IBlockMember blockMember)
            {
                AssocMember = blockMember;
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public void UpdateText()
            {
                StringBuilder name = AssocMember.Name,
                    disp = AssocMember.FormattedValue,
                    status = AssocMember.StatusText;

                textBuf.Clear();

                if (name != null)
                {
                    textBuf.Add(name, bodyText);

                    if (disp != null || status != null)
                        textBuf.Add(":", bodyText);
                }

                if (disp != null)
                {
                    textBuf.Add(' ', valueText);
                    textBuf.Add(disp, valueText);
                }

                Element.Text = textBuf;
            }

            public override void Reset()
            {
                IsSelectedForCopy = false;

                base.Reset();
            }
        }
    }
}