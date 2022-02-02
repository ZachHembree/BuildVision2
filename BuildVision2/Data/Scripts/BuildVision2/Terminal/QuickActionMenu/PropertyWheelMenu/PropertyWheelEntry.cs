using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using System.Text;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class PropertyWheelEntryBase : ScrollBoxEntry<Label>
        {
            /// <summary>
            /// Text rendered by the label.
            /// </summary>
            public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard TextBoard => Element.TextBoard;

            public PropertyWheelEntryBase()
            {
                SetElement(new Label()
                {
                    Format = valueFormatCenter,
                    VertCenterText = true,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Padding = new Vector2(20f),
                    Width = 90f
                });
            }

            public virtual void Reset() { }
        }

        /// <summary>
        /// Shortcut entry for selection wheel
        /// </summary>
        private class PropertyWheelShortcutEntry : PropertyWheelEntryBase
        {
            /// <summary>
            /// Delegate invoked when the corresponding shortcut entry is selected
            /// </summary>
            public Action ShortcutAction;

            public PropertyWheelShortcutEntry()
            { }

            public override void Reset()
            {
                ShortcutAction = null;
            }
        }

        /// <summary>
        /// Custom scroll box container for <see cref="IBlockMember"/>
        /// </summary>
        private class PropertyWheelEntry : PropertyWheelEntryBase
        {
            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && BlockMember != null && BlockMember.Enabled; }
            }

            /// <summary>
            /// Returns true if the associated block member is selected for duplication
            /// </summary>
            public bool IsSelectedForDuplication
            {
                get { return duplicator.PropertyDupeEntries[MemberIndex].isSelectedForDuplication; }
                set { duplicator.SetMemberSelection(MemberIndex, value); }
            }

            /// <summary>
            /// Returns true if the associated block member can be duplicated
            /// </summary>
            public bool CanDuplicate => duplicator.PropertyDupeEntries[MemberIndex].canDuplicate;

            /// <summary>
            /// Block member index
            /// </summary>
            public int MemberIndex { get; private set; }

            /// <summary>
            /// Associated block member
            /// </summary>
            public IBlockMember BlockMember { get; private set; }

            private readonly RichText textBuf;
            private BlockPropertyDuplicator duplicator;

            public PropertyWheelEntry()
            {
                textBuf = new RichText();
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(int index, BlockPropertyDuplicator duplicator)
            {
                MemberIndex = index;
                this.duplicator = duplicator;
                BlockMember = duplicator.BlockMembers[MemberIndex];
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
                    textBuf.Add(name, bodyFormatCenter);

                    if (disp != null || status != null)
                        textBuf.Add(":\n", bodyFormatCenter);
                }

                if (disp != null)
                {
                    textBuf.Add(' ', valueFormatCenter);
                    textBuf.Add(disp, valueFormatCenter);
                }

                Element.Text = textBuf;
            }

            public override void Reset()
            {
                MemberIndex = -1;
                BlockMember = null;
                duplicator = null;
            }
        }
    }
}