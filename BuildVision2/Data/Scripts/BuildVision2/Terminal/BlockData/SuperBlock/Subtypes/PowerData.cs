using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

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
        }
    }
}