using System;
using UnityEngine;
using Utils;

namespace Items
{
    public static class ItemGrid<T>
    {
        public static T[,] ValidateGrid(T[,] grid)
        {
            // Enforce ItemConstants.ItemWidth and ItemConstants.ItemHeight
            if (grid.GetLength(0) > ItemConstants.ItemHeight ||
                grid.GetLength(1) > ItemConstants.ItemWidth)
            {
                throw new ArgumentException("Grid must be of size ItemConstants.ItemWidth x ItemConstants.ItemHeight");
            }

            // Create a new grid of size ItemConstants.ItemWidth x ItemConstants.ItemHeight
            T[,] newGrid = new T[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

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

        public static T[,] SingleCellGrid(T defaultValue, T value)
        {
            T[,] grid = new T[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    grid[row, col] = value;
                }
            }

            grid[0, 0] = defaultValue;

            return grid;
        }

        public static T[,] RotateMultiple(T[,] grid, int rotation)
        {
            // rotate 3 is the same as rotate -1, rotate 4 is the same as rotate 0, etc.
            rotation = MathUtils.Modulo(rotation + 2, 4) - 2;

            int rotationsLeft = rotation;
            switch (rotationsLeft)
            {
                case > 0:
                {
                    while (rotationsLeft > 0)
                    {
                        grid = RotateCounterClockwise(grid);
                        rotationsLeft--;
                    }

                    break;
                }
                case < 0:
                {
                    while (rotationsLeft < 0)
                    {
                        grid = RotateClockwise(grid);
                        rotationsLeft++;
                    }

                    break;
                }
            }

            return grid;
        }

        public static T[,] RotateClockwise(T[,] grid)
        {
            // Rotate -90 degrees

            // Create a new grid of size ItemConstants.ItemWidth x ItemConstants.ItemHeight
            T[,] newGrid = new T[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    newGrid[row, col] = grid[ItemConstants.ItemWidth - col - 1, row];
                }
            }

            return newGrid;
        }

        public static T[,] RotateCounterClockwise(T[,] grid)
        {
            // Rotate +90 degrees

            // Create a new grid of size ItemConstants.ItemWidth x ItemConstants.ItemHeight
            T[,] newGrid = new T[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    newGrid[row, col] = grid[col, ItemConstants.ItemHeight - row - 1];
                }
            }

            return newGrid;
        }

        public static BoundsInt GetBounds(T[,] grid, T value)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            
            int width = grid.GetLength(1);
            int height = grid.GetLength(0);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (grid[row, col].Equals(value))
                    {
                        minX = Mathf.Min(minX, col);
                        minY = Mathf.Min(minY, row);
                        maxX = Mathf.Max(maxX, col);
                        maxY = Mathf.Max(maxY, row);
                    }
                }
            }

            return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
        }
    }
}