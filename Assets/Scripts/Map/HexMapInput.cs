using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Detects clicks on an existing Tilemap and logs the selected cell coordinate.
    /// </summary>
    public class HexMapInput : MonoBehaviour
    {
        [Header("Map References")]
        [SerializeField] private Tilemap hexTilemap;
        [SerializeField] private HexMapManager mapManager;

        [Header("Player")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector3Int currentCell;

        private Camera mainCamera;

        public Vector3Int CurrentCell => currentCell;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (hexTilemap == null)
            {
                hexTilemap = GetComponent<Tilemap>();
                if (hexTilemap == null)
                    hexTilemap = GetComponentInChildren<Tilemap>();
            }

            if (mapManager == null)
            {
                mapManager = GetComponent<HexMapManager>();
                if (mapManager == null)
                    mapManager = GetComponentInParent<HexMapManager>();
                if (mapManager == null)
                    mapManager = FindAnyObjectByType<HexMapManager>();
            }

            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }

            if (hexTilemap == null)
            {
                Debug.LogError("HexMapInput: Assign the HexTilemap in the Inspector.", this);
            }

            if (mapManager == null)
            {
                Debug.LogError("HexMapInput: Assign the HexMapManager in the Inspector.", this);
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("HexMapInput: No Player Transform assigned or tagged in the scene.", this);
            }

            if (mainCamera == null)
            {
                Debug.LogError("HexMapInput: The scene needs a camera tagged MainCamera.", this);
            }
        }

        private void Start()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            if (!TryFindTownCell(out currentCell))
            {
                Debug.LogError(
                    "HexMapInput: Could not find a Town tile for the Player's starting position.",
                    this);
                return;
            }

            MovePlayerToCell(currentCell);
            Debug.Log($"Player placed on starting tile: {currentCell}", this);
            mapManager.EnterTile(currentCell);
        }

        private bool TryFindTownCell(out Vector3Int townCell)
        {
            foreach (Vector3Int cellPosition in hexTilemap.cellBounds.allPositionsWithin)
            {
                HexTerrainTile terrainTile = hexTilemap.GetTile<HexTerrainTile>(cellPosition);
                if (terrainTile != null && terrainTile.terrainType == TerrainType.Town)
                {
                    townCell = cellPosition;
                    return true;
                }
            }

            townCell = Vector3Int.zero;
            return false;
        }

        private void Update()
        {
            if (!HasRequiredReferences() || Mouse.current == null)
            {
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float distanceToTilemap = hexTilemap.transform.position.z - mainCamera.transform.position.z;
            Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, distanceToTilemap);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            Vector3Int cellPosition = GetVisualCellPosition(worldPosition);

            if (hexTilemap.HasTile(cellPosition))
            {
                Debug.Log($"Clicked Hex Tile: {cellPosition}", this);
                TryMovePlayer(cellPosition);
            }
        }

        private Vector3Int GetVisualCellPosition(Vector3 worldPosition)
        {
            // Tile sprites are drawn relative to the Tilemap's Tile Anchor.
            // Remove that offset so WorldToCell matches the hex that is visible under the mouse.
            Vector3 cellAnchorOffset =
                hexTilemap.GetCellCenterWorld(Vector3Int.zero) -
                hexTilemap.CellToWorld(Vector3Int.zero);

            return hexTilemap.WorldToCell(worldPosition - cellAnchorOffset);
        }

        private void TryMovePlayer(Vector3Int destinationCell)
        {
            if (destinationCell == currentCell)
            {
                Debug.Log($"Player is already on {currentCell}.", this);
                return;
            }

            if (!mapManager.GetNeighbors(currentCell).Contains(destinationCell))
            {
                Debug.Log($"Tile is not adjacent: {currentCell} -> {destinationCell}", this);
                return;
            }

            if (!mapManager.IsWalkable(destinationCell))
            {
                Debug.Log($"Tile is not walkable: {destinationCell}", this);
                return;
            }

            Vector3Int previousCell = currentCell;
            currentCell = destinationCell;
            MovePlayerToCell(currentCell);

            Debug.Log($"Valid move: {previousCell} -> {currentCell}", this);
            mapManager.EnterTile(currentCell);
        }

        private void MovePlayerToCell(Vector3Int cellPosition)
        {
            Vector3 cellCenter = hexTilemap != null ? hexTilemap.GetCellCenterWorld(cellPosition) : Vector3.zero;
            cellCenter.z = playerTransform.position.z;
            playerTransform.position = cellCenter;
        }

        private bool HasRequiredReferences()
        {
            return hexTilemap != null &&
                   mapManager != null &&
                   playerTransform != null &&
                   mainCamera != null;
        }
    }
}
