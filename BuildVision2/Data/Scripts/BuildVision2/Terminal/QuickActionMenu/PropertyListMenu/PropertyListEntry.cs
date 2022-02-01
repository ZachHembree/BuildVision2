using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class PropertyListEntryElement : HudElementBase, IMinLabelElement
        {
            public ITextBoard TextBoard => name.TextBoard;

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

                ParentAlignment = ParentAlignments.Left;
            }

            protected override void Draw()
            {
                Width = layout.Width;
            }
        }

        private class PropertyListEntry : ListBoxEntry<PropertyListEntryElement, IBlockMember>
        {
            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard TextBoard => Element.TextBoard;

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
            /// Returns/set true if the property has been selected and opened for editing
            /// </summary>
            public bool PropertyOpen { get; set; }

            /// <summary>
            /// Flag used to indicate entries selected for duplication
            /// </summary>
            public bool IsSelectedForCopy { get; set; }

            /// <summary>
            /// Flag used to indicate text entry has been opened, but the property is waiting for chat
            /// </summary>
            public bool WaitingForChatInput { get; set; }

            /// <summary>
            /// Optional data associated with block member
            /// </summary>
            public object MemberData { get; private set; }

            /// <summary>
            /// Returns true if text input for the text box is open
            /// </summary>
            public bool InputOpen => Element.value.InputOpen;

            public PropertyListEntry()
            {
                // Resizing was disabled in the parent constructor, I'm just turning it back on.
                TextBoard.AutoResize = true;
            }

            public void SetMember(IBlockMember member, object data = null)
            {
                AssocMember = member;
                MemberData = data;
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
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public virtual void UpdateText(bool highlight)
            {
                ITextBoard nameTB = Element.name.TextBoard,
                    valueTB = Element.value.TextBoard,
                    postTB = Element.postfix.TextBoard;
                StringBuilder name = AssocMember.Name,
                    disp = AssocMember.FormattedValue,
                    status = AssocMember.StatusText;

                // Update Name
                nameTB.Clear();

                var nameFormat = highlight ? nameTB.Format : bodyText;
                var valueFormat = highlight ? nameTB.Format : valueText;

                if (name != null)
                {
                    nameTB.Append(name, nameFormat);

                    if (disp != null || status != null)
                        nameTB.Append(": ", nameFormat);
                }

                // Update Value
                valueTB.Format = valueFormat;

                if (!Element.value.InputOpen && !WaitingForChatInput)
                {
                    valueTB.Clear();

                    if (disp != null)
                    {
                        var colorMember = AssocMember as IBlockValue<Color>;

                        if (colorMember != null)
                            valueTB.Append(colorMember.Value.GetChannel((int)MemberData).ToString(), valueFormat);
                        else
                            valueTB.Append(disp, valueFormat);
                    }
                }

                // Update Postfix
                postTB.Clear();

                if (status != null)
                {
                    postTB.Append(' ', nameFormat);
                    postTB.Append(status, nameFormat);
                }
            }

            public override void Reset()
            {
                IsSelectedForCopy = false;
                PropertyOpen = false;
                MemberData = null;
                CloseInput();
                base.Reset();
            }
        }
    }
}