using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using VRageMath;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block light members, if defined.
        /// </summary>
        public LightAccessor Light  { get { return _light; } private set { _light = value; } }

        private LightAccessor _light;

        public class LightAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Controls the light's color
            /// </summary>
            public Color Color 
            { 
                get { return subtype?.Color ?? component.Color; } 
                set 
                {
                    if (subtype != null)
                        subtype.Color = value;
                    else
                        component.Color = value;
                }
            }

            /// <summary>
            /// Controls the light's intensity
            /// </summary>
            public float Intensity 
            { 
                get { return subtype?.Intensity ?? component.Intensity; } 
                set 
                { 
                    if (subtype != null)
                        subtype.Intensity = value;
                    else
                        component.Intensity = value;
                } 
            }

            /// <summary>
            /// Controls the lighting radius
            /// </summary>
            public float Radius 
            { 
                get { return subtype?.Radius ?? component.Radius; } 
                set 
                { 
                    if (subtype != null)
                        subtype.Radius = value;
                    else
                        component.Radius = value;
                } 
            }

            private IMyLightingBlock subtype;
            private IMyLightingComponent component;

            public override void SetBlock(SuperBlock block)
            {
                this.block = block;
				subtype = block.TBlock as IMyLightingBlock;
				this.SubtypeId = TBlockSubtypes.Light;

                // TBD: No terminal controls for lighting components yet, and enabling
                // this clutters the summaries on affected blocks.
				if (subtype != null)// || block.TBlock.Components.TryGet(out component))
				{
					block.SubtypeId |= TBlockSubtypes.Light;
					block.subtypeAccessors.Add(this);
				}
            }

			public override void Reset()
			{
				base.Reset();
                subtype = null;
                component = null;
			}

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                // Color packed into one line
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightColor), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add("R: ", nameFormat);

                block.textBuffer.Clear();
                block.textBuffer.Append(Color.R);
                block.textBuffer.Append(" ");
                builder.Add(block.textBuffer, valueFormat); 

                builder.Add("G: ", nameFormat);

                block.textBuffer.Clear();
                block.textBuffer.Append(Color.G);
                block.textBuffer.Append(" ");
                builder.Add(block.textBuffer, valueFormat);

                builder.Add("B: ", nameFormat);

                block.textBuffer.Clear();
                block.textBuffer.Append(Color.B);
                block.textBuffer.Append("\n");
                builder.Add(block.textBuffer, valueFormat);

                // Light radius
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightRadius), nameFormat);
                builder.Add(": ", nameFormat);

                block.textBuffer.Clear();
                TerminalUtilities.GetDistanceDisplay(Radius, block.textBuffer);
                block.textBuffer.Append('\n');

                builder.Add(block.textBuffer, valueFormat);

                // Light intensity
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightIntensity), nameFormat);
                builder.Add(": ", nameFormat);

                block.textBuffer.Clear();
                block.textBuffer.Append(Math.Round(Intensity, 2));
                block.textBuffer.Append('\n');

                builder.Add(block.textBuffer, valueFormat);
            }
        }
    }
}