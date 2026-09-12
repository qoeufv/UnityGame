using System.Collections.Generic;
using UnityEngine;

namespace StrategyRPG.Map
{
    /// <summary>
    /// A lightweight visual container for the tile views in one chunk.
    /// It does not own or duplicate any gameplay data.
    /// </summary>
    public class HexMapChunk : MonoBehaviour
    {
        [SerializeField] private Vector2Int chunkCoordinate;

        private readonly List<HexTileView> tileViews = new List<HexTileView>();

        public Vector2Int ChunkCoordinate => chunkCoordinate;
        public IReadOnlyList<HexTileView> TileViews => tileViews;

        public void Initialize(Vector2Int coordinate)
        {
            chunkCoordinate = coordinate;
            gameObject.name = $"Chunk_{coordinate.x}_{coordinate.y}";
        }

        public void Add(HexTileView tileView)
        {
            if (tileView == null || tileViews.Contains(tileView)) return;

            tileViews.Add(tileView);
            tileView.transform.SetParent(transform, true);
        }

        public void Remove(HexTileView tileView)
        {
            tileViews.Remove(tileView);
        }

        public void Clear()
        {
            tileViews.Clear();
        }
    }
}
