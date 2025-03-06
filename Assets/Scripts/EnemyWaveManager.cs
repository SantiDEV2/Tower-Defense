using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{

    [Header("Variables")]
    [Range(0.1f, 10f)]
    public float speed = 5f;
    int nextTileCellIndex;
    bool enemyReached;

    [Header("References")]
    public GameObject enemyPrefab;
    private List<Vector2Int> tileCells;
    private GameObject enemyInstance;

    void Start()
    {
        if (tileCells != null && tileCells.Count > 0)
        {
            Vector2Int startTile = tileCells[0];
            Vector3 startPos = new Vector3(startTile.x * 5 + 2.5f, 2.5f, startTile.y * 5 + 2.5f);
            enemyInstance = Instantiate(enemyPrefab, startPos, Quaternion.identity);
            nextTileCellIndex = 1;
            enemyReached = false;
        }
    }

    void Update()
    {
        if(tileCells != null && tileCells.Count > 1 && !enemyReached)
        {
            Vector3 currentPos = enemyInstance.transform.position;
            Vector2Int nextTile = tileCells[nextTileCellIndex];
            Vector3 nextPos = new Vector3(nextTile.x * 5 + 2.5f, 2.5f, nextTile.y * 5 + 2.5f);

            enemyInstance.transform.position = Vector3.MoveTowards(currentPos, nextPos, Time.deltaTime * speed);
            if (Vector3.Distance(currentPos, nextPos) < 0.05f)
            {
                if (nextTileCellIndex >= tileCells.Count)
                {
                    enemyReached = true;
                }
                else
                {
                    nextTileCellIndex++;
                }
            }
        }    
    }

    public void SetTileCells(List<Vector2Int> tileCells)
    {
        this.tileCells = tileCells;
    }
}
