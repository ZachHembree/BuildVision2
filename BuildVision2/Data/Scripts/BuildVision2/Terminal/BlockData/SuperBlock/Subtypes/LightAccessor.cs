using Sandbox.ModAPI;
using VRageMath;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block light members, if defined.
        /// </summary>
        public LightAccessor Light { get; private set; }

        public class LightAccessor : SubtypeAccessor<IMyLightingBlock>
        {
            /// <summary>
            /// Controls the light's color
            /// </summary>
        public Color Color { get { return subtype.Color; } set { subtype.Color = value; } }

            /// <summary>
            /// Controls the light's intensity
            /// </summary>
            public float Intensity { get { return subtype.Intensity; } set { subtype.Intensity = value; } }

            /// <summary>
            /// Controls the light's falloff
            /// </summary>
            public float Falloff { get { return subtype.Falloff; } set { subtype.Falloff = value; } }

            /// <summary>
            /// Controls the lighting radius
            /// </summary>
            public float Radius { get { return subtype.Radius; } set { subtype.Radius = value; } }

            public LightAccessor(SuperBlock block) : base(block, TBlockSubtypes.Light)
            { }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightColor)}: ", nameFormat },
                    { $"R: ", nameFormat }, { $"{Color.R} ", valueFormat }, 
                    { $"G: ", nameFormat }, { $"{Color.G} ", valueFormat }, 
                    { $"B: ", nameFormat }, { $"{Color.B}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightRadius)}: ", nameFormat },
                    { $"{TerminalUtilities.GetDistanceDisplay(Radius)}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightFalloff)}: ", nameFormat },
                    { $"{Falloff.Round(2)}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LightIntensity)}: ", nameFormat },
                    { $"{Intensity.Round(2)}\n", valueFormat },
                };
            }
        }
    }
}