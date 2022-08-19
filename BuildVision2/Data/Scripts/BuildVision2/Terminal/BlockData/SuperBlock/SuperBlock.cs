using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using System;
using System.Text;
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

        public TerminalGrid TerminalGrid { get; private set; }

        /// <summary>
        /// Returns true if the underlying block exists and is valid
        /// </summary>
        public bool IsValid => TBlock != null;

        /// <summary>
        /// Block type identifier. Uses IMyCubeBlock.BlockDefinition.TypeIdString.
        /// </summary>
        public string TypeID { get; private set; }

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
        private readonly StringBuilder textBuffer;

        public SuperBlock()
        {
            subtypeAccessors = new List<SubtypeAccessorBase>();
            textBuffer = new StringBuilder();
        }

        public virtual void SetBlock(TerminalGrid grid, IMyTerminalBlock tBlock)
        {
            Utils.Debug.AssertNotNull(tBlock);
            Utils.Debug.AssertNotNull(grid);

            Reset();
            TBlock = tBlock;
            TerminalGrid = grid;

            TypeID = tBlock.BlockDefinition.TypeIdString;
            TBlock.OnMarkForClose += BlockClosing;

            AddBlockSubtypes();
        }

        public virtual void Update() { }

        public virtual void Reset()
        {
            for (int i = 0; i < subtypeAccessors.Count; i++)
                subtypeAccessors[i].Reset();

            if (TBlock != null)
                TBlock.OnMarkForClose -= BlockClosing;

            subtypeAccessors.Clear();
            TypeID = null;
            TBlock = null;
            TerminalGrid = null;
            SubtypeId = 0;
        }

        /// <summary>
        /// Appends a summary of the block's current configuration
        /// </summary>
        public void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
        {
            foreach (SubtypeAccessorBase subtype in SubtypeAccessors)
            {
                if (subtype != null)
                    subtype.GetSummary(builder, nameFormat, valueFormat);
            }
        }

        /// <summary>
        /// Clears all references to the <see cref="IMyTerminalBlock"/> held by the <see cref="SuperBlock"/>.
        /// </summary>
        private void BlockClosing(IMyEntity entity)
        {
            
            Reset();
        }

        private void AddBlockSubtypes()
        {
            SetOrCreateAccessor(ref _general, this);

            SetOrCreateAccessor(ref _power, this);

            SetOrCreateAccessor(ref _battery, this);

            SetOrCreateAccessor(ref _inventory, this);

            SetOrCreateAccessor(ref _production, this);

            SetOrCreateAccessor(ref _gasTank, this);

            SetOrCreateAccessor(ref _airVent, this);

            SetOrCreateAccessor(ref _door, this);

            SetOrCreateAccessor(ref _landingGear, this);

            SetOrCreateAccessor(ref _connector, this);

            SetOrCreateAccessor(ref _mechConnection, this);

            SetOrCreateAccessor(ref _piston, this);

            SetOrCreateAccessor(ref _rotor, this);

            SetOrCreateAccessor(ref _light, this);

            SetOrCreateAccessor(ref _jumpDrive, this);

            SetOrCreateAccessor(ref _thruster, this);

            SetOrCreateAccessor(ref _beacon, this);

            SetOrCreateAccessor(ref _laserAntenna, this);

            SetOrCreateAccessor(ref _radioAntenna, this);

            SetOrCreateAccessor(ref _oreDetector, this);

            SetOrCreateAccessor(ref _warhead, this);

            SetOrCreateAccessor(ref _weapon, this);

            SetOrCreateAccessor(ref _turret, this);

            SetOrCreateAccessor(ref _gyroscope, this);

            SetOrCreateAccessor(ref _gravityGen, this);

            SetOrCreateAccessor(ref _sensor, this);

            SetOrCreateAccessor(ref _projector, this);

            SetOrCreateAccessor(ref _timer, this);

            SetOrCreateAccessor(ref _program, this);
        }

        public void GetGroupNamesForBlock(List<string> groups) =>
            TerminalGrid.GetGroupNamesForBlock(TBlock, groups);

        public void GetGroupsForBlock(List<IMyBlockGroup> groups) =>
            TerminalGrid.GetGroupsForBlock(TBlock, groups);

        private static void SetOrCreateAccessor<T>(ref T accessor, SuperBlock block) where T : SubtypeAccessorBase, new()
        {
            if (accessor == null)
                accessor = new T();

            accessor.SetBlock(block);
        }

        public abstract class SubtypeAccessorBase
        {
            public TBlockSubtypes SubtypeId { get; protected set; }

            protected SuperBlock block;

            public abstract void SetBlock(SuperBlock block);

            public virtual void Reset()
            {
                SubtypeId = TBlockSubtypes.None;
                block = null;
            }

            protected virtual void SetBlock(SuperBlock block, TBlockSubtypes subtypeId, bool addSubtype = true)
            {
                this.block = block;
                this.SubtypeId = subtypeId;

                if (addSubtype)
                {
                    block.SubtypeId |= subtypeId;
                    block.subtypeAccessors.Add(this);
                }
            }

            protected virtual void SetBlock(SuperBlock block, TBlockSubtypes subtypeId, TBlockSubtypes prerequsites) =>
                SetBlock(block, subtypeId, block.SubtypeId.UsesSubtype(prerequsites));

            public abstract void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat);
        }

        public abstract class SubtypeAccessor<T> : SubtypeAccessorBase where T : class
        {
            protected T subtype;

            protected override void SetBlock(SuperBlock block, TBlockSubtypes subtypeId, bool addSubtype = true)
            {
                this.block = block;
                subtype = block.TBlock as T;
                this.SubtypeId = subtypeId;

                if (addSubtype && subtype != null)
                {
                    block.SubtypeId |= subtypeId;
                    block.subtypeAccessors.Add(this);
                }
            }

            public override void Reset()
            {
                block = null;
                subtype = null;
                SubtypeId = 0;
            }
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