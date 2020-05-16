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
        public InventoryAccessor Inventory { get; private set; }

        public class InventoryAccessor : SubtypeAccessorBase
        {
            public IReadOnlyList<InventoryWrapper> Inventories { get; private set; }

            /// <summary>
            /// Returns the number of inventories contained by the block
            /// </summary>
            public int InventoryCount => block.TBlock.InventoryCount;

            public InventoryAccessor(SuperBlock block) : base(block, TBlockSubtypes.Inventory, block.TBlock.HasInventory)
            {
                if (block.TBlock.HasInventory)
                {
                    var inventories = new List<InventoryWrapper>(InventoryCount);
                    Inventories = inventories;

                    for (int n = 0; n < InventoryCount; n++)
                        inventories.Add(new InventoryWrapper(block.TBlock.GetInventory(n)));
                }
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText();

                if (Inventories.Count > 1)
                {
                    for (int n = 0; n < Inventories.Count; n++)
                    {
                        var inventory = Inventories[n];

                        summary.Add(new RichText
                        {
                            { $"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)} {n}: ", nameFormat },
                            { $"{inventory.CurrentVolume.ToString("G6")} / {inventory.MaxVolume.ToString("G6")} L ", valueFormat },
                            { $"({(100d * inventory.CurrentVolume / inventory.MaxVolume).Round(2)}%)\n", nameFormat },
                        });
                    }
                }
                else
                {
                    var inventory = Inventories[0];

                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.ScreenTerminalProduction_InventoryButton)}: ", nameFormat },
                        { $"{inventory.CurrentVolume.ToString("G6")} / {inventory.MaxVolume.ToString("G6")} L ", valueFormat },
                        { $"({(100d * inventory.CurrentVolume / inventory.MaxVolume).Round(2)}%)\n", nameFormat },
                    });
                }


                return summary;
            }
        }

        public class InventoryWrapper
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