using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System;
using System.Diagnostics;
using System.Text;
using VRage.Game.ModAPI;
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
        public int Count => scrollBody.Collection.Count;

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
                if (PropertiesMenu.Target.TBlock != null && value != ScrollMenuModes.Peek && Count == 0)
                    UpdateProperties();

                _menuMode = value;
            }
        }

        /// <summary>
        /// Currently highlighted property. Null if none selected.
        /// </summary>
        private BvPropertyBox Selection => (index < scrollBody.Collection.Count) ? scrollBody.Collection[index].Element : null;

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

        private readonly BvPropBoxPool propBoxPool;
        private readonly Stopwatch listWrapTimer, notificationTimer;

        /// <summary>
        /// Index of the currently selected property
        /// </summary>
        private int index;
        private int tick;

        private float _bgOpacity;
        private bool waitingForChat;

        private string notification;
        private ScrollMenuModes _menuMode;
        private readonly RichText peekBuilder;

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
                UseSmoothScrolling = false,
                SizingMode = HudChainSizingModes.ClampChainOffAxis | HudChainSizingModes.FitChainAlignAxis,
                MinVisibleCount = 10,
                Padding = new Vector2(48f, 16f),
            };

            scrollBody.ScrollBar.Padding = new Vector2(12f, 16f);
            scrollBody.ScrollBar.Width = 4f;

            selectionBox = new TexturedBox(scrollBody.Background)
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

            footer.LeftTextBuilder.Format = footerTextLeft;

            layout = new HudChain(true, this)
            {
                MemberMinSize = new Vector2(300f, 0f),
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                CollectionContainer =
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

            propBoxPool = new BvPropBoxPool();
            peekBuilder = new RichText();
            notificationTimer = new Stopwatch();
            listWrapTimer = new Stopwatch();

            listWrapTimer.Start();
        }

        /// <summary>
        /// Updates menu text
        /// </summary>
        public void UpdateText()
        {
            if (PropertiesMenu.Target != null)
            {
                if (MenuMode == ScrollMenuModes.Peek)
                    UpdatePeekBody();
                else if (tick % 3 == 0)
                    UpdatePropertyBody();

                if (tick % 3 == 0)
                    UpdateFooterText();
            }
            else
                footer.RightTextBoard.SetText("[Target is null]", blockIncText);

            tick++;

            if (tick % 12 == 0)
                tick = 0;
        }

        /// <summary>
        /// Updates peek body text
        /// </summary>
        private void UpdatePeekBody()
        {
            if (tick % 12 == 0)
            {
                peekBuilder.Clear();

                foreach (SuperBlock.SubtypeAccessorBase subtype in PropertiesMenu.Target.SubtypeAccessors)
                {
                    if (subtype != null)
                        subtype.GetSummary(peekBuilder, bodyText, valueText);
                }

                peekBody.TextBoard.SetText(peekBuilder);
            }
        }

        /// <summary>
        /// Updates text for property list
        /// </summary>
        private void UpdatePropertyBody()
        {
            for (int n = 0; n < Count; n++)
            {
                var entry = scrollBody.Collection[n];
                BvPropertyBox propertyBox = entry.Element;
                entry.Enabled = propertyBox.BlockMember.Enabled;

                if (n == index)
                {
                    if ((!PropOpen || updateSelection) && !propertyBox.InputOpen)
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
                footer.LeftTextBuilder.SetText($"[{notification}]");

                if (notificationTimer.ElapsedMilliseconds > notifTime)
                    notification = null;
            }
            else if (MenuMode == ScrollMenuModes.Dupe)
            {
                int copyCount = 0;

                for (int n = 0; n < Count; n++)
                {
                    if (scrollBody.Collection[n].Element.Copying)
                        copyCount++;
                }

                footer.LeftTextBuilder.SetText($"[Copying {copyCount} of {scrollBody.EnabledCount}]");
            }
            else if (MenuMode == ScrollMenuModes.Peek)
                footer.LeftTextBuilder.SetText("[Peeking]");
            else
                footer.LeftTextBuilder.SetText($"[{scrollBody.VisStart + 1} - {scrollBody.VisStart + scrollBody.VisCount} of {scrollBody.EnabledCount}]");

            if (PropertiesMenu.Target.IsWorking)
                footer.RightTextBoard.SetText("[Working]", footerTextRight);
            else if (PropertiesMenu.Target.IsFunctional)
                footer.RightTextBoard.SetText("[Functional]", footerTextRight);
            else
                footer.RightTextBoard.SetText("[Incomplete]", blockIncText);
        }

        /// <summary>
        /// Displays a temporary message in the footer.
        /// </summary>
        public void ShowNotification(string message)
        {
            notification = message;
            notificationTimer.Restart();
        }

        protected override void Layout()
        {
            if (MenuMode == ScrollMenuModes.Control || MenuMode == ScrollMenuModes.Dupe)
            {
                scrollBody.Visible = true;
                peekBody.Visible = false;

                float memberWidth = 0f;
                int visCount = 0;
                var entries = scrollBody.Collection;

                for (int i = 0; i < entries.Count && visCount < scrollBody.VisCount; i++)
                {
                    int j = Math.Min(scrollBody.VisStart + i, entries.Count - 1);
                    HudElementBase element = entries[j].Element;

                    if (element.Visible)
                    {
                        memberWidth = Math.Max(memberWidth, element.Width);
                        visCount++;
                    }
                }

                memberWidth += scrollBody.Padding.X + scrollBody.ScrollBar.Width + scrollBody.Divider.Width;
                layout.Width = Math.Max(memberWidth, layout.MemberMinSize.X);
            }
            else if (MenuMode == ScrollMenuModes.Peek)
            {
                peekBody.Visible = true;
                scrollBody.Visible = false;

                peekBody.TextBoard.FixedSize = new Vector2(0, peekBody.TextBoard.TextSize.Y);
                layout.Width = 300f;
            }

            if (AlignToEdge)
            {
                Vector2 alignment = Vector2.Zero;

                if (Offset.X > 0f)
                    alignment.X = -layout.Width / 2f;
                else
                    alignment.X = layout.Width / 2f;

                if (Offset.Y > 0f)
                    alignment.Y = -layout.Height / 2f;
                else
                    alignment.Y = layout.Height / 2f;

                layout.Offset = alignment;
            }
            else
                layout.Offset = Vector2.Zero;
        }

        protected override void Draw()
        {
            if (Selection != null)
            {
                selectionBox.Size = new Vector2(scrollBody.Width - scrollBody.Divider.Width - scrollBody.ScrollBar.Width, Selection.Size.Y + 2f);
                selectionBox.Offset = new Vector2(0f, Selection.Offset.Y - 1f);
                tab.Height = selectionBox.Height;
            };
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (MenuMode != ScrollMenuModes.Peek && !BvBinds.Open.IsPressed)
            {
                if (!HudMain.Cursor.Visible)
                {
                    if (BvBinds.ToggleSelectMode.IsNewPressed || (MenuMode == ScrollMenuModes.Control && BvBinds.SelectAll.IsNewPressed))
                        ToggleDuplicationMode();

                    HandleSelectionInput();

                    if (MenuMode == ScrollMenuModes.Dupe)
                        HandleDuplicatorInput();
                    else if (MenuMode == ScrollMenuModes.Control)
                        HandlePropertyInput();
                }
                else
                    CloseProp();
            }
        }

        /// <summary>
        /// Sets the target block to the one given.
        /// </summary>
        public void UpdateTarget()
        {
            Clear();

            if (MenuMode != ScrollMenuModes.Peek)
                UpdateProperties();
        }

        /// <summary>
        /// Adds block member property boxes
        /// </summary>
        private void UpdateProperties()
        {
            PropertyBlock block = PropertiesMenu.Target;

            for (int n = 0; n < block.BlockMembers.Count; n++)
                AddMember(block.BlockMembers[n]);

            index = GetFirstIndex();
            scrollBody.Start = 0;
        }

        /// <summary>
        /// Adds the given block member to the list of <see cref="BvPropertyBox"/>es.
        /// </summary>
        private void AddMember(IBlockMember blockMember)
        {
            var entry = propBoxPool.Get();
            entry.Enabled = true;
            entry.Element.BlockMember = blockMember;

            scrollBody.Add(entry);
        }

        /// <summary>
        /// Clears block data from the menu and resets the count.
        /// </summary>
        public void Clear()
        {
            propBoxPool.ReturnRange(scrollBody.Collection, 0, scrollBody.Collection.Count);
            scrollBody.Clear();

            waitingForChat = false;
            PropOpen = false;
            index = 0;
            scrollBody.Start = 0;
        }

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

                StringBuilder disp = _blockMember.Display,
                    status = _blockMember.Status;

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