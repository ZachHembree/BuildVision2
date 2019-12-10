using DarkHelmet.UI.TextHudApi;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Wrapper used to make precise pixel-level manipluation of <see cref="HudAPIv2.HUDMessage"/> easier.
    /// </summary>
    public class TextHudMessage : HudElementBase
    {
        public new Vector2 Size { get; private set; }
        public new float Width { get { return Size.X; } private set { } }
        public new float Height { get { return Size.Y; } private set { } }
        public string Text { get { return text; } set { text = value; UpdateMessage(); } }

        public TextAlignment textAlignment;
        private HudAPIv2.HUDMessage hudMessage;
        private Vector2 alignmentOffset;
        private string text;

        public TextHudMessage(IHudParent parent = null) : base(parent)
        {
            textAlignment = TextAlignment.Center;
        }

        protected override void Draw()
        {
            if (HudAPIv2.Heartbeat)
            {
                if (hudMessage == null)
                {
                    hudMessage = new HudAPIv2.HUDMessage
                    {
                        Blend = BlendTypeEnum.PostPP,
                        Scale = Scale * (1080f / HudMain.ScreenHeight),
                        Options = HudAPIv2.Options.Fixed,
                        Visible = false,
                    };

                    UpdateMessage();
                }

                hudMessage.Scale = Scale * (1080f / HudMain.ScreenHeight);
                UpdateTextOffset();

                Vector2 pos = HudMain.GetNativeVector(Origin + Offset + alignmentOffset);

                hudMessage.Origin = new Vector2D(pos.X, pos.Y);
                hudMessage.Draw();
            }
        }

        private void UpdateMessage()
        {
            if (hudMessage != null && Text != null)
            {
                hudMessage.Message.Clear();
                hudMessage.Message.Append(Text);

                Vector2D textLength = hudMessage.GetTextLength();
                Size = HudMain.GetPixelVector(new Vector2((float)textLength.X, (float)textLength.Y));
            }
        }

        private void UpdateTextOffset()
        {
            Vector2 offset = Size / 2f;
            alignmentOffset = offset;
            alignmentOffset.X *= -1;

            if (textAlignment == TextAlignment.Right)
                alignmentOffset.X -= offset.X;
            else if (textAlignment == TextAlignment.Left)
                alignmentOffset.X += offset.X;
        }
    }
}
