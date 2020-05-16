using RichHudFramework.UI;
using VRageMath;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyGravityGeneratorBase = SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase;
using IMyGravityGenerator = SpaceEngineers.Game.ModAPI.IMyGravityGenerator;
using IMyGravityGeneratorSphere = SpaceEngineers.Game.ModAPI.IMyGravityGeneratorSphere;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public GravityGenAccessor GravityGen { get; private set; }

        public class GravityGenAccessor : SubtypeAccessor<IMyGravityGeneratorBase>
        {
            public float Acceleration { get { return subtype.GravityAcceleration; } set { subtype.GravityAcceleration = value; } }

            public Vector3 FieldSize 
            { 
                get { return box?.FieldSize ?? Vector3.Zero; } 
                set { if (box != null) box.FieldSize = value; } 
            }

            public float Radius 
            {
                get { return sphere?.Radius ?? 0f; }
                set { if (sphere != null) sphere.Radius = value; }
            }

            public bool IsSpherical => sphere != null;

            private readonly IMyGravityGenerator box;
            private readonly IMyGravityGeneratorSphere sphere;

            public GravityGenAccessor(SuperBlock block) : base(block, TBlockSubtypes.GravityGen)
            {
                if (subtype != null)
                {
                    box = block.TBlock as IMyGravityGenerator;
                    sphere = block.TBlock as IMyGravityGeneratorSphere;
                }
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityAcceleration)}: ", nameFormat },
                    { $"{Acceleration.ToString("G4")} m/s²\n", valueFormat }
                };

                if (IsSpherical)
                {
                    summary.Add(new RichText 
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldRadius)}: ", nameFormat },
                        { $"{TerminalUtilities.GetDistanceDisplay(Radius)}\n", valueFormat },
                    });
                }
                else
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldWidth)}: ", nameFormat },
                        { $"{TerminalUtilities.GetDistanceDisplay(FieldSize.X)}\n", valueFormat },

                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldHeight)}: ", nameFormat },
                        { $"{TerminalUtilities.GetDistanceDisplay(FieldSize.Y)}\n", valueFormat },

                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldDepth)}: ", nameFormat },
                        { $"{TerminalUtilities.GetDistanceDisplay(FieldSize.Z)}\n", valueFormat },
                    });
                }

                return summary;
            }
        }
    }
}