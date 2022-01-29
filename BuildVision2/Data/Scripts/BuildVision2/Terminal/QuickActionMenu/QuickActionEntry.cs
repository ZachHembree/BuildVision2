using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using System.Text;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class QuickActionEntryBase : ScrollBoxEntry<Label>
        {
            /// <summary>
            /// Text rendered by the label.
            /// </summary>
            public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard TextBoard => Element.TextBoard;

            public QuickActionEntryBase()
            {
                SetElement(new Label()
                {
                    Format = valueTextCenter,
                    VertCenterText = true,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Padding = new Vector2(20f),
                    Width = 90f
                });
            }

            public virtual void Reset() { }
        }

        private class QuickActionShortcutEntry : QuickActionEntryBase
        {
            public Action ShortcutAction;

            public QuickActionShortcutEntry()
            { }

            public override void Reset()
            {
                ShortcutAction = null;
            }
        }

        /// <summary>
        /// Custom scroll box container for <see cref="IBlockMember"/>
        /// </summary>
        private class QuickBlockPropertyEntry : QuickActionEntryBase
        {
            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && BlockMember != null && BlockMember.Enabled; }
            }

            /// <summary>
            /// Flag used to indicate entries selected for duplication
            /// </summary>
            public bool IsSelectedForCopy { get; set; }

            /// <summary>
            /// Returns associated property member wrapper
            /// </summary>
            public IBlockMember BlockMember { get; private set; }

            private readonly RichText textBuf;

            public QuickBlockPropertyEntry()
            {
                textBuf = new RichText();
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(IBlockMember blockMember)
            {
                BlockMember = blockMember;
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public void UpdateText()
            {
                StringBuilder name = BlockMember.Name,
                    disp = BlockMember.FormattedValue,
                    status = BlockMember.StatusText;

                textBuf.Clear();

                if (name != null)
                {
                    textBuf.Add(name, bodyTextCenter);

                    if (disp != null || status != null)
                        textBuf.Add(":\n", bodyTextCenter);
                }

                if (disp != null)
                {
                    textBuf.Add(' ', valueTextCenter);
                    textBuf.Add(disp, valueTextCenter);
                }

                Element.Text = textBuf;
            }

            public override void Reset()
            {
                IsSelectedForCopy = false;
                BlockMember = null;
            }
        }
    }
}