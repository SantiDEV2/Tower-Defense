using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    
    [Header("Variables")]
    public int health = 3;
    private readonly int maxHealth = 3;


    [Header("References")]
    private List<Vector2Int> path;
    private int currentPathIndex = 0;
    private EnemyWaveManager waveManager;
    private PlayerManager playerManager;

    [System.Obsolete]
    private void Start()
    {
        waveManager = FindObjectOfType<EnemyWaveManager>();
        playerManager = FindObjectOfType<PlayerManager>();

        path = waveManager.GetTileCells();

        currentPathIndex = 0;
    }

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if(PlayerManager.IsGamePaused) return;

        if (path == null || path.Count <= 1 || currentPathIndex >= path.Count)
            return;

        Vector3 currentPos = transform.position;
        Vector2Int nextTile = path[currentPathIndex];
        Vector3 nextPos = new Vector3(nextTile.x * 5 + 2.5f, 2.5f, nextTile.y * 5 + 2.5f);

        transform.position = Vector3.MoveTowards(currentPos, nextPos, Time.deltaTime * speed);

        if (Vector3.Distance(currentPos, nextPos) > 0.05f)
        {
            Vector3 direction = (nextPos - currentPos).normalized;
            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }

        if (Vector3.Distance(currentPos, nextPos) < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex >= path.Count)
            {
                ReachedEnd();
            }
        }
    }

    public void InitializeMovement(List<Vector2Int> newPath)
    {
        currentPathIndex = 0;
        path = new List<Vector2Int>(newPath);
        health = maxHealth;
    }

    private void ReachedEnd()
    {
        if (playerManager != null)
        {
            playerManager.TakeDamage();
        }

        if (waveManager != null)
        {
            waveManager.EnemyDefeated();
        }

        this.gameObject.SetActive(false);
    }

    public void TakeDamage(int damage){
        health -= damage;
        if(health == 0 )
        {
            waveManager.EnemyDefeated();
            this.gameObject.SetActive(false);
        }
    }
}