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
    private EnemyWaveManager enemyWaveManager;
    private Transform parent;

    private void OnEnable()
    {
        UIManager.OnGameStart += InitializeGame;
    }

    private void OnDisable()
    {
        UIManager.OnGameStart -= InitializeGame;
    }

    private void InitializeGame()
    {
        parent = GameObject.Find("Tiles").transform;
        pathGenerator = new PathGenerator(gridWidth, gridHeight);
        enemyWaveManager = GetComponent<EnemyWaveManager>();

        List<Vector2Int> tileCells = pathGenerator.GeneratePath();
        int pathSize = tileCells.Count;

        while (pathSize < minPathLength)
        {
            tileCells = pathGenerator.GeneratePath();
            pathSize = tileCells.Count;
        }
        enemyWaveManager.SetTileCells(tileCells);

        StartCoroutine(DrawPath(tileCells));
        StartCoroutine(DrawGrassTiles());
    }

    private IEnumerator DrawPath(List<Vector2Int> tileCells)
    {
        for (int i = 0; i < tileCells.Count; i++)
        {
            bool hasNeighbor = pathGenerator.HasNeighbor(tileCells[i].x, tileCells[i].y);

            GameObject tilePrefab = hasNeighbor ? dirtTile.tilePrefab : grassTile.tilePrefab;

            Instantiate(tilePrefab, new Vector3(tileCells[i].x * 5, 0f, tileCells[i].y * 5), Quaternion.identity, parent);
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    private IEnumerator DrawGrassTiles()
    {
        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                if (pathGenerator.TileisFree(x, y))
                {
                    Instantiate(grassTile.tilePrefab, new Vector3(x * 5, 0f, y * 5), Quaternion.identity, parent);
                    yield return new WaitForSeconds(0.025f);
                }
            }
        }
        yield return null;
    }
}
