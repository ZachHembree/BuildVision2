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

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        private static readonly Color
            headerColor = new Color(41, 54, 62, 225),
            bodyColor = new Color(70, 78, 86, 225),
            selectionBoxColor = new Color(41, 54, 62, 225);
        private static readonly GlyphFormat
            headerText = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            bodyText = new GlyphFormat(Color.White, textSize: .885f),
            valueText = bodyText.WithColor(new Color(210, 210, 210)),
            bodyTextCenter = bodyText.WithAlignment(TextAlignment.Center),
            valueTextCenter = valueText.WithAlignment(TextAlignment.Center),
            footerTextLeft = bodyText.WithColor(new Color(220, 235, 245)),
            footerTextRight = footerTextLeft.WithAlignment(TextAlignment.Right),
            highlightText = bodyText.WithColor(new Color(220, 180, 50)),
            selectedText = bodyText.WithColor(new Color(50, 200, 50)),
            blockIncText = footerTextRight.WithColor(new Color(200, 35, 35));

        private readonly RadialSelectionBox<QuickActionEntry, Label> selectionBox;
        private readonly Body centerElement;
        private readonly ObjectPool<QuickActionEntry> entryPool;

        private int tick;
        private const int textTickDivider = 4;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            selectionBox = new RadialSelectionBox<QuickActionEntry, Label>(this) 
            {
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor
            };

            centerElement = new Body(selectionBox) 
            { };

            entryPool = new ObjectPool<QuickActionEntry>(() => new QuickActionEntry(), x => x.Reset());
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
                BindManager.RequestTempBlacklist(SeBlacklistModes.MouseAndCam);
                selectionBox.IsInputEnabled = !centerElement.IsWidgetOpen;

                if (selectionBox.IsInputEnabled && SharedBinds.LeftButton.IsNewPressed)
                {
                    var selection = selectionBox.Selection;
                    var member = selection?.BlockMember;

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
                        }
                    }
                }
            }
        }

        public void UpdateTarget()
        {
            Clear();

            foreach (IBlockMember blockMember in MenuManager.Target.BlockMembers)
            {
                var entry = entryPool.Get();
                entry.SetMember(blockMember);
                selectionBox.Add(entry);
            }

            selectionBox.IsInputEnabled = true;
        }

        public void Clear()
        {
            entryPool.ReturnRange(selectionBox.EntryList, 0, selectionBox.EntryList.Count);
            selectionBox.Clear();
            selectionBox.IsInputEnabled = false;
        }

        private class QuickActionEntry : ScrollBoxEntry<Label>
        {
            public override bool Enabled
            {
                get { return base.Enabled && BlockMember != null && BlockMember.Enabled; }
            }

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
                    Width = 80f
                });
            }

            public void SetMember(IBlockMember blockMember)
            {
                BlockMember = blockMember;
            }

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