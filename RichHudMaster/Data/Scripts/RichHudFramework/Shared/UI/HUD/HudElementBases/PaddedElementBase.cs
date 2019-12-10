using System;
using VRage;
using VRageMath;

namespace DarkHelmet
{
    namespace UI
    {
        /// <summary>
        /// Base for types of <see cref="HudElementBase"/> that have use padded sizing.
        /// </summary>
        public abstract class PaddedElementBase : HudElementBase
        {
            public override float Width { get { return width + Padding.X; } set { width = value - Padding.X; } }
            public override float Height { get { return height + Padding.Y; } set { height = value - Padding.Y; } }
            public virtual Vector2 Padding { get; set; }

            private float width, height;

            public PaddedElementBase(IHudParent parent) : base(parent)
            { }
        }
    }
}