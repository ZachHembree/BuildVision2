using RichHudFramework;
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
    public sealed partial class BvScrollMenu : HudElementBase
    {
        public override float Width { get { return layout.Width; } set { layout.Width = value; } }

        public override float Height { get { return header.Height + body.Height + footer.Height; } set { layout.Height = value; } }

        public override Vector2 Offset
        {
            get
            {
                if (AlignToEdge)
                    return base.Offset + alignment;
                else
                    return base.Offset;
            }
        }

        /// <summary>
        /// Opacity between 0 and 1
        /// </summary>
        public float BgOpacity
        {
            get { return _bgOpacity; }
            set
            {
                header.Color = header.Color.SetAlphaPct(_bgOpacity);
                body.Color = body.Color.SetAlphaPct(_bgOpacity);
                footer.Color = footer.Color.SetAlphaPct(_bgOpacity);
                _bgOpacity = value;
            }
        }

        /// <summary>
        /// Maximum number of properties visible at once
        /// </summary>
        public int MaxVisible { get { return body.MinimumVisCount; } set { body.MinimumVisCount = value; } }

        /// <summary>
        /// Number of block members registered with the menu
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// If true, then a property is currently selected and open
        /// </summary>
        public bool PropOpen { get; private set; }

        /// <summary>
        /// If true, then the menu will automatically align itself to the edge of the screen.
        /// </summary>
        public bool AlignToEdge { get; set; }

        public bool ReplicationMode { get; private set; }

        /// <summary>
        /// Currently highlighted property. Null if none selected.
        /// </summary>
        private BvPropertyBox Selection => (index < body.List.Count) ? body.List[index] : null;

        /// <summary>
        /// If true, then if the property currently selected and open will have its text updated.
        /// </summary>
        private bool updateSelection;

        public readonly LabelBox header;
        public readonly DoubleLabelBox footer;
        public readonly TexturedBox selectionBox, tab;

        private readonly ScrollBox<BvPropertyBox> body;
        private readonly HudChain<HudElementBase> layout;
        private readonly Utils.Stopwatch listWrapTimer;

        private int index;
        private float _bgOpacity;
        private PropertyBlock target;
        private Vector2 alignment;
        private bool waitingForChat;

        private string notification;
        private Utils.Stopwatch notificationTimer;
        private const long notifTime = 3000;

        private static readonly Color headerColor, bodyColor, selectionBoxColor;
        private static readonly GlyphFormat headerText, bodyText, valueText,
            footerTextLeft, footerTextRight,
            highlightText, selectedText, blockIncText;

        static BvScrollMenu()
        {
            headerColor = new Color(41, 54, 62);
            bodyColor = new Color(70, 78, 86);
            selectionBoxColor = new Color(41, 54, 62);

            headerText = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f);

            bodyText = new GlyphFormat(Color.White, textSize: .885f);
            valueText = bodyText.WithColor(new Color(210, 210, 210));
            highlightText = bodyText.WithColor(new Color(220, 180, 50));
            selectedText = bodyText.WithColor(new Color(50, 200, 50));

            footerTextLeft = bodyText.WithColor(new Color(220, 235, 245));
            footerTextRight = footerTextLeft.WithAlignment(TextAlignment.Right);
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
                SizingMode = ScrollBoxSizingModes.FitToMembers,
                Color = bodyColor,
                Padding = new Vector2(48f, 16f),
                MinimumVisCount = 10,
                MinimumSize = new Vector2(300f, 0f)
            };

            body.scrollBar.Padding = new Vector2(12f, 16f);
            body.scrollBar.Width = 4f;
            body.Chain.AutoResize = false;

            selectionBox = new TexturedBox(body.Chain)
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
                AutoResize = true,
                FitToTextElement = false,
                Padding = new Vector2(48f, 0f),
                Height = 24f,
                Color = headerColor,
            };

            footer.LeftTextBoard.Format = footerTextLeft;

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

            _bgOpacity = 0.9f;
            BgOpacity = 0.9f;
            Count = 0;

            notificationTimer = new Utils.Stopwatch();

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
                int copyCount = 0;

                for (int n = 0; n < Count; n++)
                {
                    if (body.List[n].Replicating)
                        copyCount++;

                    if (n == index)
                    {
                        if ((!PropOpen || updateSelection) && !body.List[n].value.InputOpen)
                            body.List[n].UpdateText(true, PropOpen);
                    }
                    else
                        body.List[n].UpdateText(false, false);
                }

                if (notification != null)
                {
                    footer.LeftText = $"[{notification}]";

                    if (notificationTimer.ElapsedMilliseconds > notifTime)
                        notification = null;
                }
                else if (ReplicationMode)
                    footer.LeftText = $"[Copying {copyCount} of {body.EnabledCount}]";
                else
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
        }

        /// <summary>
        /// Displays a temporary message in the footer.
        /// </summary>
        public void ShowNotification(string message)
        {
            notification = message;
            notificationTimer.Start();
        }

        protected override void Draw()
        {
            layout.Width = body.Width;

            if (base.Offset.X < 0)
                alignment.X = Width / 2f;
            else
                alignment.X = -Width / 2f;

            if (base.Offset.Y < 0)
                alignment.Y = Height / 2f;
            else
                alignment.Y = -Height / 2f;

            if (Selection != null)
            {
                selectionBox.Size = new Vector2(body.Width - body.divider.Width - body.scrollBar.Width, Selection.Size.Y + (2f * Scale));
                selectionBox.Offset = new Vector2((-22f * Scale), Selection.Offset.Y - (1f * Scale));
                tab.Height = selectionBox.Height;
            };
        }

        protected override void HandleInput()
        {
            if (BvBinds.ToggleSelectMode.IsNewPressed || (!ReplicationMode && BvBinds.SelectAll.IsNewPressed))
                ToggleReplicationMode();

            HandleSelectionInput();

            if (ReplicationMode)
                HandleReplicatorInput();
            else
                HandlePropertyInput();
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

            ReplicationMode = false;
            waitingForChat = false;
            target = null;
            PropOpen = false;
            index = 0;
            body.Start = 0;
            Count = 0;
        }

        private class BvPropertyBox : HudElementBase, IListBoxEntry
        {
            public override float Width { get { return layout.Width; } }
            public override float Height { get { return layout.Height; } }

            public override bool Visible => base.Visible && Enabled;
            public bool Enabled { get { return _enabled && (BlockMember!= null && BlockMember.Enabled); } set { _enabled = value; } }
            public bool Replicating { get { return selectionBox.Visible; } set { selectionBox.Visible = value && (_blockMember is IBlockProperty); } }

            public IBlockMember BlockMember
            {
                get { return _blockMember; }
                set
                {
                    _blockMember = value;
                    Replicating = false;

                    if (value != null)
                    {
                        var textMember = _blockMember as IBlockTextMember;

                        if (textMember != null)
                            this.value.CharFilterFunc = textMember.CharFilterFunc;
                        else
                            this.value.CharFilterFunc = null;

                        name.Format = bodyText;

                        if (_blockMember.Name != null && _blockMember.Name.Length > 0)
                            name.Text = $"{_blockMember.Name}: ";
                        else
                            name.TextBoard.Clear();
                    }
                }
            }

            public readonly int index;
            public readonly Label name, postfix;
            public readonly TextBox value;

            private readonly HudChain<HudElementBase> layout;
            private readonly SelectionBox selectionBox;
            private IBlockMember _blockMember;
            private bool _enabled, blockEnabled;

            public BvPropertyBox(int index, IHudParent parent = null) : base(parent)
            {
                this.index = index;

                selectionBox = new SelectionBox();
                name = new Label();
                value = new TextBox() { UseMouseInput = false };
                postfix = new Label();

                layout = new HudChain<HudElementBase>(this)
                {
                    AlignVertical = false,
                    ChildContainer = { selectionBox, name, value, postfix }
                };
            }

            protected override void Draw()
            {
                selectionBox.Height = Math.Max(name.Height, Math.Max(value.Height, postfix.Height));
            }

            public void UpdateText(bool highlighted, bool selected)
            {
                postfix.Format = bodyText;

                if (highlighted)
                {
                    if (selected)
                        value.Format = selectedText;
                    else
                        value.Format = highlightText;
                }
                else
                    value.Format = valueText;

                value.Text = _blockMember.Value;

                if (_blockMember.Postfix != null && _blockMember.Postfix.Length > 0)
                    postfix.Text = $" {_blockMember.Postfix}";
                else
                    postfix.TextBoard.Clear();
            }

            private class SelectionBox : Label
            {
                public SelectionBox(IHudParent parent = null) : base (parent)
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