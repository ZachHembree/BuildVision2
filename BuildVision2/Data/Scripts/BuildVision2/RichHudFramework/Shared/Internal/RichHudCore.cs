using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace RichHudFramework.Internal
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
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

        public override void Draw()
        {
            // It seems there's some kind of bug in the game's session component system that prevents the Before/Sim/After
            // update methods from being called on more than one component with the same fully qualified name, update order
            // and priority, but for some reason, Draw and HandleInput still work.
            //
            // It would be really nice if I didn't have to work around this issue like this, but here we are.         
            BeforeUpdate();
            base.Draw();
        }

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