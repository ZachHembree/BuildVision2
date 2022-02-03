using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework;
using System.Text;
using VRageMath;

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

            public readonly Label name, postfix;
            public readonly TextBox value;
            private readonly HudChain layout;

            public PropertyListEntryElement() : base(null)
            {
                name = new Label();
                postfix = new Label();
                value = new TextBox()
                {
                    UseCursor = false,
                };

                layout = new HudChain(false, this)
                {
                    Padding = new Vector2(18f, 0f),
                    SizingMode = HudChainSizingModes.FitChainBoth,
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                    CollectionContainer = { name, value, postfix }
                };

                Height = 19f;
                ParentAlignment = ParentAlignments.Left;
            }

            protected override void Draw()
            {
                Width = layout.Width;
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
                get { return base.Enabled && AssocMember != null && AssocMember.Enabled; }
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

            private PropertyBlock target;

            public PropertyListEntry()
            {
                SetElement(new PropertyListEntryElement());
                NameText.Format = bodyFormat;
            }

            public void SetMember(int index, PropertyBlock target)
            {
                this.target = target;
                MemberIndex = index;
                AssocMember = target.BlockMembers[index];
            }

            public void Reset()
            {
                WaitingForChatInput = false;
                PropertyOpen = false;
                target = null;
                MemberIndex = -1;
                Enabled = true;
                AssocMember = null;

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
            public virtual void UpdateText(bool isDuplicating)
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
                    valueFormat = QuickActionMenu.valueFormat;
                }

                // Update Name
                nameTB.Clear();

                if (isDuplicating && IsSelectedForDuplication)
                    nameTB.Append("+ ", dupeCrossFormat);

                if (name != null)
                {
                    nameTB.Append(name, bodyFormat);

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