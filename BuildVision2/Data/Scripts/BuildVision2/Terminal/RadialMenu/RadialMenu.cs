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
        private readonly RadialSelectionBox<QuickActionEntry, Label> selectionBox;
        private readonly ObjectPool<QuickActionEntry> entryPool;

        private int tick;
        private const int tickDivider = 4;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            selectionBox = new RadialSelectionBox<QuickActionEntry, Label>(this) { IsInputEnabled = true };
            entryPool = new ObjectPool<QuickActionEntry>(() => new QuickActionEntry(), x => x.Reset());   
        }

        protected override void Layout()
        {
            if (tick == 0)
            {
                foreach (QuickActionEntry entry in selectionBox)
                {
                    entry.UpdateText();
                }
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
        }

        public void Clear()
        {
            entryPool.ReturnRange(selectionBox.EntryList, 0, selectionBox.EntryList.Count);
            selectionBox.Clear();
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
                    Element.TextBoard.Append(name);

                if (disp != null)
                {
                    Element.TextBoard.Append(" ");
                    Element.TextBoard.Append(disp);
                }

                if (status != null)
                {
                    Element.TextBoard.Append(" ");
                    Element.TextBoard.Append(status);
                }
            }

            public void Reset()
            {
                BlockMember = null;
            }
        }
    }
}