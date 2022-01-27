using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        /// <summary>
        /// Returns the current state of the menu
        /// </summary>
        public QuickActionMenuState MenuState { get; private set; }

        public static bool DrawDebug { get; set; }

        private readonly RadialSelectionBox<QuickActionEntry, Label> selectionBox;
        private readonly Body centerElement;
        private readonly ObjectPool<QuickActionEntry> entryPool;

        private readonly Label debugText;

        private int tick;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            selectionBox = new RadialSelectionBox<QuickActionEntry, Label>(this) 
            {
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding
            };

            centerElement = new Body(selectionBox) 
            { };

            entryPool = new ObjectPool<QuickActionEntry>(() => new QuickActionEntry(), x => x.Reset());

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };

            Size = new Vector2(512f);
        }

        protected override void Layout()
        {
            Vector2 size = cachedSize - cachedPadding;
            centerElement.Size = 1.05f * selectionBox.Size * selectionBox.polyBoard.InnerRadius;

            if (tick == 0)
            {
                foreach (QuickActionEntry entry in selectionBox)
                {
                    entry.UpdateText();
                }
            }

            tick++;
            tick %= textTickDivider;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (selectionBox.EntryList.Count > 0)
            {
                BindManager.RequestTempBlacklist(SeBlacklistModes.Mouse);

                if (MenuState == QuickActionMenuState.RadialSelect)
                {
                    var selection = selectionBox.Selection;
                    var member = selection?.BlockMember;
                    selectionBox.IsInputEnabled = BvBinds.EnableMouse.IsPressed;

                    if (BvBinds.EnableMouse.IsPressed)
                        BindManager.RequestTempBlacklist(SeBlacklistModes.MouseAndCam);

                    if (BvBinds.Select.IsReleased)
                    {
                        if (member != null && selection.Enabled)
                        {
                            if (member is IBlockAction)
                            {
                                var blockAction = member as IBlockAction;
                                blockAction.Action();
                            }
                            else
                            {
                                centerElement.OpenMemberWidget(member);
                                selectionBox.IsInputEnabled = false;
                                MenuState = QuickActionMenuState.WidgetControl;
                            }
                        }
                    }
                    else if (BvBinds.ScrollUp.IsPressed)
                    {
                        ScrollSelection(1);
                    }
                    else if (BvBinds.ScrollDown.IsPressed)
                    {
                        ScrollSelection(-1);
                    }
                }
                else if (MenuState == QuickActionMenuState.WidgetControl)
                {
                    HudMain.EnableCursor = BvBinds.EnableMouse.IsPressed;
                }

                // Switch back to radial list after the widget is closed
                if (MenuState == QuickActionMenuState.WidgetControl && !centerElement.IsWidgetOpen)
                {
                    MenuState = QuickActionMenuState.RadialSelect;
                }
            }

            if (DrawDebug)
            {
                debugText.Visible = true;

                ITextBuilder debugBuilder = debugText.TextBoard;
                debugBuilder.Clear();
                debugBuilder.Append($"State: {MenuState}\n");
                debugBuilder.Append($"IsWidgetOpen: {centerElement.IsWidgetOpen}\n");
                debugBuilder.Append($"Wheel Input Enabled: {selectionBox.IsInputEnabled}\n");
                debugBuilder.Append($"Cursor Mode: {HudMain.InputMode}\n");
                debugBuilder.Append($"Blacklist Mode: {BindManager.BlacklistMode}\n");
                debugBuilder.Append($"Enable Cursor Pressed: {BvBinds.EnableMouse.IsPressed}\n");
            }
            else
                debugText.Visible = false;
        }

        /// <summary>
        /// Offsets scroll wheel selection in the direction of the given offset to the next enabled entry, 
        /// with the magnitude determining the number of steps. Wraps around.
        /// </summary>
        private void ScrollSelection(int offset)
        {
            int index = selectionBox.SelectionIndex,
                dir = offset > 0 ? 1 : -1,
                absOffset = Math.Abs(offset);

            if (dir > 0)
            {
                for (int i = 0; i < absOffset; i++)
                {
                    index = (index + dir) % selectionBox.Count;
                    index = FindFirstEnabled(index);
                }
            }
            else
            {
                for (int i = 0; i < absOffset; i++)
                {
                    index = (index + dir) % selectionBox.Count;

                    if (index < 0)
                        index += selectionBox.Count;

                    index = FindLastEnabled(index);
                }
            }

            selectionBox.SetSelectionAt(index);
        }

        /// <summary>
        /// Returns first enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindFirstEnabled(int index)
        {
            int j = index;

            for (int n = 0; n < 2 * selectionBox.Count; n++)
            {
                if (selectionBox.EntryList[j].Enabled)
                    break;

                j++;
                j %= selectionBox.Count;
            }

            return j;
        }

        /// <summary>
        /// Returns preceeding enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindLastEnabled(int index)
        {
            int j = index;

            for (int n = 0; n < 2 * selectionBox.Count; n++)
            {
                if (selectionBox.EntryList[j].Enabled)
                    break;

                j--;
                j %= selectionBox.Count;
            }

            return j;
        }

        /// <summary>
        /// Opens the menu and populates it with properties from the given property block
        /// </summary>
        public void OpenMenu(PropertyBlock block)
        {
            Clear();

            foreach (IBlockMember blockMember in block.BlockMembers)
            {
                var entry = entryPool.Get();
                entry.SetMember(blockMember);
                selectionBox.Add(entry);
            }

            selectionBox.IsInputEnabled = true;
            MenuState = QuickActionMenuState.RadialSelect;
            Visible = true;
        }

        /// <summary>
        /// Closes and resets the menu
        /// </summary>
        public void CloseMenu()
        {
            Clear();
            Visible = false;
            MenuState = QuickActionMenuState.Closed;
        }

        /// <summary>
        /// Clears all entries from the menu and closes any open widgets
        /// </summary>
        private void Clear()
        {
            centerElement.CloseWidget();
            entryPool.ReturnRange(selectionBox.EntryList, 0, selectionBox.EntryList.Count);
            selectionBox.Clear();
            selectionBox.IsInputEnabled = false;
        }

        /// <summary>
        /// Custom scroll box container for <see cref="IBlockMember"/>
        /// </summary>
        private class QuickActionEntry : ScrollBoxEntry<Label>
        {
            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && BlockMember != null && BlockMember.Enabled; }
            }

            /// <summary>
            /// Returns associated property member wrapper
            /// </summary>
            public IBlockMember BlockMember { get; private set; }

            private readonly RichText textBuf;

            public QuickActionEntry()
            {
                textBuf = new RichText();
                SetElement(new Label() 
                {
                    VertCenterText = true,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Padding = new Vector2(20f),
                    Width = 90f
                });
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(IBlockMember blockMember)
            {
                BlockMember = blockMember;
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public void UpdateText()
            {
                StringBuilder name = BlockMember.Name,
                    disp = BlockMember.FormattedValue,
                    status = BlockMember.StatusText;

                textBuf.Clear();

                if (name != null)
                {
                    textBuf.Add(name, bodyTextCenter);

                    if (disp != null || status != null)
                        textBuf.Add(":\n", bodyTextCenter);
                }

                if (disp != null)
                {
                    textBuf.Add(' ', valueTextCenter);
                    textBuf.Add(disp, valueTextCenter);
                }

                Element.Text = textBuf;
            }

            public void Reset()
            {
                BlockMember = null;
            }
        }
    }
}