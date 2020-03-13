using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace RichHudFramework.Internal
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0)]
    public sealed class RichHudCore : ModBase
    {
        public static RichHudCore Instance { get; private set; }

        public RichHudCore() : base(false, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of RichHudCore can exist at any given time.");
        }

        protected override void AfterLoadData()
        { }

        public override void Close()
        {
            base.Close();

            if (ExceptionHandler.Unloading)
                Instance = null;
        }       
    }

    public abstract class RichHudComponentBase : ModBase.ComponentBase
    {
        public RichHudComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudCore.Instance)
        { }
    }

    public abstract class RichHudParallelComponentBase : ModBase.ParallelComponentBase
    {
        public RichHudParallelComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudCore.Instance)
        { }
    }
}