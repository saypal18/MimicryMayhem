using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class GridBorder
{
    [SerializeField] private GameObject borderPrefab;
    [SerializeField] private Transform borderParent;

        // remove border children
    // create border prefab set scale to cover 1 unit outside the grid in all directions,
    // visual only
    // center of the first grid cell is at 0,0
    public void CreateGridBorder(Vector2 TileSize, Vector2Int Size, Vector3 baseWorldPos)
    {
        DestroyAllChildren();

        float thickness = 1f;
        float halfTileX = TileSize.x / 2f;
        float halfTileY = TileSize.y / 2f;

        Vector2 totalSize = new Vector2(Size.x * TileSize.x, Size.y * TileSize.y);
        Vector2 center = new Vector2((Size.x - 1) * TileSize.x / 2f, (Size.y - 1) * TileSize.y / 2f);

        // Bottom
        CreateSingleBorder(
            baseWorldPos + new Vector3(center.x, -halfTileY - thickness / 2f, 0),
            new Vector3(totalSize.x + 2 * thickness, thickness, 1));
        // Top
        CreateSingleBorder(
            baseWorldPos + new Vector3(center.x, (Size.y - 0.5f) * TileSize.y + thickness / 2f, 0),
            new Vector3(totalSize.x + 2 * thickness, thickness, 1));
        // Left
        CreateSingleBorder(
            baseWorldPos + new Vector3(-halfTileX - thickness / 2f, center.y, 0),
            new Vector3(thickness, totalSize.y, 1));
        // Right
        CreateSingleBorder(
            baseWorldPos + new Vector3((Size.x - 0.5f) * TileSize.x + thickness / 2f, center.y, 0),
            new Vector3(thickness, totalSize.y, 1));
    }
    private void DestroyAllChildren()
    {
        for (int i = borderParent.childCount - 1; i >= 0; i--)
        {
            PoolingEntity.Despawn(borderParent.GetChild(i).gameObject);
        }
    }

    private void CreateSingleBorder(Vector3 worldPosition, Vector3 scale)
    {
        GameObject border = PoolingEntity.Spawn(borderPrefab, borderParent);
        border.transform.localScale = scale;
        border.transform.position = worldPosition;
    }
}