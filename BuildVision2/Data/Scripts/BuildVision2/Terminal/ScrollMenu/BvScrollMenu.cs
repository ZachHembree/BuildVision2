﻿using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    public sealed partial class BvScrollMenu : HudElementBase
    {
        private const long notifTime = 2000;
        private static readonly Color
            headerColor = new Color(41, 54, 62),
            bodyColor = new Color(70, 78, 86),
            selectionBoxColor = new Color(41, 54, 62);
        private static readonly GlyphFormat
            headerText = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            bodyText = new GlyphFormat(Color.White, textSize: .885f),
            valueText = bodyText.WithColor(new Color(210, 210, 210)),
            footerTextLeft = bodyText.WithColor(new Color(220, 235, 245)),
            footerTextRight = footerTextLeft.WithAlignment(TextAlignment.Right),
            highlightText = bodyText.WithColor(new Color(220, 180, 50)),
            selectedText = bodyText.WithColor(new Color(50, 200, 50)),
            blockIncText = footerTextRight.WithColor(new Color(200, 35, 35));

        public override float Width { get { return layout.Width; } set { layout.Width = value; } }

        public override float Height { get { return layout.Height; } set { layout.Height = value; } }

        public override Vector2 Padding { get { return layout.Padding; } set { layout.Padding = value; } }

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
                peekBody.Color = peekBody.Color.SetAlphaPct(_bgOpacity);
                scrollBody.Color = scrollBody.Color.SetAlphaPct(_bgOpacity);
                footer.Color = footer.Color.SetAlphaPct(_bgOpacity);
                _bgOpacity = value;
            }
        }

        /// <summary>
        /// Maximum number of properties visible at once
        /// </summary>
        public int MaxVisible { get { return scrollBody.MinVisibleCount; } set { scrollBody.MinVisibleCount = value; } }

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

        public ScrollMenuModes MenuMode
        {
            get { return _menuMode; }
            set
            {
                if (Target != null && value != ScrollMenuModes.Peek && Count == 0)
                    AddMembers();

                _menuMode = value;
            }
        }

        /// <summary>
        /// Currently highlighted property. Null if none selected.
        /// </summary>
        private BvPropertyBox Selection => (index < scrollBody.ChainEntries.Count) ? scrollBody.ChainEntries[index].Element : null;

        /// <summary>
        /// Returns the block currently targeted
        /// </summary>
        public PropertyBlock Target { get; private set; }

        /// <summary>
        /// If true, then if the property currently selected and open will have its text updated.
        /// </summary>
        private bool updateSelection;

        public readonly LabelBox header;
        public readonly TexturedBox selectionBox, tab;
        public readonly DoubleLabelBox footer;

        private readonly LabelBox peekBody;
        private readonly ScrollBox<ScrollBoxEntry<BvPropertyBox>, BvPropertyBox> scrollBody;
        private readonly HudChain layout;

        private readonly Utils.Stopwatch peekUpdateTimer, listWrapTimer, notificationTimer;

        /// <summary>
        /// Index of the currently selected property
        /// </summary>
        private int index;

        private float _bgOpacity;
        private Vector2 alignment;
        private bool targetChanged, waitingForChat;

        private string notification;
        private ScrollMenuModes _menuMode;

        public BvScrollMenu(HudParentBase parent = null) : base(parent)
        {
            header = new LabelBox()
            {
                Format = headerText,
                Text = "Build Vision",
                AutoResize = false,
                Size = new Vector2(300f, 34f),
                Color = headerColor,
            };

            peekBody = new LabelBox()
            {
                AutoResize = false,
                VertCenterText = false,
                Color = bodyColor,
                TextPadding = new Vector2(48f, 16f),
                BuilderMode = TextBuilderModes.Lined,
            };

            scrollBody = new ScrollBox<ScrollBoxEntry<BvPropertyBox>, BvPropertyBox>(true)
            {
                Color = bodyColor,
                EnableScrolling = false,
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainOffAxis | HudChainSizingModes.FitChainAlignAxis,
                MinVisibleCount = 10,
                Padding = new Vector2(48f, 16f),
                MemberMinSize = new Vector2(300f, 0f),
            };

            scrollBody.scrollBar.Padding = new Vector2(12f, 16f);
            scrollBody.scrollBar.Width = 4f;

            selectionBox = new TexturedBox(scrollBody.background)
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
                Size = new Vector2(300f, 24f),
                Color = headerColor,
            };

            footer.LeftTextBoard.Format = footerTextLeft;

            layout = new HudChain(true, this)
            {
                MemberMinSize = new Vector2(300f, 0f),
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                ChainContainer =
                {
                    header,
                    peekBody,
                    scrollBody,
                    footer
                }
            };

            _bgOpacity = 0.9f;
            BgOpacity = 0.9f;
            MenuMode = ScrollMenuModes.Peek;
            Count = 0;

            peekUpdateTimer = new Utils.Stopwatch();
            notificationTimer = new Utils.Stopwatch();
            listWrapTimer = new Utils.Stopwatch();

            peekUpdateTimer.Start();
            listWrapTimer.Start();
        }

        /// <summary>
        /// Updates menu text
        /// </summary>
        public void UpdateText()
        {
            if (Target != null)
            {
                if (MenuMode == ScrollMenuModes.Peek)
                    UpdatePeekBody();
                else
                    UpdatePropertyBody();

                UpdateFooterText();
            }
            else
                footer.RightText = new RichText("[Target is null]", blockIncText);

            targetChanged = false;
        }

        /// <summary>
        /// Updates peek body text
        /// </summary>
        private void UpdatePeekBody()
        {
            if (peekUpdateTimer.ElapsedMilliseconds > 100 || targetChanged)
            {
                var peekText = new RichText();

                foreach (SuperBlock.SubtypeAccessorBase subtype in Target.SubtypeAccessors)
                {
                    if (subtype != null)
                    {
                        RichText summary = subtype.GetSummary(bodyText, valueText);

                        if (summary != null)
                            peekText.Add(summary);
                    }
                }

                peekBody.TextBoard.SetText(peekText);
                peekUpdateTimer.Reset();
            }
        }

        /// <summary>
        /// Updates text for property list
        /// </summary>
        private void UpdatePropertyBody()
        {
            for (int n = 0; n < Count; n++)
            {
                var entry = scrollBody.ChainEntries[n];
                BvPropertyBox propertyBox = entry.Element;
                entry.Enabled = propertyBox.BlockMember.Enabled;

                if (n == index)
                {
                    if ((!PropOpen || updateSelection) && !propertyBox.valueText.InputOpen)
                        propertyBox.UpdateText(true, PropOpen);
                }
                else
                    propertyBox.UpdateText(false, false);
            }
        }

        /// <summary>
        /// Updates footer text
        /// </summary>
        private void UpdateFooterText()
        {
            if (notification != null)
            {
                footer.LeftText = $"[{notification}]";

                if (notificationTimer.ElapsedMilliseconds > notifTime)
                    notification = null;
            }
            else if (MenuMode == ScrollMenuModes.Dupe)
            {
                int copyCount = 0;

                for (int n = 0; n < Count; n++)
                {
                    if (scrollBody.ChainEntries[n].Element.Copying)
                        copyCount++;
                }

                footer.LeftText = $"[Copying {copyCount} of {scrollBody.EnabledCount}]";
            }
            else if (MenuMode == ScrollMenuModes.Peek)
                footer.LeftText = "[Peeking]";
            else
                footer.LeftText = $"[{scrollBody.VisStart + 1} - {scrollBody.VisStart + scrollBody.VisCount} of {scrollBody.EnabledCount}]";

            if (Target.IsWorking)
                footer.RightText = new RichText($"[Working]", footerTextRight);
            else if (Target.IsFunctional)
                footer.RightText = new RichText($"[Functional]", footerTextRight);
            else
                footer.RightText = new RichText($"[Incomplete]", blockIncText);
        }

        /// <summary>
        /// Displays a temporary message in the footer.
        /// </summary>
        public void ShowNotification(string message)
        {
            notification = message;
            notificationTimer.Start();
        }

        protected override void Layout()
        {
            if (MenuMode == ScrollMenuModes.Control || MenuMode == ScrollMenuModes.Dupe)
            {
                scrollBody.Visible = true;
                peekBody.Visible = false;
                // Assigns scrollBody width from last frame
                layout.Width = scrollBody.Width;
            }
            else if (MenuMode == ScrollMenuModes.Peek)
            {
                peekBody.Visible = true;
                scrollBody.Visible = false;

                peekBody.TextBoard.FixedSize = new Vector2(0, peekBody.TextBoard.TextSize.Y);
                layout.Width = 300f * Scale;
            }

            if (AlignToEdge)
            {
                if (base.Offset.X < 0)
                    alignment.X = Width / 2f;
                else
                    alignment.X = -Width / 2f;

                if (base.Offset.Y < 0)
                    alignment.Y = Height / 2f;
                else
                    alignment.Y = -Height / 2f;
            }
        }

        protected override void Draw(object matrix)
        {
            if (Selection != null)
            {
                selectionBox.Size = new Vector2(scrollBody.Width - scrollBody.divider.Width - scrollBody.scrollBar.Width, Selection.Size.Y + (2f * Scale));
                selectionBox.Offset = new Vector2(0f, Selection.Offset.Y - (1f * Scale));
                tab.Height = selectionBox.Height;
            };
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MenuMode != ScrollMenuModes.Peek && !BvBinds.Open.IsPressed)
            {
                if (BvBinds.ToggleSelectMode.IsNewPressed || (MenuMode == ScrollMenuModes.Control && BvBinds.SelectAll.IsNewPressed))
                    ToggleDuplicationMode();

                HandleSelectionInput();

                if (MenuMode == ScrollMenuModes.Dupe)
                    HandleDuplicatorInput();
                else if (MenuMode == ScrollMenuModes.Control)
                    HandlePropertyInput();
            }
        }

        /// <summary>
        /// Sets the target block to the one given.
        /// </summary>
        public void SetTarget(PropertyBlock newTarget)
        {
            Clear();
            Target = newTarget;
            targetChanged = Target != newTarget;

            if (MenuMode != ScrollMenuModes.Peek)
                AddMembers();
        }

        /// <summary>
        /// Adds block member property boxes
        /// </summary>
        private void AddMembers()
        {
            for (int n = 0; n < Target.BlockMembers.Count; n++)
                AddMember(Target.BlockMembers[n]);

            index = GetFirstIndex();
            scrollBody.Start = 0;
        }

        /// <summary>
        /// Adds the given block member to the list of <see cref="BvPropertyBox"/>es.
        /// </summary>
        private void AddMember(IBlockMember blockMember)
        {
            if (scrollBody.ChainEntries.Count <= Count)
            {
                scrollBody.Add(new BvPropertyBox(Count));
            }

            var entry = scrollBody.ChainEntries[Count];
            entry.Enabled = true;
            entry.Element.BlockMember = blockMember;

            Count++;
        }

        /// <summary>
        /// Clears block data from the menu and resets the count.
        /// </summary>
        public void Clear()
        {
            if (Count != 0)
            {
                for (int n = 0; n < scrollBody.ChainEntries.Count; n++)
                {
                    var entry = scrollBody.ChainEntries[n];
                    entry.Enabled = false;
                    entry.Element.Clear();
                }
            }
            
            waitingForChat = false;
            PropOpen = false;
            index = 0;
            scrollBody.Start = 0;
            Target = null;
            Count = 0;
        }

        private class BvPropertyBox : HudElementBase
        {
            /// <summary>
            /// Width of the property box
            /// </summary>
            public override float Width 
            { 
                get { return layout.Width; } 
                set 
                {
                    /*value = Math.Max(value - (layout.Width - valueText.Width), 0f);

                    if (value > layout.Padding.X)
                        value -= layout.Padding.X;

                    valueText.Width = value; */
                } 
            }

            /// <summary>
            /// Height of the property box
            /// </summary>
            public override float Height { get { return layout.Height; } set { /*layout.Height = value;*/ } }

            public override Vector2 Padding { get { return layout.Padding; } set { layout.Padding = value; } }

            /// <summary>
            /// Indicates whether or not the property is currently being copied.
            /// </summary>
            public bool Copying { get { return copyIndicator.Visible; } set { copyIndicator.Visible = value && (_blockMember is IBlockProperty); } }

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
                            this.valueText.CharFilterFunc = textMember.CharFilterFunc;
                        else
                            this.valueText.CharFilterFunc = null;

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
            public readonly TextBox valueText;

            private readonly SelectionBox copyIndicator;
            private readonly HudChain layout;
            private IBlockMember _blockMember;

            public BvPropertyBox(int index, HudParentBase parent = null) : base(parent)
            {
                this.index = index;
                ParentAlignment = ParentAlignments.Left;

                copyIndicator = new SelectionBox();
                name = new Label();
                valueText = new TextBox() { UseCursor = false };
                postfix = new Label();

                layout = new HudChain(false, this)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH,
                    ChainContainer = { copyIndicator, name, valueText, postfix }
                };
            }

            /// <summary>
            /// Clears property information from the property box
            /// </summary>
            public void Clear()
            {
                name.TextBoard.Clear();
                postfix.TextBoard.Clear();
                valueText.CloseInput();
                BlockMember = null;
            }

            protected override void Layout()
            {
                float textHeight = Math.Max(name.TextBoard.TextSize.Y, Math.Max(name.TextBoard.TextSize.Y, name.TextBoard.TextSize.Y));
                layout.Height = Math.Max(textHeight + layout.Padding.Y, layout.Height);
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
                        valueText.Format = selectedText;
                    else
                        valueText.Format = highlightText;
                }
                else
                    valueText.Format = BvScrollMenu.valueText;

                valueText.Text = _blockMember.Display;
                postfix.Text = $" {_blockMember.Status}";
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