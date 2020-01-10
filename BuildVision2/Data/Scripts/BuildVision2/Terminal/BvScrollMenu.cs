using RichHudFramework;
using RichHudFramework.Game;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
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
        public override float Width { get { return layout.Width; } set { layout.Width = value; } }
        public override float Height { get { return layout.Height; } set { layout.Height = value; } }

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

        public int MaxVisible { get { return body.MinimumVisCount; } set { body.MinimumVisCount = value; } }
        public int Count { get; private set; }
        public bool PropOpen { get; private set; }
        private BvPropertyBox Selection => (index < body.List.Count) ? body.List[index] : null;

        public readonly LabelBox header;
        public readonly DoubleLabelBox footer;
        public readonly TexturedBox selectionBox, tab;

        private readonly ScrollBox<BvPropertyBox> body;
        private readonly HudChain<HudElementBase> layout;
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
            ShareCursor = false;

            header = new LabelBox()
            {
                Format = headerText,
                Text = "Build Vision",
                AutoResize = false,
                Height = 34f,
                Color = headerColor,
            };

            body = new ScrollBox<BvPropertyBox>()
            {
                AlignVertical = true,
                FitToChain = true,
                Color = bodyColor,
                Padding = new Vector2(48f, 16f),
                MinimumVisCount = 10,
                MinimumSize = new Vector2(300f, 0f)
            };

            body.scrollBar.Padding = new Vector2(12f, 16f);
            body.scrollBar.Width = 4f;
            body.Members.AutoResize = false;

            selectionBox = new TexturedBox(body.Members)
            {
                Color = selectionBoxColor,
                Padding = new Vector2(30f, 0f),
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
            };

            tab = new TexturedBox(selectionBox)
            {
                Width = 3f,
                Offset = new Vector2(15f, 0f),
                Color = new Color(225, 225, 240, 255),
                ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
            };

            footer = new DoubleLabelBox()
            {
                AutoResize = false,
                TextPadding = new Vector2(48f, 0f),
                Height = 24f,
                Color = headerColor,
            };

            footer.LeftTextBoard.Format = footerTextLeft;
            footer.BuilderMode = TextBuilderModes.Unlined;

            layout = new HudChain<HudElementBase>(this)
            {
                AutoResize = true,
                AlignVertical = true,
                ChildContainer =
                {
                    header,
                    body, 
                    footer
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
                        body.List[n].UpdateText(true, PropOpen);
                    else
                        body.List[n].UpdateText(false, false);
                }

                footer.LeftText = $"[{body.VisStart + 1} - {body.VisStart + body.VisCount} of {body.EnabledCount}]";

                if (target.IsWorking)
                    footer.RightText = new RichText("[Working]", footerTextRight);
                else if (target.IsFunctional)
                    footer.RightText = new RichText("[Functional]", footerTextRight);
                else
                    footer.RightText = new RichText("[Incomplete]", blockIncText);
            }
            else
                footer.RightText = new RichText("[Target is null]", blockIncText);

            //footer.LeftText = $"[{body.Start}/{index}/{body.End}; {body.scrollBar.Min}/{body.scrollBar.Max}]";
        }

        protected override void Draw()
        {
            layout.Width = body.Width;

            if (Selection != null)
            {
                selectionBox.Size = new Vector2(body.Width - body.divider.Width - body.scrollBar.Width, Selection.Size.Y + (2f * Scale));
                selectionBox.Offset = new Vector2((-22f * Scale), Selection.Offset.Y - (1f * Scale));
                tab.Height = selectionBox.Height;
            };
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
                UpdateIndex(BvBinds.MultX.IsPressed ? dir * 4 : dir);
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
            int min = GetFirstIndex(), max = GetLastIndex(), dir = (offset > 0) ? 1 : -1;
            offset = Math.Abs(offset);

            for (int x = 1; x <= offset; x++)
            {
                index += dir;

                for (int y = index; (y <= max && y >= min); y += dir)
                {
                    if (body.List[y].BlockMember.Enabled)
                    {
                        index = y;
                        break;
                    }
                }
            }

            if (listWrapTimer.ElapsedMilliseconds > 400 && (index > max || index < min) && !BvBinds.MultX.IsPressed)
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
        /// Toggles the textbox of the selected property open/closed if the property supports text
        /// input.
        /// </summary>
        private void ToggleTextBox()
        {
            if (Selection.BlockMember is IBlockTextMember)
            {
                if (!PropOpen)
                {
                    if (!MyAPIGateway.Gui.ChatEntryVisible)
                    {
                        PropOpen = true;
                        OpenTextInput();
                    }
                }
                else
                {
                    if (!MyAPIGateway.Gui.ChatEntryVisible)
                        OpenTextInput();
                    else
                        CloseProp();
                }
            }
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

                if (Selection.BlockMember is IBlockTextMember)
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                        OpenTextInput();
                }
            }
            else
            {
                blockAction.Action();
            }
        }

        private void OpenTextInput()
        {
            Selection.value.OpenInput();
            Selection.value.TextBoard.SetFormatting(selectedText);
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
                    textMember.SetValueText(Selection.value.Text.ToString());
                }

                Selection.value.CloseInput();
            }

            PropOpen = false;
        }

        /// <summary>
        /// Sets the target block to the one given.
        /// </summary>
        public void SetTarget(PropertyBlock newTarget)
        {
            Clear();
            target = newTarget;

            for (int n = 0; n < target.BlockMembers.Count; n++)
            {
                AddMember(target.BlockMembers[n]);
            }

            index = GetFirstIndex();
            body.Start = 0;
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
                if (body.List[n].Enabled)
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
                if (body.List[n].Enabled)
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
            if (body.List.Count <= Count)
            {
                BvPropertyBox propBox = new BvPropertyBox(Count)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
                };

                body.AddToList(propBox);
            }

            body.List[Count].Enabled = true;
            body.List[Count].BlockMember = blockMember;
            Count++;
        }

        /// <summary>
        /// Clears block data from the menu and resets the count.
        /// </summary>
        public void Clear()
        {
            for (int n = 0; n < body.List.Count; n++)
            {
                body.List[n].value.CloseInput();
                body.List[n].Enabled = false;
                body.List[n].BlockMember = null;
            }

            target = null;
            PropOpen = false;
            index = 0;
            body.Start = 0;
            Count = 0;
        }

        private class BvPropertyBox : HudElementBase, IListBoxEntry
        {
            public override bool Visible => base.Visible && Enabled;
            public bool Enabled { get { return enabled && (BlockMember!= null && BlockMember.Enabled); } set { enabled = value; } }

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
                            this.value.CharFilterFunc = textMember.CharFilterFunc;
                        else
                            this.value.CharFilterFunc = null;
                    }
                }
            }

            public readonly int index;
            public readonly Label name, postfix;
            public readonly TextBox value;

            private IBlockMember blockMember;
            private bool enabled;

            public BvPropertyBox(int index, IHudParent parent = null) : base(parent)
            {
                this.index = index;

                name = new Label(this)
                { ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH };

                value = new TextBox(name)
                { ParentAlignment = ParentAlignments.Right, UseMouseInput = false };

                postfix = new Label(value)
                { ParentAlignment = ParentAlignments.Right };
            }

            protected override void Draw()
            {
                Width = name.Width + value.Width + postfix.Width;
                Height = Math.Max(name.Height, Math.Max(value.Height, postfix.Height));
            }

            public void UpdateText(bool highlighted, bool selected)
            {
                name.Format = bodyText;
                postfix.Format = bodyText;

                if (BlockMember.Name != null && BlockMember.Name.Length > 0)
                    name.Text = $"{BlockMember.Name}: ";
                else
                    name.TextBoard.Clear();

                if (highlighted)
                {
                    if (selected)
                        value.Format = selectedText;
                    else
                        value.Format = highlightText;
                }
                else
                    value.Format = bodyText;

                if (!value.InputOpen)
                    value.Text = BlockMember.Value;

                if (BlockMember.Postfix != null && BlockMember.Postfix.Length > 0)
                    postfix.Text = $" {BlockMember.Postfix}";
                else
                    postfix.TextBoard.Clear();
            }
        }
    }
}