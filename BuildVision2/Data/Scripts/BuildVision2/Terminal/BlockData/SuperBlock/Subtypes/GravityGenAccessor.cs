﻿using RichHudFramework.UI;
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
        private GravityGenAccessor _gravityGen;

        public GravityGenAccessor GravityGen
        {
            get
            {
                return _gravityGen;
            }
            private set
            {
                _gravityGen = value;
            }
        }

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

            private IMyGravityGenerator box;
            private IMyGravityGeneratorSphere sphere;

            public override void SetBlock(SuperBlock block)
            {
                SetBlock(block, TBlockSubtypes.GravityGen);

                if (subtype != null)
                {
                    box = block.TBlock as IMyGravityGenerator;
                    sphere = block.TBlock as IMyGravityGeneratorSphere;
                }
            }

            public override void Reset()
            {
                base.Reset();
                box = null;
                sphere = null;
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityAcceleration)}: ", nameFormat);
                builder.Add($"{Acceleration:G4} m/s²\n", valueFormat);

                if (IsSpherical)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldRadius)}: ", nameFormat);
                    builder.Add($"{TerminalUtilities.GetDistanceDisplay(Radius)}\n", valueFormat);
                }
                else
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldWidth)}: ", nameFormat);
                    builder.Add($"{TerminalUtilities.GetDistanceDisplay(FieldSize.X)}\n", valueFormat);

                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldHeight)}: ", nameFormat);
                    builder.Add($"{TerminalUtilities.GetDistanceDisplay(FieldSize.Y)}\n", valueFormat);

                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GravityFieldDepth)}: ", nameFormat);
                    builder.Add($"{TerminalUtilities.GetDistanceDisplay(FieldSize.Z)}\n", valueFormat);
                }
            }
        }
    }
}