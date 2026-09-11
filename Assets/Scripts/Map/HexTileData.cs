using System;
using UnityEngine;

namespace StrategyRPG.Map
{
    /// <summary>
    /// The placeholder event associated with a map cell.
    /// </summary>
    public enum HexEventType
    {
        None,
        Battle,
        Resource,
        Merchant,
        Ruins,
        RandomEvent,
        Boss
    }

    /// <summary>
    /// Holds runtime gameplay data and state for a single hexagonal tile.
    /// Used by future systems (movement, events, combat, regional conquest, etc.).
    /// </summary>
    [Serializable]
    public class HexTileData
    {
        [Tooltip("The grid coordinates (column, row, 0) of this hex tile.")]
        public Vector3Int gridPosition;

        [Tooltip("The terrain classification of this tile.")]
        public TerrainType terrainType;

        [Tooltip("Whether units can traverse onto this tile.")]
        public bool isWalkable;

        [Tooltip("Whether this tile has been discovered by the player.")]
        public bool isExplored;

        [Tooltip("Whether the tile's encounter/event has been completed.")]
        public bool isCleared;

        [Tooltip("The placeholder event assigned to this tile.")]
        public HexEventType eventType;

        public HexTileData()
        {
        }

        public HexTileData(
            Vector3Int gridPosition,
            TerrainType terrainType,
            bool isWalkable,
            HexEventType eventType = HexEventType.None)
        {
            this.gridPosition = gridPosition;
            this.terrainType = terrainType;
            this.isWalkable = isWalkable;
            this.isExplored = false;
            this.isCleared = false;
            this.eventType = eventType;
        }
    }
}
