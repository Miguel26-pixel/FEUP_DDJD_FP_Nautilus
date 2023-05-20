namespace Inventory
{
    public class CraftingInventory : InventoryGrid
    {
        public CraftingInventory(bool[,] gridShape) : base(gridShape, "Crafting Result")
        {
        }

        public static CraftingInventory CreateCraftingInventory()
        {
            bool[,] gridShape = new bool[InventoryConstants.PlayerInventoryMaxHeight,
                InventoryConstants.PlayerInventoryMaxWidth];

            for (int y = 0; y < gridShape.GetLength(0); y++)
            {
                for (int x = 0; x < gridShape.GetLength(1); x++)
                {
                    gridShape[y, x] = true;
                }
            }

            return new CraftingInventory(gridShape);
        }
    }
}