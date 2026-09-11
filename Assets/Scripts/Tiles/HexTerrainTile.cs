using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Custom Tilemap Tile representing a hexagonal terrain tile.
    /// Stores the base terrain type and default walkability for the tile asset.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHexTerrainTile", menuName = "Strategy RPG/Hex Terrain Tile")]
    public class HexTerrainTile : Tile
    {
        [Header("Terrain Properties")]
        [Tooltip("The terrain type associated with this tile.")]
        public TerrainType terrainType = TerrainType.Grass;

        [Tooltip("Default walkability for this terrain type.")]
        public bool isWalkable = true;
    }
}
