using UnityEngine;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Visual presentation component for an individual 3D hexagonal tile.
    /// Decoupled from gameplay logic; driven by HexTileData.
    /// </summary>
    [SelectionBase]
    public class HexTileView : MonoBehaviour
    {
        [Header("Tile Association")]
        [SerializeField] private Vector3Int gridPosition;
        [SerializeField] private TerrainType terrainType;

        [Header("Visual Elements")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform eventMarkerRoot;
        [SerializeField] private MeshRenderer baseRenderer;

        private HexTileData boundData;

        public Vector3Int GridPosition => gridPosition;
        public TerrainType TerrainType => terrainType;
        public HexTileData BoundData => boundData;

        /// <summary>
        /// Binds runtime HexTileData to this reusable visual view.
        /// </summary>
        public void Bind(HexTileData tileData, Vector3 worldPosition)
        {
            if (tileData == null) return;

            boundData = tileData;
            gridPosition = tileData.gridPosition;
            terrainType = tileData.terrainType;
            transform.position = worldPosition;

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            if (baseRenderer == null)
            {
                baseRenderer = GetComponentInChildren<MeshRenderer>();
            }

            UpdateVisualState();
        }

        public void Unbind()
        {
            boundData = null;

            if (eventMarkerRoot != null)
            {
                eventMarkerRoot.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Updates visual elements based on the bound tile data.
        /// </summary>
        public void UpdateVisualState()
        {
            if (boundData == null) return;

            // Update event marker visibility if present
            if (eventMarkerRoot != null)
            {
                bool hasActiveEvent = boundData.eventType != HexEventType.None && !boundData.isCleared;
                eventMarkerRoot.gameObject.SetActive(hasActiveEvent);
            }
        }

        /// <summary>
        /// Returns the top center world position where units or props should stand.
        /// </summary>
        public Vector3 GetTopCenterWorldPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Highlights or unhighlights the tile (e.g. for selection or path preview).
        /// </summary>
        public void SetHighlight(bool isHighlighted, Color highlightColor)
        {
            if (baseRenderer == null) return;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            baseRenderer.GetPropertyBlock(block);
            if (isHighlighted)
            {
                block.SetColor("_EmissionColor", highlightColor * 0.5f);
            }
            else
            {
                block.SetColor("_EmissionColor", Color.black);
            }
            baseRenderer.SetPropertyBlock(block);
        }
    }
}
