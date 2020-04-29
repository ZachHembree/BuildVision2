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

        public class GravityGenAccessor : SubtypeAccessorBase
        {
            public float Acceleration { get { return gravGen.GravityAcceleration; } set { gravGen.GravityAcceleration = value; } }

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

            private readonly IMyGravityGeneratorBase gravGen;
            private readonly IMyGravityGenerator box;
            private readonly IMyGravityGeneratorSphere sphere;

            public GravityGenAccessor(SuperBlock block) : base(block, TBlockSubtypes.GravityGen)
            {
                gravGen = block.TBlock as IMyGravityGeneratorBase;
                box = block.TBlock as IMyGravityGenerator;
                sphere = block.TBlock as IMyGravityGeneratorSphere;
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
                        { $"{TerminalExtensions.GetDistanceDisplay(Radius)}\n", valueFormat },
                    });
                }
                else
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldWidth)}: ", nameFormat },
                        { $"{TerminalExtensions.GetDistanceDisplay(FieldSize.X)}\n", valueFormat },

                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldHeight)}: ", nameFormat },
                        { $"{TerminalExtensions.GetDistanceDisplay(FieldSize.Y)}\n", valueFormat },

                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldDepth)}: ", nameFormat },
                        { $"{TerminalExtensions.GetDistanceDisplay(FieldSize.Z)}\n", valueFormat },
                    });
                }

                return summary;
            }
        }
    }
}