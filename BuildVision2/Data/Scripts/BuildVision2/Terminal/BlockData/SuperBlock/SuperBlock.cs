using Sandbox.ModAPI;
using System;
using VRage.ModAPI;
using VRageMath;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum TBlockSubtypes : int
    {
        Powered = 0x1,
        Battery = 0x2,
        GasTank = 0x4,
        Warhead = 0x8,
        Door = 0x10,
        Parachute = 0x20,
        LandingGear = 0x40,
        Connector = 0x80,
        MechanicalConnection = 0x100,
        Suspension = 0x200,
        Piston = 0x400,
        Rotor = 0x800
    }

    /// <summary>
    /// General purpose terminal block wrapper used to provide easy access to block subtype members.
    /// </summary>
    public partial class SuperBlock
    {
        /// <summary>
        /// Associated terminal block
        /// </summary>
        public IMyTerminalBlock TBlock { get; private set; }

        /// <summary>
        /// Block type identifier. Uses IMyCubeBlock.BlockDefinition.TypeIdString.
        /// </summary>
        public string TypeID { get; }

        /// <summary>
        /// Returns the position of the block in world space.
        /// </summary>
        public Vector3D Position => TBlock != null ? TBlock.GetPosition() : Vector3D.Zero;

        /// <summary>
        /// True if the block integrity is above its breaking threshold
        /// </summary>
        public bool IsFunctional => TBlock != null && TBlock.IsFunctional;

        /// <summary>
        /// True if the block is functional and able to do work.
        /// </summary>
        public bool IsWorking => TBlock != null && TBlock.IsWorking;

        /// <summary>
        /// True if the local player has terminal access permissions
        /// </summary>
        public bool CanLocalPlayerAccess => TBlock != null && TBlock.HasLocalPlayerAccess();

        #region SubtypeAccessors
        /// <summary>
        /// Provides access to block power information, if defined.
        /// </summary>
        public PowerData Power { get; private set; }

        /// <summary>
        /// Provides access to block battery information, if defined.
        /// </summary>
        public BatteryData Battery { get; private set; }

        /// <summary>
        /// Provides access to block tank information, if defined.
        /// </summary>
        public GasTankData GasTank { get; private set; }

        /// <summary>
        /// Provides access to block warhead information, if defined.
        /// </summary>
        public WarheadData Warhead { get; private set; }

        /// <summary>
        /// Provides access to block door information, if defined.
        /// </summary>
        public DoorData Door { get; private set; }

        /// <summary>
        /// Provides access to block landing gear information, if defined.
        /// </summary>
        public LandingGearData LandingGear { get; private set; }

        /// <summary>
        /// Provides access to block connector information, if defined.
        /// </summary>
        public ConnectorData Connector { get; private set; }

        /// <summary>
        /// Provides access to mechanical connection block information, if defined.
        /// </summary>
        public MechConnectionData MechConnection { get; private set; }

        /// <summary>
        /// Provides access to block piston information, if defined.
        /// </summary>
        public PistonData Piston { get; private set; }

        /// <summary>
        /// Provides access to block rotor information, if defined.
        /// </summary>
        public RotorData Rotor { get; private set; }
        #endregion

        public TBlockSubtypes SubtypeId { get; private set; }

        public SuperBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            TypeID = tBlock.BlockDefinition.TypeIdString;
            TBlock.OnMarkForClose += BlockClosing;

            AddBlockSubtypes();
        }

        /// <summary>
        /// Clears all references to the <see cref="IMyTerminalBlock"/> held by the <see cref="SuperBlock"/>.
        /// </summary>
        private void BlockClosing(IMyEntity entity)
        {
            TBlock = null;
            Power = null;
            Battery = null;
            GasTank = null;
            Warhead = null;
            Door = null;
            LandingGear = null;
            Connector = null;
            MechConnection = null;
            Piston = null;
            Rotor = null;
            SubtypeId = 0;
        }

        private void AddBlockSubtypes()
        {
            if (TBlock.ResourceSink != null || TBlock is IMyPowerProducer)
            {
                SubtypeId |= TBlockSubtypes.Powered;
                Power = new PowerData(TBlock);

                if (TBlock is IMyBatteryBlock)
                {
                    SubtypeId |= TBlockSubtypes.Battery;
                    Battery = new BatteryData(TBlock);
                }
            }

            if (TBlock is IMyGasTank)
            {
                SubtypeId |= TBlockSubtypes.GasTank;
                GasTank = new GasTankData(TBlock);
            }

            if (TBlock is IMyWarhead)
            {
                SubtypeId |= TBlockSubtypes.Warhead;
                Warhead = new WarheadData(TBlock);
            }

            if (TBlock is IMyDoor)
            {
                SubtypeId |= TBlockSubtypes.Door;
                Door = new DoorData(TBlock);
            }

            if (TBlock is IMyParachute)
            {
                SubtypeId |= TBlockSubtypes.Parachute;
            }

            if (TBlock is IMyLandingGear)
            {
                SubtypeId |= TBlockSubtypes.LandingGear;
                LandingGear = new LandingGearData(TBlock);
            }

            if (TBlock is IMyShipConnector)
            {
                SubtypeId |= TBlockSubtypes.Connector;
                Connector = new ConnectorData(TBlock);
            }

            if (TBlock is IMyMechanicalConnectionBlock)
            {
                SubtypeId |= TBlockSubtypes.MechanicalConnection;
                MechConnection = new MechConnectionData(TBlock);

                if (TBlock is IMyMotorSuspension)
                    SubtypeId |= TBlockSubtypes.Suspension;

                if (TBlock is IMyPistonBase)
                {
                    SubtypeId |= TBlockSubtypes.Piston;
                    Piston = new PistonData(TBlock);
                }

                if (TBlock is IMyMotorStator)
                {
                    SubtypeId |= TBlockSubtypes.Rotor;
                    Rotor = new RotorData(TBlock);
                }
            }
        }
    }
}