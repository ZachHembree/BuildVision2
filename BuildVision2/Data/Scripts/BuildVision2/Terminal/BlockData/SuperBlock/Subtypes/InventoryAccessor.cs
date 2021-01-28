using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using System.Collections.Generic;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using VRage.Game.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block inventory, if defined.
        /// </summary>
        public InventoryAccessor Inventory  { get { return _inventory; } private set { _inventory = value; } }

        private InventoryAccessor _inventory;

        public class InventoryAccessor : SubtypeAccessorBase
        {
            public IReadOnlyList<InventoryWrapper> Inventories => inventories;

            /// <summary>
            /// Returns the number of inventories contained by the block
            /// </summary>
            public int InventoryCount => block.TBlock.InventoryCount;

            private readonly List<InventoryWrapper> inventories;

            public InventoryAccessor()
            {
                inventories = new List<InventoryWrapper>();
            }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Inventory, block.TBlock.HasInventory);

                if (block.TBlock.HasInventory)
                {
                    inventories.Clear();
                    inventories.EnsureCapacity(InventoryCount);

                    for (int n = 0; n < InventoryCount; n++)
                        inventories.Add(new InventoryWrapper(block.TBlock.GetInventory(n)));
                }
            }

            public override void Reset()
            {
                base.Reset();
                inventories.Clear();
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (Inventories.Count > 1)
                {
                    for (int n = 0; n < Inventories.Count; n++)
                    {
                        var inventory = Inventories[n];

                        builder.Add($"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)} {n}: ", nameFormat);
                        builder.Add($"{inventory.CurrentVolume.ToString("G6")} / {inventory.MaxVolume.ToString("G6")} L ", valueFormat);
                        builder.Add($"({(100d * inventory.CurrentVolume / inventory.MaxVolume).Round(2)}%)\n", nameFormat);
                    }
                }
                else
                {
                    var inventory = Inventories[0];

                    builder.Add($"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)}: ", nameFormat);
                    builder.Add($"{inventory.CurrentVolume.ToString("G6")} / {inventory.MaxVolume.ToString("G6")} L ", valueFormat);
                    builder.Add($"({(100d * inventory.CurrentVolume / inventory.MaxVolume).Round(2)}%)\n", nameFormat);
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