using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace StrategyRPG.Map
{
    /// <summary>
    /// Converts logical HexTileData into pooled 3D HexTileView instances grouped by chunk.
    /// Gameplay code does not need to know whether a chunk is currently rendered.
    /// </summary>
    [ExecuteAlways]
    public class HexMapRenderer : MonoBehaviour
    {
        [Header("Data & Coordinate Sources")]
        [SerializeField] private HexMapManager mapManager;
        [SerializeField] private Grid hexGrid;

        [Header("3D Terrain Prefabs")]
        [SerializeField] private GameObject grassPrefab;
        [SerializeField] private GameObject forestPrefab;
        [SerializeField] private GameObject mountainPrefab;
        [SerializeField] private GameObject waterPrefab;
        [SerializeField] private GameObject townPrefab;

        [Header("Chunk Rendering")]
        [SerializeField, Min(1)] private int chunkSize = 5;
        [SerializeField] private Transform chunksRoot;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<Vector2Int, HexMapChunk> loadedChunks = new Dictionary<Vector2Int, HexMapChunk>();
        private readonly Dictionary<Vector3Int, HexTileView> activeViews = new Dictionary<Vector3Int, HexTileView>();
        private readonly Dictionary<TerrainType, Stack<HexTileView>> viewPools = new Dictionary<TerrainType, Stack<HexTileView>>();

        public int ChunkSize => chunkSize;
        public IReadOnlyDictionary<Vector2Int, HexMapChunk> LoadedChunks => loadedChunks;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                RebuildEditorPreview();
            }
        }

        /// <summary>
        /// Shows the current logical map in Edit Mode without saving generated tile views
        /// into the scene. This keeps the scene easy to inspect while data remains the source of truth.
        /// </summary>
        [ContextMenu("Rebuild Map Preview")]
        public void RebuildEditorPreview()
        {
            if (Application.isPlaying || !gameObject.scene.IsValid()) return;

            EnsureInitialized();
            ClearEditorPreviewObjects();

            if (mapManager != null && mapManager.TileData.Count > 0)
            {
                RenderAll(mapManager.TileData);
            }
        }

        /// <summary>
        /// Renders every supplied tile. The current prototype passes all 19 tiles here.
        /// A larger world can instead call RenderChunk and UnloadChunk selectively.
        /// </summary>
        public void RenderAll(IReadOnlyList<HexTileData> tileData)
        {
            EnsureInitialized();
            UnloadAllChunks();

            Dictionary<Vector2Int, List<HexTileData>> tilesByChunk =
                new Dictionary<Vector2Int, List<HexTileData>>();

            foreach (HexTileData data in tileData)
            {
                Vector2Int chunkCoordinate = GetChunkCoordinate(data.gridPosition);
                if (!tilesByChunk.TryGetValue(chunkCoordinate, out List<HexTileData> chunkTiles))
                {
                    chunkTiles = new List<HexTileData>();
                    tilesByChunk.Add(chunkCoordinate, chunkTiles);
                }

                chunkTiles.Add(data);
            }

            foreach (KeyValuePair<Vector2Int, List<HexTileData>> pair in tilesByChunk)
            {
                RenderChunk(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Loads or replaces one visual chunk using the supplied logical tile data.
        /// </summary>
        public void RenderChunk(Vector2Int chunkCoordinate, IReadOnlyList<HexTileData> tileData)
        {
            EnsureInitialized();
            UnloadChunk(chunkCoordinate);

            GameObject chunkObject = new GameObject();
            SetEditorPreviewFlags(chunkObject);
            chunkObject.transform.SetParent(chunksRoot, false);
            HexMapChunk chunk = chunkObject.AddComponent<HexMapChunk>();
            chunk.Initialize(chunkCoordinate);
            loadedChunks.Add(chunkCoordinate, chunk);

            foreach (HexTileData data in tileData)
            {
                if (GetChunkCoordinate(data.gridPosition) != chunkCoordinate) continue;

                HexTileView view = AcquireView(data.terrainType);
                if (view == null) continue;

                view.Bind(data, GetWorldPosition(data.gridPosition));
                chunk.Add(view);
                activeViews[data.gridPosition] = view;
            }
        }

        /// <summary>
        /// Removes one chunk from the active world and returns its views to the pool.
        /// HexTileData remains untouched and can be rendered again later.
        /// </summary>
        public void UnloadChunk(Vector2Int chunkCoordinate)
        {
            if (!loadedChunks.TryGetValue(chunkCoordinate, out HexMapChunk chunk)) return;

            // Edit Mode preview objects can be removed by Unity while entering Play Mode,
            // especially when Domain/Scene Reload is disabled. Discard their stale records.
            if (chunk == null)
            {
                loadedChunks.Remove(chunkCoordinate);
                RemoveActiveViewsInChunk(chunkCoordinate);
                return;
            }

            HexTileView[] viewsToRelease = new HexTileView[chunk.TileViews.Count];
            for (int i = 0; i < chunk.TileViews.Count; i++)
            {
                viewsToRelease[i] = chunk.TileViews[i];
            }

            foreach (HexTileView view in viewsToRelease)
            {
                ReleaseView(view);
            }

            chunk.Clear();
            loadedChunks.Remove(chunkCoordinate);

            if (Application.isPlaying)
            {
                Destroy(chunk.gameObject);
            }
            else
            {
                DestroyImmediate(chunk.gameObject);
            }
        }

        public void UnloadAllChunks()
        {
            Vector2Int[] chunkCoordinates = new Vector2Int[loadedChunks.Count];
            loadedChunks.Keys.CopyTo(chunkCoordinates, 0);

            foreach (Vector2Int chunkCoordinate in chunkCoordinates)
            {
                UnloadChunk(chunkCoordinate);
            }

            loadedChunks.Clear();
            activeViews.Clear();
        }

        public Vector2Int GetChunkCoordinate(Vector3Int hexCoordinate)
        {
            int safeChunkSize = Mathf.Max(1, chunkSize);
            return new Vector2Int(
                Mathf.FloorToInt(hexCoordinate.x / (float)safeChunkSize),
                Mathf.FloorToInt(hexCoordinate.y / (float)safeChunkSize));
        }

        public HexTileView GetTileView(Vector3Int hexCoordinate)
        {
            activeViews.TryGetValue(hexCoordinate, out HexTileView view);
            return view;
        }

        public void RefreshTile(Vector3Int hexCoordinate)
        {
            if (activeViews.TryGetValue(hexCoordinate, out HexTileView view))
            {
                view.UpdateVisualState();
            }
        }

        private void EnsureInitialized()
        {
            if (mapManager == null)
            {
                mapManager = FindAnyObjectByType<HexMapManager>();
            }

            if (hexGrid == null && mapManager != null)
            {
                hexGrid = mapManager.HexGrid;
            }

            EnsureRoot(ref chunksRoot, "Chunks", active: true);
            EnsureRoot(ref poolRoot, "Tile View Pool", active: false);

            foreach (TerrainType terrainType in System.Enum.GetValues(typeof(TerrainType)))
            {
                if (!viewPools.ContainsKey(terrainType))
                {
                    viewPools[terrainType] = new Stack<HexTileView>();
                }
            }

            Tilemap tilemap = mapManager != null ? mapManager.HexTilemap : null;
            TilemapRenderer tilemapRenderer = tilemap != null ? tilemap.GetComponent<TilemapRenderer>() : null;
            if (tilemapRenderer != null)
            {
                tilemapRenderer.enabled = false;
            }

        }

        private void EnsureRoot(ref Transform root, string rootName, bool active)
        {
            if (root == null)
            {
                GameObject rootObject = new GameObject(rootName);
                rootObject.transform.SetParent(transform, false);
                root = rootObject.transform;
            }

            root.gameObject.SetActive(active);
        }

        private HexTileView AcquireView(TerrainType terrainType)
        {
            if (!viewPools.TryGetValue(terrainType, out Stack<HexTileView> pool))
            {
                pool = new Stack<HexTileView>();
                viewPools[terrainType] = pool;
            }

            HexTileView view = null;

            while (pool.Count > 0 && view == null)
            {
                view = pool.Pop();
            }

            if (view == null)
            {
                GameObject prefab = GetPrefabForTerrain(terrainType);
                if (prefab == null)
                {
                    Debug.LogWarning($"HexMapRenderer: No prefab assigned for {terrainType}.", this);
                    return null;
                }

                GameObject viewObject = Instantiate(prefab, poolRoot);
                SetEditorPreviewFlags(viewObject);
                view = viewObject.GetComponent<HexTileView>();
                if (view == null)
                {
                    view = viewObject.AddComponent<HexTileView>();
                }
            }

            view.gameObject.name = $"{terrainType} Tile View";
            view.gameObject.SetActive(true);
            return view;
        }

        private void ReleaseView(HexTileView view)
        {
            if (view == null) return;

            TerrainType terrainType = view.TerrainType;
            if (view.BoundData != null)
            {
                activeViews.Remove(view.BoundData.gridPosition);
            }

            HexMapChunk chunk = view.GetComponentInParent<HexMapChunk>();
            if (chunk != null)
            {
                chunk.Remove(view);
            }

            view.Unbind();
            view.transform.SetParent(poolRoot, false);
            view.gameObject.SetActive(false);
            if (!viewPools.TryGetValue(terrainType, out Stack<HexTileView> pool))
            {
                pool = new Stack<HexTileView>();
                viewPools[terrainType] = pool;
            }

            pool.Push(view);
        }

        private Vector3 GetWorldPosition(Vector3Int hexCoordinate)
        {
            return hexGrid != null ? hexGrid.GetCellCenterWorld(hexCoordinate) : Vector3.zero;
        }

        private GameObject GetPrefabForTerrain(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Grass: return grassPrefab;
                case TerrainType.Forest: return forestPrefab;
                case TerrainType.Mountain: return mountainPrefab;
                case TerrainType.Water: return waterPrefab;
                case TerrainType.Town: return townPrefab;
                default: return grassPrefab;
            }
        }

        private void ClearEditorPreviewObjects()
        {
            loadedChunks.Clear();
            activeViews.Clear();

            foreach (Stack<HexTileView> pool in viewPools.Values)
            {
                pool.Clear();
            }

            DestroyChildrenImmediately(chunksRoot);
            DestroyChildrenImmediately(poolRoot);
        }

        private static void DestroyChildrenImmediately(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void SetEditorPreviewFlags(GameObject generatedObject)
        {
            if (!Application.isPlaying)
            {
                generatedObject.hideFlags = HideFlags.DontSaveInEditor;
            }
        }

        private void RemoveActiveViewsInChunk(Vector2Int chunkCoordinate)
        {
            List<Vector3Int> staleCoordinates = new List<Vector3Int>();

            foreach (KeyValuePair<Vector3Int, HexTileView> pair in activeViews)
            {
                if (pair.Value == null || GetChunkCoordinate(pair.Key) == chunkCoordinate)
                {
                    staleCoordinates.Add(pair.Key);
                }
            }

            foreach (Vector3Int coordinate in staleCoordinates)
            {
                activeViews.Remove(coordinate);
            }
        }
    }
}
