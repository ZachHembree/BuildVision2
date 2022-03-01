﻿using RichHudFramework.UI;
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
                comboBox.SetSelectionAt((int)blockComboMember.Value);
                comboBox.MouseInput.GetInputFocus();
            }

            public override void Reset()
            {
                blockComboMember = null;
                CloseWidgetCallback = null;
                comboBox.ClearEntries();
            }

            protected override void Confirm()
            {
                if (comboBox.SelectionIndex != -1)
                    blockComboMember.Value = comboBox.SelectionIndex;

                CloseWidgetCallback();
            }

            protected override void Cancel()
            {
                blockComboMember.Value = initValue;
                CloseWidgetCallback();
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                base.HandleInput(cursorPos);

                if (BvBinds.ScrollUp.IsPressed || BvBinds.ScrollDown.IsPressed)
                {
                    int index = comboBox.SelectionIndex;

                    if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                        index++;
                    else if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                        index--;

                    comboBox.SetSelectionAt(index);
                }
            }
        }
    }
}