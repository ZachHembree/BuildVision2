using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class ComboWidget : BlockValueWidgetBase
        {
            private readonly Dropdown<KeyValuePair<long, StringBuilder>> comboBox;

            private IBlockComboBox blockComboMember;
            private long initValue;

            public ComboWidget(HudParentBase parent = null) : base(parent)
            {
                comboBox = new Dropdown<KeyValuePair<long, StringBuilder>>() 
                {
                    DropdownHeight = 0f,
                    MinVisibleCount = 4,
                };

                var layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        comboBox,
                        buttonChain
                    }
                };
            }

            public override void SetData(object member, Action CloseWidgetCallback)
            {
                blockComboMember = member as IBlockComboBox;
                this.CloseWidgetCallback = CloseWidgetCallback;

                foreach (var entry in blockComboMember.ComboEntries)
                    comboBox.Add(entry.Value, entry);

                initValue = blockComboMember.Value;
                int selectedIndex = comboBox.HudCollection.FindIndex(x => x.AssocMember.Key == initValue);
                comboBox.SetSelectionAt(selectedIndex);
            }

            public override void Reset()
            {
                blockComboMember = null;
                CloseWidgetCallback = null;
                comboBox.ClearEntries();
            }

            protected override void Confirm()
            {
                if (comboBox.Selection != null)
                    blockComboMember.Value = comboBox.Selection.AssocMember.Key;

                CloseWidgetCallback();
            }

            protected override void Cancel()
            {
                CloseWidgetCallback();
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                base.HandleInput(cursorPos); 
                
                if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollDown.IsNewPressed)
                {
                    int index = comboBox.SelectionIndex;

                    if (BvBinds.ScrollUp.IsNewPressed)
                        index++;
                    else if (BvBinds.ScrollDown.IsNewPressed)
                        index--;

                    comboBox.SetSelectionAt(index);
                }
            }
        }
    }
}