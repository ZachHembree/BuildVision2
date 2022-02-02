using RichHudFramework.UI;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private abstract class PropertyWheelWidgetBase : HudElementBase
        {
            protected readonly BorderedButton cancelButton, confirmButton;
            protected readonly HudChain buttonChain;

            protected Action CloseWidgetCallback;

            public PropertyWheelWidgetBase(HudParentBase parent = null) : base(parent)
            {
                confirmButton = new BorderedButton()
                {
                    Text = "Confirm",
                    Height = 40f,
                    Width = 150f,
                    Padding = Vector2.Zero,
                    TextPadding = Vector2.Zero,
                };
                cancelButton = new BorderedButton()
                {
                    Text = "Cancel",
                    Height = 40f,
                    Width = 150f,
                    Padding = Vector2.Zero,
                    TextPadding = Vector2.Zero,
                };

                buttonChain = new HudChain(false)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                    Spacing = 8f,
                    CollectionContainer = { confirmButton, cancelButton }
                };

                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding;
            }

            public abstract void SetData(object data, Action CloseWidgetCallback);

            public abstract void Reset();

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (cancelButton.MouseInput.IsLeftReleased || BvBinds.Cancel.IsReleased)
                {
                    Cancel();
                }
                else if (confirmButton.MouseInput.IsLeftReleased ||
                    (BvBinds.Select.IsReleased && !BvBinds.EnableMouse.IsPressed))
                {
                    Confirm();
                }
            }

            protected abstract void Confirm();

            protected abstract void Cancel();
        }
    }
}