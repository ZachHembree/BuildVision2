using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	[Flags]
	public enum TBlockSubtypes : ulong
	{
		None = 0,
		Functional = 1 << 0,
		Powered = 1 << 1,
		Battery = 1 << 2,
		Inventory = 1 << 3,
		Production = 1 << 4,
		GasTank = 1 << 5,
		AirVent = 1 << 6,
		Door = 1 << 7,
		Parachute = 1 << 8,
		LandingGear = 1 << 9,
		Connector = 1 << 10,
		MechanicalConnection = 1 << 11,
		Suspension = 1 << 12,
		Piston = 1 << 13,
		Rotor = 1 << 14,
		Light = 1 << 15,
		JumpDrive = 1 << 16,
		Thruster = 1 << 17,
		Beacon = 1 << 18,
		LaserAntenna = 1 << 19,
		RadioAntenna = 1 << 20,
		OreDetector = 1 << 21,
		Gyroscope = 1 << 22,
		Warhead = 1 << 23,
		GunBase = 1 << 24,
		Turret = 1 << 25,
		GravityGen = 1 << 26,
		Sensor = 1 << 27,
		Projector = 1 << 28,
		Timer = 1 << 29,
		Programmable = 1 << 30,

		EventController = 1ul << 31,
		SolarFoodGenerator = 1ul << 32,
		FarmPlot = 1ul << 33, // Component - IMyFarmPlotLogic
		OxygenFarm = 1ul << 34, // IMyOxygenFarm - IMyGasBlock
		Assembler = 1ul << 35,
		Refinery = 1ul << 36,
		ButtonPanel = 1ul << 37,
		TextSurfaceProvider = 1ul << 38
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

		/// <summary>
		/// Block definition base
		/// </summary>
		public MyDefinitionBase BlockDefinition { get; private set; }

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
			BlockDefinition = MyDefinitionManager.Static.GetDefinition(tBlock.BlockDefinition);

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
			BlockDefinition = null;
		}

		/// <summary>
		/// Appends a summary of the block's current configuration
		/// </summary>
		public void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
		{
			builder.defaultFormat = valueFormat;

			foreach (SubtypeAccessorBase subtype in SubtypeAccessors)
			{
				if (subtype != null)
					subtype.GetSummary(builder, nameFormat, valueFormat);
			}
		}

		private StringBuilder GetCleanLocalizedText(MyStringId textID, string breakChars = ":")
		{
			string text = MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_PanelContent);
			char lastChar = ' ';
			textBuffer.Clear();

			foreach (char c in text)
			{
				bool shouldBreak = false;

				foreach (char breakChar in breakChars)
				{
					if (c == breakChar)
					{
						shouldBreak = true;
						break;
					}
				}

				if (shouldBreak)
					break;

				if (c >= ' ' && !(c == ' ' && textBuffer.Length == 0) && !(c == ' ' && lastChar == ' '))
					textBuffer.Append(c);

				lastChar = c;
			}

			return textBuffer;
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

			SetOrCreateAccessor(ref _eventController, this);

			SetOrCreateAccessor(ref _solarFood, this);

			SetOrCreateAccessor(ref _farmPlot, this);

			SetOrCreateAccessor(ref _oxygenFarm, this);

			SetOrCreateAccessor(ref _assembler, this);

			SetOrCreateAccessor(ref _refinery, this);

			SetOrCreateAccessor(ref _textSurfaceProvider, this);

			SetOrCreateAccessor(ref _buttonPanel, this);
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

		public abstract class ComponentAccessor<T> : SubtypeAccessorBase where T : class
		{
			protected T component;

			protected override void SetBlock(SuperBlock block, TBlockSubtypes subtypeId, bool addSubtype = true)
			{
				this.block = block;
				this.SubtypeId = subtypeId;

				if (block.TBlock.Components.TryGet<T>(out component))
				{
					block.SubtypeId |= subtypeId;
					block.subtypeAccessors.Add(this);
				}
			}

			public override void Reset()
			{
				block = null;
				component = null;
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