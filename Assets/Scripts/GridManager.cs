using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Variables")]
    public int gridWidth = 16;
    public int gridHeight = 8;
    public int minPathLength = 30;

    [Header("References")]
    public TileObject dirtTile;
    public TileObject grassTile;
    private PathGenerator pathGenerator;

    private void Start()
    {
        pathGenerator = new PathGenerator(gridWidth, gridHeight);

        List<Vector2Int> tileCells = pathGenerator.GeneratePath();
        int pathSize = tileCells.Count;

        while (pathSize < minPathLength)
        {
            tileCells = pathGenerator.GeneratePath();
            pathSize = tileCells.Count;
        }

        StartCoroutine(DrawPath(tileCells));
    }

    private IEnumerator DrawPath(List<Vector2Int> tileCells)
    {
        for (int i = 0; i < tileCells.Count; i++)
        {
            bool hasNeighbor = pathGenerator.HasNeighbor(tileCells[i].x, tileCells[i].y);

            GameObject tilePrefab = hasNeighbor ? grassTile.tilePrefab : dirtTile.tilePrefab;

            Instantiate(tilePrefab, new Vector3(tileCells[i].x * 5, 0f, tileCells[i].y * 5), Quaternion.identity);
            yield return new WaitForSeconds(0f);
        }
        yield return null;
    }
}
