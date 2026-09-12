using System.Collections.Generic;
using StrategyRPG.Combat;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Owns the hexagonal grid queries and runtime tile data.
    /// Provides spatial queries and neighbor calculations for Point-Top hex layouts.
    /// </summary>
    public class HexMapManager : MonoBehaviour
    {
        [Header("Grid & Tilemap References")]
        [SerializeField] private Grid hexGrid;
        [SerializeField] private Tilemap hexTilemap;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private HexMapRenderer mapRenderer;

        [Header("Terrain Tile Assets")]
        [SerializeField] private HexTerrainTile grassTile;
        [SerializeField] private HexTerrainTile forestTile;
        [SerializeField] private HexTerrainTile mountainTile;
        [SerializeField] private HexTerrainTile waterTile;
        [SerializeField] private HexTerrainTile townTile;

        [Header("Runtime Tile Data (Read-Only in Inspector)")]
        [SerializeField] private List<HexTileData> tileDataList = new List<HexTileData>();

        // Fast runtime lookup table by grid coordinates
        private readonly Dictionary<Vector3Int, HexTileData> tileDataMap = new Dictionary<Vector3Int, HexTileData>();

        public Grid HexGrid => hexGrid;
        public Tilemap HexTilemap => hexTilemap;
        public IReadOnlyList<HexTileData> TileData => tileDataList;

        private void Awake()
        {
            if (hexGrid == null)
            {
                hexGrid = GetComponentInParent<Grid>();
            }

            if (hexTilemap == null)
            {
                hexTilemap = GetComponent<Tilemap>();
            }

            if (battleManager == null)
            {
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            if (mapRenderer == null)
            {
                mapRenderer = FindAnyObjectByType<HexMapRenderer>();
            }

            BuildRuntimeTileData();
        }

        /// <summary>
        /// Reads the authored Tilemap, builds independent runtime HexTileData, then asks the renderer
        /// to display that data. Gameplay code uses the data map rather than the visual views.
        /// </summary>
        public void BuildRuntimeTileData()
        {
            tileDataMap.Clear();
            tileDataList.Clear();

            if (hexTilemap == null) return;

            BoundsInt bounds = hexTilemap.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                TileBase tileBase = hexTilemap.GetTile(pos);
                if (tileBase == null) continue;

                if (tileBase is HexTerrainTile hexTile)
                {
                    HexTileData data = new HexTileData(
                        pos,
                        hexTile.terrainType,
                        hexTile.isWalkable,
                        GetPrototypeEventType(pos));
                    tileDataMap[pos] = data;
                    tileDataList.Add(data);
                }
                else
                {
                    // Fallback for generic tiles
                    HexTileData data = new HexTileData(
                        pos,
                        TerrainType.Grass,
                        true,
                        GetPrototypeEventType(pos));
                    tileDataMap[pos] = data;
                    tileDataList.Add(data);
                }
            }

            if (mapRenderer != null)
            {
                mapRenderer.RenderAll(tileDataList);
            }
        }

        /// <summary>
        /// Temporary event assignments for the 19-cell prototype map.
        /// These can later be replaced by authored map data.
        /// </summary>
        private HexEventType GetPrototypeEventType(Vector3Int gridPos)
        {
            if (gridPos == new Vector3Int(0, 1, 0)) return HexEventType.Battle;
            if (gridPos == new Vector3Int(1, 0, 0)) return HexEventType.Resource;
            if (gridPos == new Vector3Int(-1, 0, 0)) return HexEventType.Merchant;
            if (gridPos == new Vector3Int(-1, -1, 0)) return HexEventType.Ruins;
            if (gridPos == new Vector3Int(0, -1, 0)) return HexEventType.RandomEvent;
            if (gridPos == new Vector3Int(1, 2, 0)) return HexEventType.Boss;

            return HexEventType.None;
        }

        /// <summary>
        /// Generates the prototype 19-cell hex map layout.
        /// Center (0,0) is Town. Surrounding tiles have a mix of Grass, Forest, Mountain, and Water.
        /// </summary>
        public void GeneratePrototype19Map()
        {
            if (hexTilemap == null)
            {
                hexTilemap = GetComponent<Tilemap>();
            }

            if (hexTilemap == null)
            {
                Debug.LogError("HexMapManager: Tilemap reference is missing!");
                return;
            }

            hexTilemap.ClearAllTiles();

            // Preset 19-cell layout (point-top odd-r coordinates)
            // Center is (0,0) -> Town
            SetCell(new Vector3Int(0, 0, 0), townTile);

            // Ring 1 (6 neighbors around town)
            SetCell(new Vector3Int(1, 0, 0), grassTile);
            SetCell(new Vector3Int(-1, 0, 0), grassTile);
            SetCell(new Vector3Int(0, 1, 0), forestTile);
            SetCell(new Vector3Int(-1, 1, 0), waterTile);
            SetCell(new Vector3Int(0, -1, 0), forestTile);
            SetCell(new Vector3Int(-1, -1, 0), mountainTile);

            // Ring 2 (outer 12 hexes)
            // Top row (y = 2, 3 tiles)
            SetCell(new Vector3Int(-1, 2, 0), mountainTile);
            SetCell(new Vector3Int(0, 2, 0), forestTile);
            SetCell(new Vector3Int(1, 2, 0), grassTile);

            // Upper middle row (y = 1, outer tiles)
            SetCell(new Vector3Int(-2, 1, 0), waterTile);
            SetCell(new Vector3Int(1, 1, 0), grassTile);

            // Center row (y = 0, outer tiles)
            SetCell(new Vector3Int(-2, 0, 0), waterTile);
            SetCell(new Vector3Int(2, 0, 0), grassTile);

            // Lower middle row (y = -1, outer tiles)
            SetCell(new Vector3Int(-2, -1, 0), mountainTile);
            SetCell(new Vector3Int(1, -1, 0), forestTile);

            // Bottom row (y = -2, 3 tiles)
            SetCell(new Vector3Int(-1, -2, 0), waterTile);
            SetCell(new Vector3Int(0, -2, 0), grassTile);
            SetCell(new Vector3Int(1, -2, 0), mountainTile);

            BuildRuntimeTileData();
        }

        private void SetCell(Vector3Int cellPos, HexTerrainTile tile)
        {
            if (tile != null && hexTilemap != null)
            {
                hexTilemap.SetTile(cellPos, tile);
            }
        }

        /// <summary>
        /// Retrieves runtime data for a tile at a specific grid position.
        /// </summary>
        public HexTileData GetTileData(Vector3Int gridPos)
        {
            tileDataMap.TryGetValue(gridPos, out HexTileData data);
            return data;
        }

        /// <summary>
        /// Checks if a cell is within bounds and walkable.
        /// </summary>
        public bool IsWalkable(Vector3Int gridPos)
        {
            if (tileDataMap.TryGetValue(gridPos, out HexTileData data))
            {
                return data.isWalkable;
            }
            return false;
        }

        /// <summary>
        /// Updates tile state and triggers its placeholder event after the player enters it.
        /// </summary>
        public void EnterTile(Vector3Int gridPos)
        {
            HexTileData data = GetTileData(gridPos);
            if (data == null)
            {
                Debug.LogWarning($"HexMapManager: No tile data exists at {gridPos}.", this);
                return;
            }

            data.isExplored = true;

            mapRenderer?.RefreshTile(gridPos);

            Debug.Log(
                $"Entered Tile: ({gridPos.x}, {gridPos.y})\n" +
                $"Terrain: {data.terrainType}\n" +
                $"Event: {data.eventType}\n" +
                $"Explored: {(data.isExplored ? "true" : "false")}",
                this);

            if (data.eventType == HexEventType.Battle)
            {
                if (data.isCleared)
                {
                    Debug.Log("This Battle tile has already been cleared.", this);
                }
                else if (battleManager != null)
                {
                    battleManager.StartBattle(data);
                }
                else
                {
                    Debug.LogError("HexMapManager: No BattleManager is available.", this);
                }

                return;
            }

            TriggerPlaceholderEvent(data.eventType);
        }

        private void TriggerPlaceholderEvent(HexEventType eventType)
        {
            switch (eventType)
            {
                case HexEventType.None:
                    Debug.Log("No event on this tile.", this);
                    break;
                case HexEventType.Battle:
                    Debug.Log("Battle event triggered.", this);
                    break;
                case HexEventType.Resource:
                    Debug.Log("Resource discovered.", this);
                    break;
                case HexEventType.Merchant:
                    Debug.Log("Merchant encountered.", this);
                    break;
                case HexEventType.Ruins:
                    Debug.Log("Ruins discovered.", this);
                    break;
                case HexEventType.RandomEvent:
                    Debug.Log("Random event triggered.", this);
                    break;
                case HexEventType.Boss:
                    Debug.Log("Boss encounter.", this);
                    break;
            }
        }

        /// <summary>
        /// Returns world space position of a cell's center.
        /// </summary>
        public Vector3 GetWorldPosition(Vector3Int gridPos)
        {
            // 3D prefabs are centered on the Grid, independent of the 2D sprite anchor.
            if (mapRenderer != null)
            {
                HexTileView view = mapRenderer.GetTileView(gridPos);
                if (view != null) return view.GetTopCenterWorldPosition();
                return hexGrid != null ? hexGrid.GetCellCenterWorld(gridPos) : Vector3.zero;
            }

            return hexTilemap != null ? hexTilemap.GetCellCenterWorld(gridPos) : Vector3.zero;
        }

        /// <summary>
        /// Returns grid cell coordinate from a world space position.
        /// </summary>
        public Vector3Int GetCellPosition(Vector3 worldPos)
        {
            return hexGrid != null ? hexGrid.WorldToCell(worldPos) : Vector3Int.zero;
        }

        /// <summary>
        /// Resolves the visible 3D terrain hit to its logical cell. In 2D, intersect
        /// the Tilemap plane and compensate for the authored sprite anchor.
        /// </summary>
        public bool TryGetCellFromRay(Ray ray, out Vector3Int cell)
        {
            cell = default;
            if (mapRenderer != null)
            {
                // Views are moved out of the pool during Awake, before the first physics tick.
                Physics.SyncTransforms();
                float nearestDistance = float.PositiveInfinity;
                bool found = false;
                foreach (RaycastHit hit in Physics.RaycastAll(
                    ray, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    HexTileView view = hit.collider.GetComponentInParent<HexTileView>();
                    if (view == null || view.BoundData == null ||
                        mapRenderer.GetTileView(view.GridPosition) != view ||
                        hit.distance >= nearestDistance) continue;

                    nearestDistance = hit.distance;
                    cell = view.GridPosition;
                    found = true;
                }

                // Empty space between rendered hexes is not a tile.
                return found;
            }

            if (hexTilemap == null) return false;
            Plane plane = new Plane(hexTilemap.transform.forward, hexTilemap.transform.position);
            if (!plane.Raycast(ray, out float distance)) return false;

            Vector3 anchorOffset = hexTilemap.GetCellCenterWorld(Vector3Int.zero) -
                                   hexTilemap.CellToWorld(Vector3Int.zero);
            cell = hexTilemap.WorldToCell(ray.GetPoint(distance) - anchorOffset);
            return hexTilemap.HasTile(cell);
        }

        /// <summary>
        /// Calculates the 6 adjacent neighboring hex coordinates for a Point-Top hex grid (odd-r offset).
        /// </summary>
        public List<Vector3Int> GetNeighbors(Vector3Int gridPos)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();
            bool isOddRow = Mathf.Abs(gridPos.y) % 2 == 1;

            if (isOddRow)
            {
                // Odd row offsets
                neighbors.Add(new Vector3Int(gridPos.x + 1, gridPos.y, 0));       // Right
                neighbors.Add(new Vector3Int(gridPos.x - 1, gridPos.y, 0));       // Left
                neighbors.Add(new Vector3Int(gridPos.x + 1, gridPos.y + 1, 0));   // Top-Right
                neighbors.Add(new Vector3Int(gridPos.x,     gridPos.y + 1, 0));   // Top-Left
                neighbors.Add(new Vector3Int(gridPos.x + 1, gridPos.y - 1, 0));   // Bottom-Right
                neighbors.Add(new Vector3Int(gridPos.x,     gridPos.y - 1, 0));   // Bottom-Left
            }
            else
            {
                // Even row offsets
                neighbors.Add(new Vector3Int(gridPos.x + 1, gridPos.y, 0));       // Right
                neighbors.Add(new Vector3Int(gridPos.x - 1, gridPos.y, 0));       // Left
                neighbors.Add(new Vector3Int(gridPos.x,     gridPos.y + 1, 0));   // Top-Right
                neighbors.Add(new Vector3Int(gridPos.x - 1, gridPos.y + 1, 0));   // Top-Left
                neighbors.Add(new Vector3Int(gridPos.x,     gridPos.y - 1, 0));   // Bottom-Right
                neighbors.Add(new Vector3Int(gridPos.x - 1, gridPos.y - 1, 0));   // Bottom-Left
            }

            return neighbors;
        }
    }
}
