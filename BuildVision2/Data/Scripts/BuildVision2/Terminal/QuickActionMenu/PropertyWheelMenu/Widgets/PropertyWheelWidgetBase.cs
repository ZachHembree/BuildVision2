using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering.Client;
using RichHudFramework.UI.Rendering;
using System;
using System.Reflection.Emit;
using VRageMath;
using VRage;
using VRage.Utils;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private abstract class PropertyWheelWidgetBase : HudElementBase, IClickableElement
        {
            public override bool IsMousedOver => MouseInput.IsMousedOver;

            public bool IsWidgetFocused { get; protected set; }

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
                    Spacing = WidgetInnerPadding,
                    CollectionContainer = { { confirmButton , 1f }, { cancelButton, 1f } }
                };
                buttonChain.Size = buttonChain.GetRangeSize();

                MouseInput = new MouseInputElement(this)
                {
                    ZOffset = 10,
                    DimAlignment = DimAlignments.UnpaddedSize,
                    ShareCursor = true,
                };

                DimAlignment = DimAlignments.UnpaddedSize;
                IsWidgetFocused = false;
            }

            public abstract void SetData(object data, Action CloseWidgetCallback);

            public abstract void Reset();

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (IsMousedOver)
                {
                    if (cancelButton.MouseInput.IsLeftReleased)
                        Cancel();
                    else if (confirmButton.MouseInput.IsLeftReleased)
                        Confirm();
                    else if (MouseInput.IsLeftClicked)
                        IsWidgetFocused = true;
                }
                else
                {
                    if (BvBinds.Cancel.IsNewPressed)
                    {
                        cancelButton.MouseInput.GetInputFocus();
                        IsWidgetFocused = false;
                    }
                    else if (BvBinds.Select.IsNewPressed)
                    {
                        confirmButton.MouseInput.GetInputFocus();
                        IsWidgetFocused = false;
                    }

                    if (!IsWidgetFocused)
                    {
                        if (BvBinds.Cancel.IsReleased)
                            Cancel();
                        else if (BvBinds.Select.IsReleased)
                            Confirm();
                    }
                }                
            }

            protected abstract void Confirm();

            protected abstract void Cancel();

            protected virtual void SetFont(IFontMin newFont)
            {
                if (newFont != null)
                {
                    confirmButton.Format = confirmButton.Format.WithFont(newFont);
                    cancelButton.Format = cancelButton.Format.WithFont(newFont);

                    confirmButton.Text = MyTexts.TrySubstitute("Confirm");
                    cancelButton.Text = MyTexts.TrySubstitute("Cancel");
                }
            }
        }
    }
}