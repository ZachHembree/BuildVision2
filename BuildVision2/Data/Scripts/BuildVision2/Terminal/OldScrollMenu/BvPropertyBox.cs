using RichHudFramework.UI;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvScrollMenu
    {
        private class BvPropertyBox : HudElementBase
        {
            /// <summary>
            /// Indicates whether or not the property is currently being copied.
            /// </summary>
            public bool Copying { get { return _copying; } set { _copying = value && (_blockMember is IBlockProperty); } }

            /// <summary>
            /// Gets/sets the block member associated with the property block
            /// </summary>
            public IBlockMember BlockMember
            {
                get { return _blockMember; }
                set
                {
                    _blockMember = value;
                    Copying = false;

                    if (value != null)
                    {
                        var textMember = _blockMember as IBlockTextMember;

                        if (textMember != null)
                            this.valueBox.CharFilterFunc = textMember.CharFilterFunc;
                        else
                            this.valueBox.CharFilterFunc = null;

                        var nameBuilder = _blockMember.Name;
                        name.Format = bodyText;
                        Name.Clear();

                        if (nameBuilder != null && nameBuilder.Length > 0)
                        {
                            Name.Add(nameBuilder);
                            Name.Add(": ");
                        }
                    }
                }
            }

            public RichText Name { get; }

            public RichText Value { get; private set; }

            public RichText Postfix { get; }

            public bool InputOpen => valueBox.InputOpen;

            private readonly Label name, postfix;
            private readonly TextBox valueBox;
            private readonly SelectionBox copyIndicator;
            private readonly HudChain layout;
            private IBlockMember _blockMember;
            private bool _copying;

            public BvPropertyBox() : base(null)
            {
                ParentAlignment = ParentAlignments.Left;

                copyIndicator = new SelectionBox();
                name = new Label();
                valueBox = new TextBox() { UseCursor = false };
                postfix = new Label();

                layout = new HudChain(false, this)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                    CollectionContainer = { copyIndicator, name, valueBox, postfix }
                };

                Name = new RichText();
                Value = new RichText();
                Postfix = new RichText();
            }

            public void OpenInput()
            {
                valueBox.Text = Value;
                valueBox.Format = Value.defaultFormat.Value;
                valueBox.OpenInput();
            }

            public void CloseInput()
            {
                valueBox.CloseInput();
                Value = valueBox.Text;
            }

            /// <summary>
            /// Clears property information from the property box
            /// </summary>
            public void Reset()
            {
                name.TextBoard.Clear();
                postfix.TextBoard.Clear();
                valueBox.CloseInput();
                BlockMember = null;
            }

            public void SetValueText(string value, GlyphFormat? format = null)
            {
                valueBox.TextBoard.SetText(value, format);
            }

            protected override void Layout()
            {
                Size = layout.Size;
                copyIndicator.Visible = Copying;
            }

            /// <summary>
            /// Updates property box text
            /// </summary>
            public void UpdateText(bool highlighted, bool selected)
            {
                postfix.Format = bodyText;

                if (highlighted)
                {
                    if (selected)
                        Value.defaultFormat = selectedText;
                    else
                        Value.defaultFormat = highlightText;
                }
                else
                    Value.defaultFormat = valueText;

                Value.Clear();
                Postfix.Clear();

                StringBuilder disp = _blockMember.FormattedValue,
                    status = _blockMember.StatusText;

                if (disp != null)
                    Value.Add(disp);

                if (status != null)
                {
                    Postfix.Add(" ");
                    Postfix.Add(status);
                }

                name.TextBoard.SetText(Name);
                valueBox.TextBoard.SetText(Value);
                postfix.TextBoard.SetText(Postfix);
            }

            private class SelectionBox : Label
            {
                public SelectionBox(HudParentBase parent = null) : base(parent)
                {
                    AutoResize = true;
                    VertCenterText = true;
                    Visible = false;

                    Padding = new Vector2(4f, 0f);
                    Format = selectedText.WithAlignment(TextAlignment.Center);
                    Text = "+";
                }
            }
        }
    }
}