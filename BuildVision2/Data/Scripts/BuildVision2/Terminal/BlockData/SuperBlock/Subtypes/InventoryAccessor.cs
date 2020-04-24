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
            public IMyInventory Inventory { get; private set; }

            /// <summary>
            /// Returns the total mass of the inventory in kilograms.
            /// </summary>
            public double CurrentMass => (double)Inventory.CurrentMass;

            /// <summary>
            /// Returns the maximum volume of items in cubic meters.
            /// </summary>
            public double MaxVolume => (double)Inventory.MaxVolume;

            /// <summary>
            /// Returns the total volume of items in cubic meters.
            /// </summary>
            public double CurrentVolume => (double)Inventory.CurrentVolume;

            /// <summary>
            /// Number of occupied inventory slots
            /// </summary>
            public int ItemCount => Inventory.ItemCount;

            /// <summary>
            /// Returns the number of inventories contained by the block
            /// </summary>
            public int InventoryCount => block.TBlock.InventoryCount;

            public InventoryAccessor(SuperBlock block) : base(block, TBlockSubtypes.Inventory)
            {
                Inventory = block.TBlock.GetInventory();             
            }
        }
    }
}