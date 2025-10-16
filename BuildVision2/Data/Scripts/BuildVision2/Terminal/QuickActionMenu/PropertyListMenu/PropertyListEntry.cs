using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework;
using System.Text;
using System;
using VRageMath;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        /// <summary>
        /// UI element attached to list entry
        /// </summary>
        private class PropertyListEntryElement : HudElementBase, IMinLabelElement
        {
            public ITextBoard TextBoard => value.TextBoard;

            public Vector2 TextSize { get; private set; }

            public readonly Label name, postfix;
            public readonly TextBox value;
            private readonly HudChain layout;

            public PropertyListEntryElement() : base(null)
            {
                name = new Label() { Text = "", AutoResize = false };
                postfix = new Label() { Text = "", AutoResize = false };
                value = new TextBox()
                {
                    Text = "",
                    UseCursor = false,
                    AutoResize = false
                };
                
                layout = new HudChain(false, this)
                {
                    ParentAlignment = ParentAlignments.PaddedInnerLeft,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.AlignMembersStart,
                    CollectionContainer = { name, value, postfix }
                };

                Height = listEntryHeight;
                IsMasking = true;
            }

            protected override void Layout()
            {
                Vector2 nsize = name.TextBoard.TextSize,
                    psize = postfix.TextBoard.TextSize,
                    vsize = value.TextBoard.TextSize;

                TextSize = nsize + psize + vsize;

                name.UnpaddedSize = nsize;
                postfix.UnpaddedSize = psize;
                value.UnpaddedSize = vsize;
                layout.Size = TextSize;

                TextSize += Padding;
            }
        }

        private class PropertyListEntry : ScrollBoxEntryTuple<PropertyListEntryElement, IBlockMember>
        {
            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard NameText => Element.name.TextBoard;

            /// <summary>
            /// TextBoard backing the value element.
            /// </summary>
            public ITextBoard ValueText => Element.value.TextBoard;

            /// <summary>
            /// TextBoard backing the postfix element.
            /// </summary>
            public ITextBoard PostText => Element.postfix.TextBoard;

            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && (AssocMember != null && AssocMember.Enabled); }
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
            /// Returns/set true if the property has been selected and opened for editing
            /// </summary>
            public bool PropertyOpen { get; set; }

            /// <summary>
            /// Flag used to indicate text entry has been opened, but the property is waiting for chat
            /// </summary>
            public bool WaitingForChatInput { get; set; }

            /// <summary>
            /// Returns true if text input for the text box is open
            /// </summary>
            public bool InputOpen => Element.value.InputOpen;

            private IPropertyBlock target;
            private readonly StringBuilder textBuf;

            public PropertyListEntry()
            {
                SetElement(new PropertyListEntryElement());
                NameText.Format = bodyFormat;
                textBuf = new StringBuilder();
            }

            public void SetMember(IPropertyBlock target, IBlockMember member, int index)
            {
                this.target = target;
                MemberIndex = index;
                AssocMember = member;
            }

            public void Reset()
            {
                WaitingForChatInput = false;
                PropertyOpen = false;
                target = null;
                MemberIndex = -1;
                Enabled = true;
                AssocMember = null;
                NameText.Clear();
                ValueText.Clear();
                PostText.Clear();

                CloseInput();
            }

            /// <summary>
            /// Opens input for value text
            /// </summary>
            public void OpenInput()
            {
                var textMember = AssocMember as IBlockTextMember;
                Element.value.CharFilterFunc = textMember.CharFilterFunc;
                Element.value.OpenInput();
            }

            /// <summary>
            /// Closes input for value text
            /// </summary>
            public void CloseInput()
            {
                Element.value.CloseInput();
            }

            /// <summary>
            /// Sets value based on parsed text value, if applicable
            /// </summary>
            public void SetValueText(string text)
            {
                var textMember = AssocMember as IBlockTextMember;
                textMember?.SetValueText(text);
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public virtual void UpdateText(bool isHighlighted, bool isDuplicating)
            {
                ITextBoard nameTB = Element.name.TextBoard,
                    valueTB = Element.value.TextBoard,
                    postTB = Element.postfix.TextBoard;
                IBlockMember blockMember = AssocMember;
                StringBuilder name = blockMember.Name,
                    disp = blockMember.FormattedValue,
                    status = blockMember.StatusText;

                GlyphFormat dupeCrossFormat, bodyFormat, valueFormat;

                if (PropertyOpen)
                {
                    dupeCrossFormat = QuickActionMenu.selectedFormat;
                    bodyFormat = QuickActionMenu.selectedFormat;
                    valueFormat = QuickActionMenu.selectedFormat;
                }
                else
                {
                    dupeCrossFormat = QuickActionMenu.dupeCrossFormat;
                    bodyFormat = QuickActionMenu.bodyFormat;
                    valueFormat = isHighlighted ? QuickActionMenu.highlightFormat : QuickActionMenu.valueFormat;
                }

                // Update Name
                nameTB.Clear();

                if (isDuplicating && IsSelectedForDuplication)
                    nameTB.Append("+ ", dupeCrossFormat);

                if (name != null)
                {
                    textBuf.Clear();
                    textBuf.AppendSubstringMax(name, maxEntryCharCount);
                    nameTB.Append(textBuf, bodyFormat);

                    if (disp != null || status != null)
                        nameTB.Append(": ", bodyFormat);
                }

                // Update Value
                if (!Element.value.InputOpen && !WaitingForChatInput)
                {
                    valueTB.Clear();

                    if (disp != null)
                    {
                        var colorMember = AssocMember as IBlockNumericValue<Color>;
                        valueTB.Append(disp, valueFormat);
                    }
                }

                // Update Postfix
                postTB.Clear();

                if (status != null)
                {
                    postTB.Append(' ', bodyFormat);
                    postTB.Append(status, bodyFormat);
                }
            }
        }
    }
}