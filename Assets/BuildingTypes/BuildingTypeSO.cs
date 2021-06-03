using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCode
{
    [CreateAssetMenu()]
    public class BuildingTypeSO : ScriptableObject
    {
        [Header("Grid Related")]
        public Transform prefab;
        public Transform visual;
        public int width;
        public int height;

        [Header("Game Related")]
        public string nameString; // Building name
        public int maxHealth; // Max Building Health
        public int[] cost; // Array of material costs; Metals, Minerals and Credits

        public bool resourceProducer; // Wether this building can produce resources
        public ResourceType productionResourceType; // which resource this building produces
        public int resourceAmountProduced; // how much resources are produced per second

        public bool energyProducer; // Wether this building can supply or demand energy
        public int energy; // How much energy supply or demand this building has

        public bool metalConsumer;
        public bool mineralConsumer;
        public bool creditConsumer;
        public int metalsConsumed;
        public int mineralsConsumed;
        public int creditsConsumed;

        public static Dir GetNextDir(Dir dir)
        {
            switch (dir)
            {
                default:
                case Dir.Down: return Dir.Left;
                case Dir.Left: return Dir.Up;
                case Dir.Up: return Dir.Right;
                case Dir.Right: return Dir.Down;
            }
        }
        public static Dir GetPreviousDir(Dir dir)
        {
            switch (dir)
            {
                default:
                case Dir.Down: return Dir.Right;
                case Dir.Left: return Dir.Down;
                case Dir.Up: return Dir.Left;
                case Dir.Right: return Dir.Up;
            }
        }
        public enum Dir
        {
            Down,
            Left,
            Up,
            Right
        }
        public int GetRotationAngle(Dir dir)
        {
            switch (dir)
            {
                default:
                case Dir.Down: return 0;
                case Dir.Left: return 270;
                case Dir.Up: return 180;
                case Dir.Right: return 90;
            }
        }
        public Vector2Int GetRotationOffset(Dir dir)
        {
            switch (dir)
            {
                default:
                case Dir.Down: return new Vector2Int(0, 0);
                case Dir.Left: return new Vector2Int(0, width);
                case Dir.Up: return new Vector2Int(width, height);
                case Dir.Right: return new Vector2Int(height, 0);
            }
        }
        public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
        {
            List<Vector2Int> gridPositionList = new List<Vector2Int>();

            switch (dir)
            {
                default:
                case Dir.Down:
                case Dir.Up:
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            gridPositionList.Add(offset + new Vector2Int(x, y));
                        }
                    }
                    break;
                case Dir.Left:
                case Dir.Right:
                    for (int x = 0; x < height; x++)
                    {
                        for (int y = 0; y < width; y++)
                        {
                            gridPositionList.Add(offset + new Vector2Int(x, y));
                        }
                    }
                    break;
            }
            return gridPositionList;
        }
    }
}