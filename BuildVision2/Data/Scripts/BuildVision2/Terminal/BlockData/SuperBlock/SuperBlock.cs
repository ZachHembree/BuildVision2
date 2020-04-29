using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.ModAPI;
using VRageMath;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyAirVent = SpaceEngineers.Game.ModAPI.IMyAirVent;
using IMyGunBaseUser = Sandbox.Game.Entities.IMyGunBaseUser;
using IMyLandingGear = SpaceEngineers.Game.ModAPI.IMyLandingGear;
using IMyGravityGeneratorBase = SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase;
using IMyTimerBlock = SpaceEngineers.Game.ModAPI.IMyTimerBlock;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum TBlockSubtypes : int
    {
        None = 0,
        Functional = 0x1,
        Powered = 0x2,
        Battery = 0x4,
        Inventory = 0x8,
        Production = 0x10,
        GasTank = 0x20,
        AirVent = 0x40,
        Door = 0x80,
        Parachute = 0x100,
        LandingGear = 0x200,
        Connector = 0x400,
        MechanicalConnection = 0x800,
        Suspension = 0x1000,
        Piston = 0x2000,
        Rotor = 0x4000,
        Light = 0x8000,
        JumpDrive = 0x10000,
        Thruster = 0x20000,
        Beacon = 0x40000,
        LaserAntenna = 0x80000,
        RadioAntenna = 0x100000,
        OreDetector = 0x200000,
        Gyroscope = 0x400000,
        Warhead = 0x800000,
        GunBase = 0x1000000,
        Turret = 0x2000000,
        GravityGen = 0x4000000,
        Sensor = 0x8000000,
        Projector = 0x10000000,
        Timer = 0x20000000,
        Programmable = 0x40000000
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
        public IReadOnlyList<SubtypeAccessorBase> SubtypeAccessors => subtypeAccessors;

        private readonly List<SubtypeAccessorBase> subtypeAccessors;

        public SuperBlock(IMyTerminalBlock tBlock)
        {
            TBlock = tBlock;
            TypeID = tBlock.BlockDefinition.TypeIdString;
            TBlock.OnMarkForClose += BlockClosing;
            subtypeAccessors = new List<SubtypeAccessorBase>();

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
            General = new GeneralAccessor(this);
            
            if (TBlock.ResourceSink != null || TBlock is IMyPowerProducer || TBlock is IMyFunctionalBlock)
            {
                Power = new PowerAccessor(this);

                if (TBlock is IMyBatteryBlock)
                    Battery = new BatteryAccessor(this);
            }

            if (TBlock.HasInventory)
                Inventory = new InventoryAccessor(this);

            if (TBlock is IMyProductionBlock)
                Production = new ProductionAccessorBase(this);

            if (TBlock is IMyGasTank)
                GasTank = new GasTankAccessor(this);

            if (TBlock is IMyAirVent)
                AirVent = new AirVentAccessor(this);

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

            if (TBlock is IMyLightingBlock)
                Light = new LightAccessor(this);

            if (TBlock is IMyJumpDrive)
                JumpDrive = new JumpDriveAccessor(this);

            if (TBlock is IMyThrust)
                Thruster = new ThrusterAccessor(this);

            if (TBlock is IMyBeacon)
                Beacon = new BeaconAccessorBase(this);

            if (TBlock is IMyLaserAntenna)
                LaserAntenna = new LaserAntennaAccessor(this);

            if (TBlock is IMyRadioAntenna)
                RadioAntenna = new RadioAntennaAccessor(this);

            if (TBlock is IMyOreDetector)
                OreDetector = new OreDetectorAccessor(this);

            if (TBlock is IMyWarhead)
                Warhead = new WarheadAccessor(this);

            if (TBlock is IMyGunBaseUser)
                Weapon = new GunBaseAccessor(this);

            if (TBlock is IMyLargeTurretBase)
                Turret = new TurretAccessor(this);

            if (TBlock is IMyGyro)
                Gyroscope = new GyroAccessor(this);

            if (TBlock is IMyGravityGeneratorBase)
                GravityGen = new GravityGenAccessor(this);

            if (TBlock is IMySensorBlock)
                Sensor = new SensorAccessor(this);

            if (TBlock is IMyProjector)
                Projector = new ProjectorAccessor(this);

            if (TBlock is IMyTimerBlock)
                Timer = new TimerAccessor(this);

            if (TBlock is IMyProgrammableBlock)
                Program = new ProgramBlockAccessor(this);
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
                block.subtypeAccessors.Add(this);
            }

            public abstract RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat);
        }
    }

    public static class SubtypeEnumExtensions
    {
        private static readonly IReadOnlyList<TBlockSubtypes> subtypes;

        static SubtypeEnumExtensions()
        {
            subtypes = Enum.GetValues(typeof(TBlockSubtypes)) as IReadOnlyList<TBlockSubtypes>;
        }

        public static TBlockSubtypes GetLargestSubtype(this TBlockSubtypes subtypeId)
        {
            for (int n = subtypes.Count - 1; n >= 0; n--)
            {
                if ((subtypeId & subtypes[n]) == subtypes[n])
                    return subtypes[n];
            }

            return TBlockSubtypes.None;
        }

        public static bool UsesSubtype(this TBlockSubtypes subtypeId, TBlockSubtypes flag) =>
            (subtypeId & flag) == flag;
    }    
}