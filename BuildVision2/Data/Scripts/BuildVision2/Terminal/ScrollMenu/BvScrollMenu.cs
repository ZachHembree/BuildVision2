using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public enum ScrollMenuModes
    {
        Peak = 1,
        Control = 2,
        Copy = 3
    }

    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    public sealed partial class BvScrollMenu : HudElementBase
    {
        private const long notifTime = 3000;

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
                peakBody.Color = peakBody.Color.SetAlphaPct(_bgOpacity);
                scrollBody.Color = scrollBody.Color.SetAlphaPct(_bgOpacity);
                footer.Color = footer.Color.SetAlphaPct(_bgOpacity);
                _bgOpacity = value;
            }
        }

        /// <summary>
        /// Maximum number of properties visible at once
        /// </summary>
        public int MaxVisible { get { return scrollBody.MinimumVisCount; } set { scrollBody.MinimumVisCount = value; } }

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
                if (target != null && value != ScrollMenuModes.Peak && Count == 0)
                    AddMembers();

                _menuMode = value;
            }
        }

        /// <summary>
        /// Currently highlighted property. Null if none selected.
        /// </summary>
        private BvPropertyBox Selection => (index < scrollBody.List.Count) ? scrollBody.List[index] : null;

        /// <summary>
        /// If true, then if the property currently selected and open will have its text updated.
        /// </summary>
        private bool updateSelection;

        public readonly LabelBox header;
        public readonly DoubleLabelBox footer;
        public readonly TexturedBox selectionBox, tab;

        private readonly LabelBox peakBody;
        private readonly ScrollBox<BvPropertyBox> scrollBody;
        private readonly HudChain<HudElementBase> layout;
        private readonly Utils.Stopwatch listWrapTimer;

        private int index;
        private float _bgOpacity;
        private PropertyBlock target;
        private Vector2 alignment;
        private bool waitingForChat;

        private string notification;
        private Utils.Stopwatch notificationTimer;
        private ScrollMenuModes _menuMode;

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

            peakBody = new LabelBox()
            {
                AutoResize = false,
                FitToTextElement = true,
                VertCenterText = false,
                Color = bodyColor,
                Padding = new Vector2(48f, 16f),
                BuilderMode = TextBuilderModes.Lined,
            };

            scrollBody = new ScrollBox<BvPropertyBox>()
            {
                AlignVertical = true,
                SizingMode = ScrollBoxSizingModes.FitToMembers,
                Color = bodyColor,
                Padding = new Vector2(48f, 16f),
                MinimumVisCount = 10,
                MinimumSize = new Vector2(300f, 0f),
            };

            scrollBody.scrollBar.Padding = new Vector2(12f, 16f);
            scrollBody.scrollBar.Width = 4f;
            scrollBody.Chain.AutoResize = false;

            selectionBox = new TexturedBox(scrollBody.Chain)
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
                    peakBody,
                    scrollBody,
                    footer
                }
            };

            _bgOpacity = 0.9f;
            BgOpacity = 0.9f;
            MenuMode = ScrollMenuModes.Control;
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
                if (MenuMode == ScrollMenuModes.Peak)
                    UpdatePeakText();
                else if (MenuMode == ScrollMenuModes.Control)
                    UpdatePropertyText();

                if (target.IsWorking)
                    footer.RightText = new RichText($"[Working]", footerTextRight);
                else if (target.IsFunctional)
                    footer.RightText = new RichText($"[Functional]", footerTextRight);
                else
                    footer.RightText = new RichText($"[Incomplete]", blockIncText);
            }
            else
                footer.RightText = new RichText("[Target is null]", blockIncText);
        }

        private void UpdatePeakText()
        {
            var peakText = new RichText
            {
                { $"{MyTexts.TrySubstitute("Name")}: ", bodyText },
                { $"{target.TBlock.CustomName}\n", valueText }
            };

            if (target.SubtypeId.HasFlag(TBlockSubtypes.Powered))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", bodyText },
                    { $"{target.Power.GetPowerDisplay()}\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.Battery))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower)}", bodyText },
                    { $"{TerminalExtensions.GetPowerDisplay(target.Battery.PowerStored)}", valueText },
                    { $" ({((target.Battery.PowerStored / target.Battery.Capacity) * 100f).Round(1)}%)\n", bodyText },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower)}", bodyText },
                    { $"{TerminalExtensions.GetPowerDisplay(target.Battery.Capacity)}\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.GasTank))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.Oxygen_Filled).Split(':')[0]}: ", bodyText },
                    { $"{(target.GasTank.FillRatio * 100d).Round(1)}%\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.Warhead))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", bodyText },
                    { $"{target.Warhead.GetLocalizedStatus()} ", valueText },
                    { $"({Math.Truncate(target.Warhead.CountdownTime)}s)\n", bodyText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.Door))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", bodyText },
                    { $"{target.Door.GetLocalizedStatus()}\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.LandingGear))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", bodyText },
                    { $"{target.LandingGear.GetLocalizedStatus()}\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.Connector))
            {
                peakText.Add(new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", bodyText },
                    { $"{target.Connector.GetLocalizedStatus()}\n", valueText },
                });
            }

            if (target.SubtypeId.HasFlag(TBlockSubtypes.MechanicalConnection))
            {
                if (target.SubtypeId.HasFlag(TBlockSubtypes.Suspension))
                {
                    peakText.Add(new RichText {
                        { $"{MyTexts.TrySubstitute("Wheel")}: ", bodyText },
                        { $"{target.MechConnection.GetLocalizedStatus()}\n", valueText },
                    });
                }
                else
                {
                    peakText.Add(new RichText {
                        { $"{MyTexts.TrySubstitute("Head")}: ", bodyText },
                        { $"{target.MechConnection.GetLocalizedStatus()}\n", valueText },
                    });

                    if (target.MechConnection.PartAttached)
                    {
                        if (target.SubtypeId.HasFlag(TBlockSubtypes.Piston))
                        {
                            peakText.Add(new RichText {
                                { $"{MyTexts.GetString(MySpaceTexts.TerminalDistance)}: ", bodyText },
                                { $"{target.Piston.ExtensionDist.Round(2)}m\n", valueText },
                            });
                        }

                        if (target.SubtypeId.HasFlag(TBlockSubtypes.Rotor))
                        {
                            peakText.Add(new RichText {
                                { MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorCurrentAngle), bodyText },
                                { $"{target.Rotor.Angle.RadiansToDegrees().Round(2)}\n", valueText },
                            });
                        }
                    }
                }
            }

            footer.LeftText = new RichText("[Peaking]", footerTextRight);
            peakBody.TextBoard.SetText(peakText);
        }

        private void UpdatePropertyText()
        {
            int copyCount = 0;

            for (int n = 0; n < Count; n++)
            {
                if (scrollBody.List[n].Replicating)
                    copyCount++;

                if (n == index)
                {
                    if ((!PropOpen || updateSelection) && !scrollBody.List[n].value.InputOpen)
                        scrollBody.List[n].UpdateText(true, PropOpen);
                }
                else
                    scrollBody.List[n].UpdateText(false, false);
            }

            if (notification != null)
            {
                footer.LeftText = $"[{notification}]";

                if (notificationTimer.ElapsedMilliseconds > notifTime)
                    notification = null;
            }
            else if (MenuMode == ScrollMenuModes.Copy)
                footer.LeftText = $"[Copying {copyCount} of {scrollBody.EnabledCount}]";
            else
                footer.LeftText = $"[{scrollBody.VisStart + 1} - {scrollBody.VisStart + scrollBody.VisCount} of {scrollBody.EnabledCount}]";
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
            if (MenuMode == ScrollMenuModes.Control || MenuMode == ScrollMenuModes.Copy)
            {
                scrollBody.Visible = true;
                peakBody.Visible = false;
                layout.Width = scrollBody.Width;
            }
            else if (MenuMode == ScrollMenuModes.Peak)
            {
                peakBody.Visible = true;
                scrollBody.Visible = false;

                peakBody.TextBoard.FixedSize = new Vector2(0f, peakBody.TextBoard.TextSize.Y);
                layout.Width = 300f * Scale;
            }

            if (base.Offset.X < 0)
                alignment.X = Width / 2f;
            else
                alignment.X = -Width / 2f;

            if (base.Offset.Y < 0)
                alignment.Y = Height / 2f;
            else
                alignment.Y = -Height / 2f;
        }

        protected override void Draw()
        {
            if (Selection != null)
            {
                selectionBox.Size = new Vector2(scrollBody.Width - scrollBody.divider.Width - scrollBody.scrollBar.Width, Selection.Size.Y + (2f * Scale));
                selectionBox.Offset = new Vector2((-22f * Scale), Selection.Offset.Y - (1f * Scale));
                tab.Height = selectionBox.Height;
            };
        }

        protected override void HandleInput()
        {
            if (MenuMode != ScrollMenuModes.Peak)
            {
                if (BvBinds.ToggleSelectMode.IsNewPressed || (MenuMode == ScrollMenuModes.Control && BvBinds.SelectAll.IsNewPressed))
                    ToggleReplicationMode();

                HandleSelectionInput();

                if (MenuMode == ScrollMenuModes.Copy)
                    HandleReplicatorInput();
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
            target = newTarget;

            if (MenuMode != ScrollMenuModes.Peak)
                AddMembers();
        }

        private void AddMembers()
        {
            for (int n = 0; n < target.BlockMembers.Count; n++)
                AddMember(target.BlockMembers[n]);

            index = GetFirstIndex();
            scrollBody.Start = 0;
        }

        /// <summary>
        /// Adds the given block member to the list of <see cref="BvPropertyBox"/>es.
        /// </summary>
        private void AddMember(IBlockMember blockMember)
        {
            if (scrollBody.List.Count <= Count)
            {
                BvPropertyBox propBox = new BvPropertyBox(Count)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
                };

                scrollBody.AddToList(propBox);
            }

            scrollBody.List[Count].Enabled = true;
            scrollBody.List[Count].BlockMember = blockMember;
            Count++;
        }

        /// <summary>
        /// Clears block data from the menu and resets the count.
        /// </summary>
        public void Clear()
        {
            if (Count != 0)
            {
                for (int n = 0; n < scrollBody.List.Count; n++)
                {
                    scrollBody.List[n].value.CloseInput();
                    scrollBody.List[n].Enabled = false;
                    scrollBody.List[n].BlockMember = null;
                }
            }

            waitingForChat = false;
            target = null;
            PropOpen = false;
            index = 0;
            scrollBody.Start = 0;
            Count = 0;
        }

        private class BvPropertyBox : HudElementBase, IListBoxEntry
        {
            public override float Width { get { return layout.Width; } set { } }

            public override float Height { get { return layout.Height; } set { } }

            public override bool Visible => base.Visible && Enabled;
            public bool Enabled { get { return _enabled && (BlockMember != null && BlockMember.Enabled); } set { _enabled = value; } }
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
            private bool _enabled;

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

            protected override void Layout()
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
                public SelectionBox(IHudParent parent = null) : base(parent)
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