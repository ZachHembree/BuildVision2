using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private abstract class PropertyWheelWidgetBase : HudElementBase, IClickableElement
        {
            public override bool IsMousedOver => MouseInput.IsMousedOver;

            public IMouseInput MouseInput { get; protected set; }

            protected readonly BorderedButton cancelButton, confirmButton;
            protected readonly HudChain buttonChain;
            protected HudChain layout;

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

                MouseInput = new MouseInputElement(this)
                {
                    ZOffset = 10,
                    DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                    ShareCursor = true,
                };
            }

            public abstract void SetData(object data, Action CloseWidgetCallback);

            public abstract void Reset();

            protected override void Layout()
            {
                Height = layout.Height + cachedPadding.Y;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (IsMousedOver)
                {
                    if (cancelButton.MouseInput.IsLeftReleased)
                        Cancel();
                    else if (confirmButton.MouseInput.IsLeftReleased)
                        Confirm();
                }
                else
                {
                    if (BvBinds.Cancel.IsNewPressed)
                        cancelButton.MouseInput.GetInputFocus();
                    else if (BvBinds.Select.IsNewPressed)
                        confirmButton.MouseInput.GetInputFocus();

                    if (BvBinds.Cancel.IsReleased)
                        Cancel();
                    else if (BvBinds.Select.IsReleased)
                        Confirm();
                }                
            }

            protected abstract void Confirm();

            protected abstract void Cancel();
        }
    }
}