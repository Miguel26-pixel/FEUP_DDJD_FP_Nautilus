using System;

namespace Items
{
    public static class ItemGrid
    {
        public static bool[,] ValidateGrid(bool[,] grid)
        {
            // Enforce ItemConstants.ItemWidth and ItemConstants.ItemHeight
            if (grid.GetLength(0) > ItemConstants.ItemHeight ||
                grid.GetLength(1) > ItemConstants.ItemWidth)
            {
                throw new ArgumentException("Grid must be of size ItemConstants.ItemWidth x ItemConstants.ItemHeight");
            }

            // Create a new grid of size ItemConstants.ItemWidth x ItemConstants.ItemHeight
            bool[,] newGrid = new bool[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

            // Copy the old grid into the new grid
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    newGrid[row, col] = grid[row, col];
                }
            }

            return newGrid;
        }

        public static bool[,] SingleCellGrid()
        {
            bool[,] grid = new bool[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    grid[row, col] = false;
                }
            }

            grid[0, 0] = true;

            return grid;
        }

        public static bool[,] RotateClockwise(bool[,] grid)
        {
            throw new NotImplementedException();
        }

        public static bool[,] RotateCounterClockwise(bool[,] grid)
        {
            throw new NotImplementedException();
        }
    }
}