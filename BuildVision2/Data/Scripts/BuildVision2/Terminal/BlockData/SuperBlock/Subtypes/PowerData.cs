using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using System;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class PowerData
        {
            /// <summary>
            /// Returns block power input in megawatts if it has a <see cref="MyResourceSinkComponentBase"/>.
            /// </summary>
            public float Input => sink != null ? sink.CurrentInputByType(resourceId) : -1f;

            /// <summary>
            /// Returns block power output in megawatts if the underlying fat block implements <see cref="IMyPowerProducer"/>.
            /// </summary>
            public float Out => powerProducer != null ? powerProducer.CurrentOutput : -1f;

            private readonly MyDefinitionId resourceId;
            private readonly MyResourceSinkComponentBase sink;
            private readonly IMyPowerProducer powerProducer;

            public PowerData(IMyTerminalBlock tBlock)
            {
                resourceId = MyDefinitionId.FromContent(tBlock.SlimBlock.GetObjectBuilder());
                sink = tBlock.ResourceSink;
                powerProducer = tBlock as IMyPowerProducer;
            }

            /// <summary>
            /// Returns a one-line summary of the power input/output in the format -Input/+Output
            /// </summary>
            public string GetPowerDisplay()
            {
                float scale;
                string suffix, disp = "";
                TerminalExtensions.GetPowerScale(Math.Max(Input, 0f) + Math.Max(Out, 0f), out scale, out suffix);

                if (Input >= 0f)
                    disp += "-" + (Input * scale).ToString("G4");

                if (Out >= 0f)
                {
                    if (Input >= 0f)
                        disp += " / ";

                    disp += "+" + (Out * scale).ToString("G4");
                }

                return $"{disp} {suffix}";
            }
        }
    }
}