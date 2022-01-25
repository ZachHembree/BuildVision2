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
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed class QuickActionMenu : HudElementBase
    {
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

        private readonly RadialSelectionBox<QuickActionEntry, Label> selectionBox;
        private readonly ObjectPool<QuickActionEntry> entryPool;
        private readonly Label summaryText;
        private readonly RichText summaryBuilder;

        private int tick;
        private const int tickDivider = 4;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            selectionBox = new RadialSelectionBox<QuickActionEntry, Label>(this);
            entryPool = new ObjectPool<QuickActionEntry>(() => new QuickActionEntry(), x => x.Reset());

            summaryText = new Label(selectionBox)
            {
                BuilderMode = TextBuilderModes.Lined,
                AutoResize = false,
                DimAlignment = DimAlignments.Both
            };
            summaryBuilder = new RichText();
        }

        protected override void Layout()
        {
            if (tick == 0)
            {
                foreach (QuickActionEntry entry in selectionBox)
                {
                    entry.UpdateText();
                }

                summaryBuilder.Clear();

                foreach (SuperBlock.SubtypeAccessorBase subtype in MenuManager.Target.SubtypeAccessors)
                {
                    if (subtype != null)
                        subtype.GetSummary(summaryBuilder, bodyText, valueText);
                }

                summaryText.Padding = 1.25f * (1f - selectionBox.polyBoard.InnerRadius) * selectionBox.Size;
                summaryText.TextBoard.SetText(summaryBuilder);
            }

            tick++;
            tick %= tickDivider;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (selectionBox.EntryList.Count > 0)
            {
                if (SharedBinds.LeftButton.IsNewPressed)
                {
                    var selection = selectionBox.Selection;

                    if (selection != null && selection.Enabled)
                    {
                        var blockAction = selection.BlockMember as IBlockAction;
                        blockAction.Action();
                    }
                }
            }
        }

        public void UpdateTarget()
        {
            Clear();

            foreach (IBlockMember blockMember in MenuManager.Target.BlockMembers)
            {
                if (blockMember.Enabled && blockMember is IBlockAction)
                {
                    var entry = entryPool.Get();
                    entry.SetMember(blockMember);
                    selectionBox.Add(entry);
                }
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
                get
                {
                    return base.Enabled && BlockMember != null && BlockMember.Enabled;
                }
            }

            public IBlockMember BlockMember { get; private set; }

            public QuickActionEntry()
            {
                SetElement(new Label() 
                {
                    AutoResize = false,
                    Width = 100f,
                    Height = 200f,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Format = GlyphFormat.White.WithAlignment(TextAlignment.Center)
                });
            }

            public void SetMember(IBlockMember blockMember)
            {
                BlockMember = blockMember;
            }

            public void UpdateText()
            {
                Element.TextBoard.Clear();

                StringBuilder name = BlockMember.Name,
                    disp = BlockMember.Display,
                    status = BlockMember.Status;

                if (name != null)
                {
                    Element.TextBoard.Append(name);

                    if (disp != null || status != null)
                        Element.TextBoard.Append(":");
                }

                if (disp != null)
                {
                    Element.TextBoard.Append(" ");
                    Element.TextBoard.Append(disp);
                }
            }

            public void Reset()
            {
                BlockMember = null;
            }
        }
    }
}