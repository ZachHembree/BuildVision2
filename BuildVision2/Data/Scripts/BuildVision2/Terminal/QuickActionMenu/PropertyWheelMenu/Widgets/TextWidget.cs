using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class TextWidget : BlockValueWidgetBase
        {
            private readonly Label label;
            private readonly TextField textField;

            private IBlockValue<StringBuilder> textValueMember;
            private IBlockTextMember textMember;

            public TextWidget(HudParentBase parent = null) : base(parent)
            {
                label = new Label()
                {
                    Format = GlyphFormat.Blueish.WithSize(1.08f),
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner
                };
                textField = new TextField();

                var layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        label,
                        textField,
                        buttonChain
                    }
                };
            }

            public override void SetData(object member, Action CloseWidgetCallback)
            {
                textValueMember = member as IBlockValue<StringBuilder>;
                textMember = member as IBlockTextMember;
                this.CloseWidgetCallback = CloseWidgetCallback;

                label.TextBoard.SetText(textMember.Name);
                textField.CharFilterFunc = textMember.CharFilterFunc;

                if (BindManager.IsChatOpen)
                    textField.TextBoard.SetText(textValueMember.Value);
                else
                    textField.TextBoard.SetText(textEntryWarning);

                if (BindManager.IsChatOpen && !textField.InputOpen)
                    textField.OpenInput();

                textField.MouseInput.GetInputFocus();
            }

            public override void Reset()
            {
                textValueMember = null;
                textMember = null;
                CloseWidgetCallback = null;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                base.HandleInput(cursorPos);

                if (SharedBinds.Enter.IsNewPressed)
                {
                    if (BindManager.IsChatOpen && !textField.InputOpen)
                    {
                        textField.TextBoard.SetText(textValueMember.Value);
                        textField.OpenInput();
                        textField.MouseInput.GetInputFocus();
                    }
                    else if (!BindManager.IsChatOpen && textField.InputOpen)
                        Confirm();
                }
            }

            protected override void Confirm()
            {
                if (!BindManager.IsChatOpen && textField.InputOpen)
                {
                    textMember.SetValueText(textField.TextBoard.ToString());
                    textField.CloseInput();
                }

                CloseWidgetCallback();
            }

            protected override void Cancel()
            {
                textField.CloseInput();
                CloseWidgetCallback();
            }
        }
    }
}