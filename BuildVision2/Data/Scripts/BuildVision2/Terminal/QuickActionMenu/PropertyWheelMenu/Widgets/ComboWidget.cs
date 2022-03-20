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

                layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
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
                comboBox.SetSelectionAt((int)blockComboMember.Value);
                comboBox.MouseInput.GetInputFocus();
            }

            public override void Reset()
            {
                CloseWidgetCallback = null;

                Confirm();
                blockComboMember = null;
                comboBox.ClearEntries();
            }

            protected override void Confirm()
            {
                if (comboBox.SelectionIndex != -1)
                    blockComboMember.Value = comboBox.SelectionIndex;

                comboBox.CloseList();
                CloseWidgetCallback?.Invoke();
            }

            protected override void Cancel()
            {
                comboBox.SetSelectionAt((int)initValue);
                Confirm();
            }

            protected override void Layout()
            {
                Height = _parentFull.Height + cachedPadding.Y;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (!comboBox.Open && blockComboMember != null)
                {
                    if (BvBinds.ScrollUp.IsPressed || BvBinds.ScrollDown.IsPressed)
                    {
                        int index = comboBox.SelectionIndex;

                        if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                            index++;
                        else if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                            index--;

                        comboBox.SetSelectionAt(index);                      
                    }

                    blockComboMember.Value = comboBox.SelectionIndex;
                    base.HandleInput(cursorPos);
                }
            }
        }
    }
}