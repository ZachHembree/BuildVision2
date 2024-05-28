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
                    AutoResize = false,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Padding = new Vector2(20f),
                    Width = 90f,
                    Height = 100f
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
                get { return base.Enabled && target.Prioritizer.GetIsMemberEnabledAndPrioritized(MemberIndex); }
            }

            /// <summary>
            /// Returns true if the associated block member is selected for duplication
            /// </summary>
            public bool IsSelectedForDuplication
            {
                get { return target.Duplicator.DupeEntries[MemberIndex].isSelectedForDuplication; }
                set { target.Duplicator.SetMemberSelection(MemberIndex, value); }
            }

            /// <summary>
            /// Returns true if the associated block member can be duplicated
            /// </summary>
            public bool CanDuplicate => target.Duplicator.DupeEntries[MemberIndex].canDuplicate;

            /// <summary>
            /// Block member index
            /// </summary>
            public int MemberIndex { get; private set; }

            /// <summary>
            /// Associated block member
            /// </summary>
            public IBlockMember BlockMember { get; private set; }

            private readonly StringBuilder textBuf;
            private IPropertyBlock target;

            public PropertyWheelEntry()
            {
                textBuf = new StringBuilder();
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(int index, IPropertyBlock target)
            {
                MemberIndex = index;
                this.target = target;
                BlockMember = target.BlockMembers[MemberIndex];
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public void UpdateText(bool selected)
            {
                StringBuilder name = BlockMember.Name,
                    disp = BlockMember.FormattedValue;
                ITextBuilder tb = Element.TextBoard;

                var fmtName = selected ? selectedFormatCenter : nameFormatCenter;
                var fmtValue = selected ? selectedFormatCenter : valueFormatCenter;

                tb.Clear();

                if (name != null)
                {
                    textBuf.Clear();
                    textBuf.AppendSubstringMax(name, maxEntryCharCount);

                    if (disp != null)
                        textBuf.Append(":\n");

                    tb.Append(textBuf, fmtName);
                }

                if (disp != null)
                {
                    textBuf.Clear();
                    textBuf.AppendSubstringMax(disp, maxEntryCharCount);

                    tb.Append(textBuf, fmtValue);
                }
            }

            public override void Reset()
            {
                MemberIndex = -1;
                BlockMember = null;
                target = null;
            }
        }
    }
}