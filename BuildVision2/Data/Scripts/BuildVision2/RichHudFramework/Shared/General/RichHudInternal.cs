using RichHudFramework.Game;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace RichHudFramework
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0)]
    internal sealed class RichHudInternal : ModBase
    {
        public static RichHudInternal Instance { get; private set; }
        public static string MainModName { get { return Instance.ModName; } set { Instance.ModName = value; } }

        public RichHudInternal() : base(false, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of RichHudInternal can exist at any given time.");
        }

        protected override void AfterLoadData()
        { }

        protected override void BeforeClose()
        {
            if (Unloading)
                Instance = null;
        }
    }

    public abstract class InternalComponentBase : ModBase.ComponentBase
    {
        public InternalComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudInternal.Instance)
        { }
    }

    public abstract class InternalParallelComponentBase : ModBase.ParallelComponentBase
    {
        public InternalParallelComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, RichHudInternal.Instance)
        { }
    }
}
