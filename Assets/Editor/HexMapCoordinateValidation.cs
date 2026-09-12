using System;
using System.Reflection;
using StrategyRPG.Map;
using UnityEditor;
using UnityEngine;

/// <summary>Play Mode regression checks for the authored 3D map. Run from Tools.</summary>
public static class HexMapCoordinateValidation
{
    [MenuItem("Tools/Validate Hex Map Coordinates %&v")]
    public static void Validate()
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Enter Play Mode before validating the hex map.");

        HexMapManager manager = UnityEngine.Object.FindAnyObjectByType<HexMapManager>();
        HexMapRenderer renderer = UnityEngine.Object.FindAnyObjectByType<HexMapRenderer>();
        HexMapInput input = UnityEngine.Object.FindAnyObjectByType<HexMapInput>();
        Transform player = GameObject.FindWithTag("Player").transform;
        Camera camera = Camera.main;
        Require(manager != null && renderer != null && input != null && camera != null, "Map references");
        Require(manager.GetTileData(input.CurrentCell).terrainType == TerrainType.Town,
            "Run validation from the starting Town");
        Vector3Int town = input.CurrentCell;
        Vector3 cameraPosition = camera.transform.position;
        Quaternion cameraRotation = camera.transform.rotation;
        int hits = 0;
        try
        {
            Require(Vector3.Distance(player.position, renderer.GetTileView(town).transform.position) < 0.0001f,
                "Player starts at the rendered Town center");
            foreach (float tilt in new[] { 0f, 40f, 65f })
            foreach (float yaw in new[] { 0f, 90f, 180f })
            {
                Vector3 offset = Quaternion.AngleAxis(yaw, Vector3.forward) *
                    (Quaternion.AngleAxis(-tilt, Vector3.right) * Vector3.back) * 8f;
                camera.transform.position = manager.GetWorldPosition(town) + offset;
                camera.transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
                foreach (HexTileData tile in manager.TileData)
                {
                    Vector3 center = renderer.GetTileView(tile.gridPosition).transform.position;
                    Ray ray = camera.ScreenPointToRay(camera.WorldToScreenPoint(center));
                    Require(manager.TryGetCellFromRay(ray, out Vector3Int cell) && cell == tile.gridPosition,
                        $"Ray hit {tile.gridPosition} at tilt {tilt}, yaw {yaw}");
                    hits++;
                }
            }

            MethodInfo move = typeof(HexMapInput).GetMethod("TryMovePlayer", BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3Int neighbor = town + Vector3Int.right;
            Require(manager.IsWalkable(neighbor), "Right neighbor is walkable");
            move.Invoke(input, new object[] { neighbor });
            Require(input.CurrentCell == neighbor &&
                Vector3.Distance(player.position, renderer.GetTileView(neighbor).transform.position) < 0.0001f,
                "Adjacent move updates the cell and player immediately");
            Vector3Int distant = town + Vector3Int.left * 2;
            move.Invoke(input, new object[] { distant });
            Require(input.CurrentCell == neighbor, "Non-adjacent click leaves the player in place");
            input.ReturnPlayerToTown();
            Require(input.CurrentCell == town &&
                Vector3.Distance(player.position, renderer.GetTileView(town).transform.position) < 0.0001f,
                "Returning to Town restores the same center");
            Debug.Log($"HEX_MAP_VALIDATION_PASSED: {hits} ray hits, Town spawn, immediate adjacent move, non-adjacent rejection, Town return.");
        }
        finally
        {
            camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        }
    }

    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException("Hex map validation failed: " + description);
    }
}
