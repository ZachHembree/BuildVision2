using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;
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

                layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
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
                CloseWidgetCallback = null;

                Confirm();
                textValueMember = null;
                textMember = null;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                base.HandleInput(cursorPos);

                if (BindManager.IsChatOpen && !textField.InputOpen 
                    && MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("CHAT_SCREEN")) )
                {
                    textField.TextBoard.SetText(textValueMember.Value);
                    textField.OpenInput();
                    textField.MouseInput.GetInputFocus();
                }
                if (!BindManager.IsChatOpen && textField.InputOpen && SharedBinds.Enter.IsNewPressed)
                {
                    Confirm();
                }
            }

            protected override void Confirm()
            {
                if (textField.InputOpen)
                    textMember.SetValueText(textField.TextBoard.ToString());

                textField.CloseInput();
                CloseWidgetCallback?.Invoke();
            }

            protected override void Cancel()
            {
                textField.TextBoard.SetText(textValueMember.Value);
                Confirm();
            }
        }
    }
}