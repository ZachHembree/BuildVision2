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

        public TerminalGrid TerminalGrid { get; }

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

        public SuperBlock(TerminalGrid grid, IMyTerminalBlock tBlock)
        {
            Utils.Debug.AssertNotNull(tBlock);
            Utils.Debug.AssertNotNull(grid);

            TBlock = tBlock;
            TerminalGrid = grid;

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

            Power = new PowerAccessor(this);

            Battery = new BatteryAccessor(this);

            Inventory = new InventoryAccessor(this);

            Production = new ProductionAccessorBase(this);

            GasTank = new GasTankAccessor(this);

            AirVent = new AirVentAccessor(this);

            Door = new DoorAccessor(this);

            LandingGear = new LandingGearAccessor(this);

            Connector = new ConnectorAccessor(this);

            MechConnection = new MechConnectionAccessor(this);

            Piston = new PistonAccessor(this);

            Rotor = new RotorAccessor(this);

            Light = new LightAccessor(this);

            JumpDrive = new JumpDriveAccessor(this);

            Thruster = new ThrusterAccessor(this);

            Beacon = new BeaconAccessorBase(this);

            LaserAntenna = new LaserAntennaAccessor(this);

            RadioAntenna = new RadioAntennaAccessor(this);

            OreDetector = new OreDetectorAccessor(this);

            Warhead = new WarheadAccessor(this);

            Weapon = new GunBaseAccessor(this);

            Turret = new TurretAccessor(this);

            Gyroscope = new GyroAccessor(this);

            GravityGen = new GravityGenAccessor(this);

            Sensor = new SensorAccessor(this);

            Projector = new ProjectorAccessor(this);

            Timer = new TimerAccessor(this);

            Program = new ProgramBlockAccessor(this);
        }

        public void GetGroupNamesForBlock(List<string> groups) =>
            TerminalGrid.GetGroupNamesForBlock(TBlock, groups);

        public void GetGroupsForBlock(List<IMyBlockGroup> groups) =>
            TerminalGrid.GetGroupsForBlock(TBlock, groups);

        public abstract class SubtypeAccessorBase
        {
            public TBlockSubtypes SubtypeId { get; protected set; }

            protected SuperBlock block;

            protected SubtypeAccessorBase(SuperBlock block)
            {
                this.block = block;
            }

            protected SubtypeAccessorBase(SuperBlock block, TBlockSubtypes subtypeId, bool addSubtype = true)
            {
                this.block = block;
                this.SubtypeId = subtypeId;

                if (addSubtype)
                {
                    block.SubtypeId |= subtypeId;
                    block.subtypeAccessors.Add(this);
                }
            }

            protected SubtypeAccessorBase(SuperBlock block, TBlockSubtypes subtypeId, TBlockSubtypes prerequsites)
                : this(block, subtypeId, block.SubtypeId.UsesSubtype(prerequsites))
            { }

            public abstract void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat);
        }

        public abstract class SubtypeAccessor<T> : SubtypeAccessorBase where T : class
        {
            protected readonly T subtype;

            protected SubtypeAccessor(SuperBlock block, TBlockSubtypes subtypeId, bool addSubtype = true) : base(block)
            {
                subtype = block.TBlock as T;
                this.SubtypeId = subtypeId;

                if (addSubtype && subtype != null)
                {
                    block.SubtypeId |= subtypeId;
                    block.subtypeAccessors.Add(this);
                }
            }

            protected SubtypeAccessor(SuperBlock block, TBlockSubtypes subtypeId, TBlockSubtypes prerequsites) 
                : this(block, subtypeId, block.SubtypeId.UsesSubtype(prerequsites))
            { }
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