using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkFloorGenerator : MonoBehaviour
{
    [Header("Ground Settings")]
    public Tilemap groundTilemap;
    public TileBase groundTile;

    // GENERATE CHUNK FLOOR
    public void GenerateChunkFloor(
        Vector2Int chunkCoordinate,
        float chunkSize)
    {
        if (!ValidateReferences())
            return;

        BoundsInt bounds = GetChunkBounds(
            chunkCoordinate,
            chunkSize
        );

        TileBase[] tiles = new TileBase[
            bounds.size.x * bounds.size.y
        ];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = groundTile;
        }

        groundTilemap.SetTilesBlock(bounds, tiles);
    }

    // CLEAR CHUNK FLOOR
    public void ClearChunkFloor(
        Vector2Int chunkCoordinate,
        float chunkSize)
    {
        if (groundTilemap == null)
            return;

        BoundsInt bounds = GetChunkBounds(
            chunkCoordinate,
            chunkSize
        );

        TileBase[] emptyTiles = new TileBase[
            bounds.size.x * bounds.size.y
        ];

        groundTilemap.SetTilesBlock(bounds, emptyTiles);
    }

    // CALCULATE CHUNK TILE BOUNDS
    private BoundsInt GetChunkBounds(
        Vector2Int chunkCoordinate,
        float chunkSize)
    {
        Vector3 worldMin = new Vector3(
            chunkCoordinate.x * chunkSize,
            chunkCoordinate.y * chunkSize,
            0f
        );

        Vector3 worldMax = new Vector3(
            (chunkCoordinate.x + 1) * chunkSize,
            (chunkCoordinate.y + 1) * chunkSize,
            0f
        );

        Vector3Int minCell =
            groundTilemap.WorldToCell(worldMin);

        Vector3Int maxCell =
            groundTilemap.WorldToCell(worldMax);

        return new BoundsInt(
            minCell.x,
            minCell.y,
            minCell.z,
            maxCell.x - minCell.x,
            maxCell.y - minCell.y,
            1
        );
    }

    // VALIDATE REFERENCES
    private bool ValidateReferences()
    {
        if (groundTilemap == null || groundTile == null)
        {
            Debug.LogError(
                "ChunkFloorGenerator: Missing Ground Tilemap or Ground Tile."
            );

            return false;
        }

        return true;
    }
}