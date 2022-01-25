using RichHudFramework.UI;
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

            public override void SetMember(IBlockMember member, Action CloseWidgetCallback)
            {
                textValueMember = member as IBlockValue<StringBuilder>;
                textMember = member as IBlockTextMember;
                this.CloseWidgetCallback = CloseWidgetCallback;

                label.TextBoard.SetText(textMember.Name);
                textField.TextBoard.SetText(textValueMember.Value);
                textField.CharFilterFunc = textMember.CharFilterFunc;
            }

            public override void Reset()
            {
                textValueMember = null;
                textMember = null;
                CloseWidgetCallback = null;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (confirmButton.MouseInput.IsNewLeftClicked)
                {
                    textMember.SetValueText(textField.TextBoard.ToString());
                    CloseWidgetCallback();
                }
                else if (cancelButton.MouseInput.IsNewLeftClicked)
                {
                    CloseWidgetCallback();
                }
            }
        }
    }
}