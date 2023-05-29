using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generation.Resource
{
    [Serializable]
    public struct ResourceGeneratorSettings
    {
        public float radius;
        public int numSamplesBeforeRejection;
        public float minBiomeValue;
        public float maxBiomeValue;
        public float minHeight;
        public float maxHeight;
        public float minSlope;
        public float maxSlope;
        public float offset;
        public bool alignToSurface;
        public float chanceOfGenerating;
        public GameObject prefab;
        public bool isClustered;
        public float clusterMaxCount;
        public float clusterRadius;
        public float clusterChance;
    }

    public record ChunkPoints
    {
        // A list of whether each resource type has been generated in this chunk
        public readonly bool[] generated;
        public readonly LinkedList<Vector2>[] points;

        public ChunkPoints(int size)
        {
            generated = new bool[size];
            points = new LinkedList<Vector2>[size];

            for (int i = 0; i < size; i++)
            {
                points[i] = new LinkedList<Vector2>();
            }
        }
    }

    public class PointsGenerator
    {
        // A dictionary of chunk positions to a list of points in that chunk
        private readonly Dictionary<Vector2Int, ChunkPoints> _chunkPoints;

        private readonly float _chunkSize;

        private readonly int _seed;

        // A list of settings for each resource type
        private readonly ResourceGeneratorSettings[] _settings;

        // A list of cells for each resource type
        // Each cell generates a certain resource type
        // Cells of the same resource type have the same index
        // Cells of the same resource type have padding between them, so they don't overlap
        //    this is to always generate the same points for the same resource type and seed
        // Cells can span multiple chunks
        // Cells are generated in a grid pattern
        // The size of the grid is determined by the resource radius
        private readonly HashSet<Vector2Int>[] _cells;

        public PointsGenerator(ResourceGeneratorSettings[] settings, float chunkSize, int seed)
        {
            _settings = settings;
            _chunkSize = chunkSize;
            _seed = seed;
            _chunkPoints = new Dictionary<Vector2Int, ChunkPoints>();
            _cells = new HashSet<Vector2Int>[_settings.Length];

            for (int i = 0; i < _settings.Length; i++)
            {
                _cells[i] = new HashSet<Vector2Int>();
            }
        }

        public int MaxPoints()
        {
            int maxPoints = 0;

            for (int i = 0; i < _settings.Length; i++)
            {
                ResourceGeneratorSettings settings = _settings[i];
                maxPoints += PoissonDiscSampling.MaxPoints(settings.radius, CellSamplingRegion(i));
            }

            return maxPoints;
        }

        public LinkedList<Vector2>[] GeneratePoints(Vector2Int chunkPosition)
        {
            ChunkPoints chunkPoints;
            if (!_chunkPoints.TryGetValue(chunkPosition, out chunkPoints))
            {
                chunkPoints = new ChunkPoints(_settings.Length);
            }

            _chunkPoints[chunkPosition] = chunkPoints;

            for (int cellIndex = 0; cellIndex < _settings.Length; cellIndex++)
            {
                bool generated = chunkPoints.generated[cellIndex];
                if (generated)
                {
                    continue;
                }

                HashSet<Vector2Int> touchingCells = TouchingCells(chunkPosition, cellIndex);


                foreach (Vector2Int cellPosition in touchingCells)
                {
                    GeneratePointsInCell(cellPosition, cellIndex);
                }
            }

            return chunkPoints.points;
        }

        private List<Vector2> GenerateCluster(Vector2 position, int cellIndex)
        {
            ResourceGeneratorSettings settings = _settings[cellIndex];
            int seed = (int)(position.x * 10000 + position.y * 10000);

            Random.State state = Random.state;
            Random.InitState(seed);

            List<Vector2> resourceObjects = new();

            for (int i = 0; i < settings.clusterMaxCount; i++)
            {
                float x = Random.Range(-settings.clusterRadius, settings.clusterRadius);
                float y = Random.Range(-settings.clusterRadius, settings.clusterRadius);

                if (x * x + y * y > settings.clusterRadius * settings.clusterRadius)
                {
                    continue;
                }

                if (!(Random.value < settings.clusterChance))
                {
                    continue;
                }

                resourceObjects.Add(new Vector2(position.x + x, position.y + y));
            }

            Random.state = state;
            return resourceObjects;
        }

        private void GeneratePointsInCell(Vector2Int cellPosition, int cellIndex)
        {
            // If cell is already generated, return
            if (_cells[cellIndex].Contains(cellPosition))
            {
                return;
            }

            List<Vector2> points = PoissonDiscSampling.GeneratePoints(
                _settings[cellIndex].radius,
                CellSamplingRegion(cellIndex),
                _settings[cellIndex].numSamplesBeforeRejection,
                CellSeed(cellPosition, cellIndex));

            if (_settings[cellIndex].isClustered)
            {
                List<Vector2> newPoints = new();

                foreach (Vector2 point in points)
                {
                    newPoints.AddRange(GenerateCluster(point, cellIndex));
                }

                points = newPoints;
            }

            HashSet<Vector2Int> alteredChunks = new();

            // Assign points to chunks
            foreach (Vector2 point in points)
            {
                Vector2 newPoint = new(point.x, point.y);
                newPoint += CellWorldPosition(cellPosition, cellIndex) + CellSamplingStart(cellIndex);

                Vector2Int chunkPosition = ChunkPosition(newPoint);

                ChunkPoints chunkPoints;
                if (!_chunkPoints.TryGetValue(chunkPosition, out chunkPoints))
                {
                    chunkPoints = new ChunkPoints(_settings.Length);
                    _chunkPoints[chunkPosition] = chunkPoints;
                }

                chunkPoints.points[cellIndex].AddLast(newPoint);
                alteredChunks.Add(chunkPosition);
            }

            // Mark chunks as generated
            foreach (Vector2Int chunkPosition in alteredChunks)
            {
                ChunkPoints chunkPoints = _chunkPoints[chunkPosition];

                // Update chunk generated, if touching cells are generated, otherwise keep it false
                HashSet<Vector2Int> touchingCells = TouchingCells(chunkPosition, cellIndex);
                if (CheckGeneratedCells(touchingCells, cellIndex))
                {
                    chunkPoints.generated[cellIndex] = true;
                }
            }

            // Mark cell as generated
            _cells[cellIndex].Add(cellPosition);
        }

        private bool CheckGeneratedCells(HashSet<Vector2Int> cells, int cellIndex)
        {
            foreach (Vector2Int cell in cells)
            {
                if (!_cells[cellIndex].Contains(cell))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int ChunkPosition(Vector2 position)
        {
            return new Vector2Int(
                Mathf.RoundToInt(position.x / _chunkSize),
                Mathf.RoundToInt(position.y / _chunkSize)
            );
        }

        private float CellSize(int cellIndex)
        {
            return _settings[cellIndex].radius * 15;
        }

        private float CellPadding(int cellIndex)
        {
            return _settings[cellIndex].radius / 4;
        }

        private Vector2 CellSamplingRegion(int cellIndex)
        {
            return new Vector2(CellSize(cellIndex) - CellPadding(cellIndex) * 2,
                CellSize(cellIndex) - CellPadding(cellIndex) * 2);
        }

        private Vector2 CellSamplingStart(int cellIndex)
        {
            return new Vector2(CellPadding(cellIndex), CellPadding(cellIndex));
        }

        private int CellSeed(Vector2Int cellPosition, int cellIndex)
        {
            // Deterministically generate a seed for each cell
            return (_seed + cellIndex + cellPosition.x + cellPosition.y) * 31;
        }

        // Top left corner of the cell
        private Vector2Int CellPosition(Vector2 position, int cellIndex)
        {
            int cellSize = (int)CellSize(cellIndex);
            return new Vector2Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.y / cellSize)
            );
        }

        private Vector2 CellWorldPosition(Vector2Int cellPosition, int cellIndex)
        {
            int cellRadius = (int)CellSize(cellIndex);
            return new Vector2(cellPosition.x * cellRadius, cellPosition.y * cellRadius);
        }

        private HashSet<Vector2Int> TouchingCells(Vector2Int chunkPosition, int cellIndex)
        {
            Vector2 chunkPositionWorld = new(chunkPosition.x * _chunkSize, chunkPosition.y * _chunkSize);
            Vector2 chunkPositionWorldTopLeft = chunkPositionWorld - new Vector2(_chunkSize / 2, _chunkSize / 2);
            Vector2 chunkPositionWorldBottomLeft = chunkPositionWorld + new Vector2(-_chunkSize / 2, _chunkSize / 2);
            Vector2 chunkPositionWorldBottomRight = chunkPositionWorld + new Vector2(_chunkSize / 2, _chunkSize / 2);
            Vector2 chunkPositionWorldTopRight = chunkPositionWorld + new Vector2(_chunkSize / 2, -_chunkSize / 2);

            HashSet<Vector2Int> touchingCells = new();

            Vector2Int topLeftCell = CellPosition(chunkPositionWorldTopLeft, cellIndex);
            Vector2Int bottomRightCell = CellPosition(chunkPositionWorldBottomRight, cellIndex);
            Vector2Int topRightCell = CellPosition(chunkPositionWorldTopRight, cellIndex);
            Vector2Int bottomLeftCell = CellPosition(chunkPositionWorldBottomLeft, cellIndex);

            // If they are the same, since it is a set, it will only be added once
            touchingCells.Add(topLeftCell);
            touchingCells.Add(bottomRightCell);
            touchingCells.Add(topRightCell);
            touchingCells.Add(bottomLeftCell);

            return touchingCells;
        }
    }
}