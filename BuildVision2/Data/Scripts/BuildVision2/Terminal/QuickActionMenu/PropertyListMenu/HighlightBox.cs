﻿using RichHudFramework.UI;
using VRageMath;
using RichHudFramework.UI.Rendering;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private partial class PropertyListMenu
        {
            /// <summary>
            /// A textured box with a white tab positioned on the left hand side.
            /// </summary>
            private class HighlightBox : TexturedBox
            {
                public bool CanDrawTab { get; set; }

                public Color TabColor { get { return tabBoard.Color; } set { tabBoard.Color = value; } }

                private readonly MatBoard tabBoard;

                public HighlightBox(HudParentBase parent = null) : base(parent)
                {
                    tabBoard = new MatBoard() { Color = TerminalFormatting.Mercury };
                    Color = TerminalFormatting.Atomic;
                    CanDrawTab = true;
                    IsSelectivelyMasked = true;
                }

                protected override void Draw()
                {
                    CroppedBox box = default(CroppedBox);
                    Vector2 size = (cachedSize - cachedPadding),
                        halfSize = size * .5f;

                    box.bounds = new BoundingBox2(cachedPosition - halfSize, cachedPosition + halfSize);
                    box.mask = maskingBox;

                    if (hudBoard.Color.A > 0)
                        hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                    // Left align the tab
                    Vector2 tabPos = cachedPosition,
                        tabSize = new Vector2(4f, size.Y - cachedPadding.Y);
                    tabPos.X += (-size.X + tabSize.X) * .5f;
                    tabSize *= .5f;

                    if (CanDrawTab && tabBoard.Color.A > 0)
                    {
                        box.bounds = new BoundingBox2(tabPos - tabSize, tabPos + tabSize);
                        tabBoard.Draw(ref box, HudSpace.PlaneToWorldRef);
                    }
                }
            }
        }
    }
}