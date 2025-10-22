using RichHudFramework.UI;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		/// <summary>
		/// Provides access to block inventory, if defined.
		/// </summary>
		public InventoryAccessor Inventory { get { return _inventory; } }

		private InventoryAccessor _inventory;

		public class InventoryAccessor : SubtypeAccessorBase
		{
			public IReadOnlyList<InventoryWrapper> Inventories => _inventories;

			/// <summary>
			/// Returns the number of inventories contained by the block
			/// </summary>
			public int InventoryCount => block.TBlock.InventoryCount;

			private readonly List<InventoryWrapper> _inventories;

			public InventoryAccessor()
			{
				_inventories = new List<InventoryWrapper>();
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.Inventory, block.TBlock.HasInventory);

				if (block.TBlock.HasInventory)
				{
					_inventories.Clear();
					_inventories.EnsureCapacity(InventoryCount);

					for (int n = 0; n < InventoryCount; n++)
						_inventories.Add(new InventoryWrapper(block.TBlock.GetInventory(n)));
				}
			}

			public override void Reset()
			{
				base.Reset();
				_inventories.Clear();
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				var buf = block.textBuffer;

				if (Inventories.Count > 1)
				{
					if (block.SubtypeId.HasFlag(TBlockSubtypes.Production) && Inventories.Count == 2)
					{
						// Input
						var input = _inventories[0];
						builder.Add($"{MyTexts.TrySubstitute("Input")}: ", nameFormat);
						builder.Add($"{input.CurrentVolume:N1} / {input.MaxVolume:N1} L ", valueFormat);
						builder.Add($"({(input.CurrentVolume / input.MaxVolume):P1})\n", nameFormat);
						
						// Output
						var output = _inventories[1];
						builder.Add($"{MyTexts.TrySubstitute("Output")}: ", nameFormat);
						builder.Add($"{output.CurrentVolume:N1} / {output.MaxVolume:N1} L ", valueFormat);
						builder.Add($"({(output.CurrentVolume / output.MaxVolume):P1})\n", nameFormat);
					}
					else // Fallback
					{
						for (int n = 0; n < Inventories.Count; n++)
						{
							var inventory = _inventories[n];
							buf.Clear();

							builder.Add($"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)} {n}: ", nameFormat);
							builder.Add($"{inventory.CurrentVolume:N1} / {inventory.MaxVolume:N1} L ", valueFormat);
							builder.Add($"({(inventory.CurrentVolume / inventory.MaxVolume):P1})\n", nameFormat);
						}
					}
				}
				else // Normal
				{
					var inventory = _inventories[0];
					builder.Add($"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)}: ", nameFormat);
					builder.Add($"{inventory.CurrentVolume:N1} / {inventory.MaxVolume:N1} L ", valueFormat);
					builder.Add($"({(inventory.CurrentVolume / inventory.MaxVolume):P1})\n", nameFormat);
				}
			}
		}

		public struct InventoryWrapper
		{
			public readonly IMyInventory inventory;

			/// <summary>
			/// Returns the total mass of the inventory in kilograms.
			/// </summary>
			public double CurrentMass => (double)inventory.CurrentMass;

			/// <summary>
			/// Returns the maximum volume of items in liters.
			/// </summary>
			public double MaxVolume => (double)inventory.MaxVolume * 1000d;

			/// <summary>
			/// Returns the total volume of items in liters.
			/// </summary>
			public double CurrentVolume => (double)inventory.CurrentVolume * 1000d;

			/// <summary>
			/// Number of occupied inventory slots
			/// </summary>
			public int ItemCount => inventory.ItemCount;

			public InventoryWrapper(IMyInventory inventory)
			{
				this.inventory = inventory;
			}
		}
	}
}