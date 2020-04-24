using Sandbox.ModAPI;
using System;
using VRage.ModAPI;
using VRageMath;
using RichHudFramework.UI;
using System.Collections.Generic;
using IMyAirVent = SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.Ingame.IMyLandingGear;

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
        Rotor = 0x800,
        Inventory = 0x1000,
        Turret = 0x2000,
        Light = 0x4000,
        JumpDrive = 0x8000,
        Thruster = 0x10000,
        LaserAntenna = 0x20000,
        RadioAntenna = 0x40000,
        OreDetector = 0x80000,
        AirVent = 0x100000,
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

        /// <summary>
        /// Indicates the subtypes supported by the block.
        /// </summary>
        public TBlockSubtypes SubtypeId { get; private set; }

        /// <summary>
        /// List of subtype accessors
        /// </summary>
        public IReadOnlyList<SubtypeAccessorBase> Subtypes => subtypes;

        private readonly List<SubtypeAccessorBase> subtypes;

        public SuperBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            TypeID = tBlock.BlockDefinition.TypeIdString;
            TBlock.OnMarkForClose += BlockClosing;
            subtypes = new List<SubtypeAccessorBase>();

            AddBlockSubtypes();
        }

        /// <summary>
        /// Clears all references to the <see cref="IMyTerminalBlock"/> held by the <see cref="SuperBlock"/>.
        /// </summary>
        private void BlockClosing(IMyEntity entity)
        {
            TBlock = null;
            SubtypeId = 0;
        }

        private void AddBlockSubtypes()
        {
            if (TBlock.ResourceSink != null || TBlock is IMyPowerProducer)
            {
                Power = new PowerAccessor(this);

                if (TBlock is IMyBatteryBlock)
                    Battery = new BatteryAccessor(this);
            }

            if (TBlock is IMyGasTank)
                GasTank = new GasTankAccessor(this);

            if (TBlock is IMyWarhead)
                Warhead = new WarheadAccessor(this);

            if (TBlock is IMyDoor)
                Door = new DoorAccessor(this);

            if (TBlock is IMyLandingGear)
                LandingGear = new LandingGearAccessor(this);

            if (TBlock is IMyShipConnector)
                Connector = new ConnectorAccessor(this);

            if (TBlock is IMyMechanicalConnectionBlock)
            {
                MechConnection = new MechConnectionAccessor(this);

                if (TBlock is IMyPistonBase)
                    Piston = new PistonAccessor(this);

                if (TBlock is IMyMotorStator)
                    Rotor = new RotorAccessor(this);
            }

            if (TBlock.HasInventory)
                Inventory = new InventoryAccessor(this);

            if (TBlock is IMyLargeTurretBase)
                Turret = new TurretAccessor(this);

            if (TBlock is IMyLightingBlock)
                Light = new LightAccessor(this);

            if (TBlock is IMyJumpDrive)
                JumpDrive = new JumpDriveAccessor(this);

            if (TBlock is IMyThrust)
                Thruster = new ThrusterAccessor(this);

            if (TBlock is IMyLaserAntenna)
                LaserAntenna = new LaserAntennaAccessor(this);

            if (TBlock is IMyRadioAntenna)
                RadioAntenna = new RadioAntennaAccessor(this);

            if (TBlock is IMyOreDetector)
                OreDetector = new OreDetectorAccessor(this);

            if (TBlock is IMyAirVent)
                AirVent = new AirVentAccessor(this);
        }

        public abstract class SubtypeAccessorBase
        {
            public readonly TBlockSubtypes subtype;

            protected SuperBlock block;

            protected SubtypeAccessorBase(SuperBlock block, TBlockSubtypes subtype)
            {
                this.block = block;
                this.subtype = subtype;
                block.SubtypeId |= subtype;
                block.subtypes.Add(this);
            }

            public abstract RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat);
        }
    }
}