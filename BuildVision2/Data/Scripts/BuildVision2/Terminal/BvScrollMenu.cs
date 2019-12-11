using DarkHelmet.UI;
using DarkHelmet.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    internal class BvScrollMenu : HudElementBase
    {
        public override float Width { get { return main.Width; } set { main.Width = value; } }
        public override float Height { get { return main.Height; } set { main.Height = value; } }

        public float BgOpacity
        {
            get { return bgOpacity; }
            set
            {
                header.Color = header.Color.SetAlpha((byte)(255f * bgOpacity));
                body.Color = body.Color.SetAlpha((byte)(255f * bgOpacity * .887f));
                footer.Color = footer.Color.SetAlpha((byte)(255f * bgOpacity));
                bgOpacity = value;
            }
        }

        public int MaxVisible { get; set; }
        public int Count { get; private set; }
        public bool PropOpen { get; private set; }
        private BvPropertyBox Selection => (index < body.ChainElements.Count) ? body.ChainElements[index] : null;

        public readonly LabelBox header;
        public readonly DoubleLabelBox footer;
        public readonly TexturedBox selectionBox, tab;

        private readonly ScrollBox<BvPropertyBox> body;
        private readonly HudChain<HudElementBase> main;
        private readonly Utils.Stopwatch listWrapTimer;

        private int index;
        private float bgOpacity;
        private PropertyBlock target;

        private static readonly Color headerColor, bodyColor, selectionBoxColor;
        private static readonly GlyphFormat headerText, bodyText, 
            footerTextLeft, footerTextRight,
            highlightText, selectedText, blockIncText;

        static BvScrollMenu()
        {
            headerColor = new Color(41, 54, 62);
            bodyColor = new Color(70, 78, 86);
            selectionBoxColor = new Color(41, 54, 62);

            headerText = new GlyphFormat(new Color(210, 235, 245), TextAlignment.Center, .9735f);

            bodyText = new GlyphFormat(new Color(210, 235, 245), textSize: .885f);
            highlightText = new GlyphFormat(new Color(220, 190, 20), textSize: .885f);
            selectedText = new GlyphFormat(new Color(50, 200, 50), textSize: .885f);

            footerTextLeft = bodyText;
            footerTextRight = bodyText.WithAlignment(TextAlignment.Right);
            blockIncText = footerTextRight.WithColor(new Color(200, 35, 35));
        }

        public BvScrollMenu() : base(HudMain.Root)
        {
            CaptureCursor = true;
            ShareCursor = false; // temporary

            header = new LabelBox("Build Vision", headerText)
            {
                AutoResize = false,
                Height = 34f,
                Color = headerColor,
            };

            body = new ScrollBox<BvPropertyBox>()
            {
                AutoResize = false,
                Color = bodyColor,
                Padding = new Vector2(48f, 16f),
                Height = 200f,
                MinimumWidth = 300f,
            };

            body.scrollBar.Width = 14f;
            body.scrollBar.slide.bar.Color = new Color(0, 0, 0, 0); // temporary

            selectionBox = new TexturedBox(body.background)
            {
                Color = selectionBoxColor,
                Padding = new Vector2(30f, 0f),
                ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
            };

            tab = new TexturedBox(selectionBox)
            {
                Width = 3f,
                Offset = new Vector2(15f, 0f),
                Color = new Color(225, 225, 240, 255),
                ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
            };

            footer = new DoubleLabelBox()
            {
                AutoResize = false,
                Padding = new Vector2(48f, 0f),
                Height = 24f,
                Color = headerColor,
            };

            main = new HudChain<HudElementBase>(this)
            {
                AutoResize = true,
                AlignVertical = true,
                ChildContainer =
                {
                    { header, true },
                    { body, true },
                    { footer, true }
                }
            };

            bgOpacity = 0.9f;
            BgOpacity = 0.9f;
            Count = 0;

            listWrapTimer = new Utils.Stopwatch();
            listWrapTimer.Start();
        }

        /// <summary>
        /// Updates property text.
        /// </summary>
        public void UpdateText()
        {
            if (target != null)
            {
                for (int n = 0; n < Count; n++)
                {
                    if (n == index)
                        body.ChainElements[n].UpdateText(true, PropOpen);
                    else
                        body.ChainElements[n].UpdateText(false, false);
                }

                //footer.LeftText.SetText(new RichText($"[{body.VisStart} - {body.VisStart + body.VisCount - 1} of {body.EnabledCount}]", footerTextLeft));
                footer.LeftText.SetText(new RichText($"[{body.Start}/{index}/{body.End}; {GetFirstIndex()}/{GetLastIndex()}]", footerTextLeft));

                if (target.IsWorking)
                    footer.RightText.SetText(new RichText("[Working]", footerTextRight));
                else if (target.IsFunctional)
                    footer.RightText.SetText(new RichText("[Functional]", footerTextRight));
                else
                    footer.RightText.SetText(new RichText("[Incomplete]", blockIncText));
            }
        }

        public override void BeforeDraw()
        {
            base.BeforeDraw();

            main.Width = body.Width;

            if (Selection != null)
            {
                selectionBox.Size = new Vector2(body.Width - body.divider.Width - body.scrollBar.Width, Selection.Size.Y + (2f * Scale));
                selectionBox.Offset = new Vector2(0f, Selection.Offset.Y - (1f * Scale));
                tab.Height = selectionBox.Height;
            }
        }

        protected override void HandleInput()
        {
            if (BvBinds.ScrollUp.IsNewPressed)
                Scroll(-1);
            else if (BvBinds.ScrollDown.IsNewPressed)
                Scroll(1);

            if (BvBinds.Select.IsNewPressed)
                ToggleSelect();

            if (SharedBinds.Enter.IsNewPressed)
                ToggleTextBox();
        }

        /// <summary>
        /// Interprets scrolling input. If a property is currently opened, scrolling will change the
        /// current property value, if applicable. Otherwise, it will change the current property selection.
        /// </summary>
        private void Scroll(int dir)
        {
            if (!PropOpen)
            {
                UpdateIndex(BvBinds.MultX.IsPressed ? dir * 5 : dir);
                listWrapTimer.Reset();
            }
            else
            {
                var scrollable = Selection.BlockMember as IBlockScrollable;

                if (scrollable != null)
                {
                    if (dir < 0)
                        scrollable.ScrollUp();
                    else if (dir > 0)
                        scrollable.ScrollDown();
                }
            }
        }

        /// <summary>
        /// Updates the selection index.
        /// </summary>
        private void UpdateIndex(int offset)
        {
            int min = GetFirstIndex(), max = GetLastIndex();
            index += offset;

            for (int n = index; (n <= max && n >= min); n += offset)
            {
                if (body.ChainElements[n].BlockMember.Enabled)
                {
                    index = n;
                    break;
                }
            }

            if (listWrapTimer.ElapsedMilliseconds > 400 && (index > max || index < min))
            {
                if (index < min)
                {
                    index = max;
                    body.End = index;
                }
                else
                {
                    index = min;
                    body.Start = index;
                }
            }
            else
            {
                index = Utils.Math.Clamp(index, min, max);

                if (index < body.Start)
                    body.Start = index;
                else if (index > body.End)
                    body.End = index;
            }
        }

        /// <summary>
        /// Toggles property selection.
        /// </summary>
        private void ToggleSelect()
        {
            if (!PropOpen)
                OpenProp();
            else
                CloseProp();
        }

        /// <summary>
        /// Opens the currently highlighted property.
        /// </summary>
        private void OpenProp()
        {
            var blockAction = Selection.BlockMember as IBlockAction;

            if (blockAction == null)
            {
                PropOpen = true;
            }
            else
            {
                blockAction.Action();
            }
        }

        /// <summary>
        /// Closes the currently selected property.
        /// </summary>
        private void CloseProp()
        {
            if (Selection != null)
            {
                var textMember = Selection.BlockMember as IBlockTextMember;

                if (textMember != null)
                {
                    textMember.SetValueText(Selection.value.Text.GetText().ToString());              
                }

                Selection.value.InputOpen = false;
            }

            PropOpen = false;
        }

        /// <summary>
        /// Toggles the textbox of the selected property open/closed if the property supports text
        /// input.
        /// </summary>
        private void ToggleTextBox()
        {
            if (Selection.BlockMember is IBlockTextMember)
            {
                if (!PropOpen)
                {
                    OpenProp();
                    OpenTextInput();
                }
                else
                {
                    if (!MyAPIGateway.Gui.ChatEntryVisible)
                    {
                        OpenTextInput();
                    }
                    else
                        CloseProp();
                }
            }
        }

        private void OpenTextInput()
        {
            Selection.value.InputOpen = true;
            Selection.value.Text.SetFormatting(selectedText);
        }

        /// <summary>
        /// Sets the target block to the one given.
        /// </summary>
        public void SetTarget(PropertyBlock target)
        {
            Clear();
            this.target = target;

            for (int n = 0; n < body.ChainElements.Count; n++)
                body.ChainElements[n].BlockMember = null;

            for (int n = 0; n < target.BlockMembers.Count; n++)
            {
                AddMember(target.BlockMembers[n]);
            }

            index = GetFirstIndex();
        }

        /// <summary>
        /// Returns the index of the first enabled property.
        /// </summary>
        /// <returns></returns>
        private int GetFirstIndex()
        {
            int first = 0;

            for (int n = 0; n < Count; n++)
            {
                if (body.ChainElements[n].Enabled)
                {
                    first = n;
                    break;
                }
            }

            return first;
        }

        /// <summary>
        /// Retrieves the index of the last enabled property.
        /// </summary>
        /// <returns></returns>
        private int GetLastIndex()
        {
            int last = 0;

            for (int n = Count - 1; n >= 0; n--)
            {
                if (body.ChainElements[n].Enabled)
                {
                    last = n;
                    break;
                }
            }

            return last;
        }

        /// <summary>
        /// Adds the given block member to the list of <see cref="BvPropertyBox"/>es.
        /// </summary>
        private void AddMember(IBlockMember blockMember)
        {
            if (body.ChainElements.Count <= Count)
            {
                BvPropertyBox propBox = new BvPropertyBox(Count)
                {
                    ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
                };

                body.AddToList(propBox);
            }

            body.ChainElements[Count].BlockMember = blockMember;
            Count++;
        }

        /// <summary>
        /// Clears block data from the menu and resets the count.
        /// </summary>
        public void Clear()
        {
            CloseProp();

            for (int n = 0; n < body.ChainElements.Count; n++)
                body.ChainElements[n].BlockMember = null;

            target = null;
            PropOpen = false;
            index = 0;
            body.Start = 0;
            Count = 0;
        }

        private class BvPropertyBox : HudElementBase, IScrollBoxMember
        {
            public override bool Visible => base.Visible && Enabled;
            public bool Enabled => (BlockMember != null && BlockMember.Enabled);

            public override float Width
            {
                get { return name.Width + value.Width + postfix.Width; }
                set { base.Width = value; }
            }
            public override float Height
            {
                get { return Math.Max(name.Height, Math.Max(value.Height, postfix.Height)); }
                set { base.Height = value; }
            }

            public IBlockMember BlockMember
            {
                get { return blockMember; }
                set
                {
                    blockMember = value;

                    if (value != null)
                    {
                        var textMember = blockMember as IBlockTextMember;

                        if (textMember != null)
                            this.value.IsCharAllowedFunc = textMember.CharFilterFunc;
                        else
                            this.value.IsCharAllowedFunc = null;
                    }
                }
            }

            public readonly int index;
            public readonly Label name, postfix;
            public readonly TextBox value;

            private IBlockMember blockMember;

            public BvPropertyBox(int index, IHudParent parent = null) : base(parent)
            {
                this.index = index;

                name = new Label(this) { ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH };
                value = new TextBox(name) { ParentAlignment = ParentAlignment.Right };
                postfix = new Label(postfix) { ParentAlignment = ParentAlignment.Right };
            }

            public void UpdateText(bool highlighted, bool selected)
            {
                name.Text.Format = bodyText;
                postfix.Text.Format = bodyText;

                if (BlockMember.Name != null && BlockMember.Name.Length > 0)
                    name.Text.SetText($"{BlockMember.Name}: ");
                else
                    name.Text.Clear();

                if (BlockMember.Postfix != null && BlockMember.Postfix.Length > 0)
                    postfix.Text.SetText($" ({BlockMember.Postfix})");
                else
                    postfix.Text.Clear();

                if (highlighted)
                {
                    if (selected)
                        value.Text.Format = selectedText;                       
                    else
                        value.Text.Format = highlightText;
                }
                else
                    value.Text.Format = bodyText;

                if (!value.InputOpen)
                    value.Text.SetText(BlockMember.Value);
            }
        }
    }
}