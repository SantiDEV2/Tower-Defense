using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Clase que gestiona las oleadas de enemigos
public class EnemyWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float spawnDelay = 0.5f; // Tiempo entre aparición de enemigos
    public float timeBetweenWaves = 20f; // Tiempo entre oleadas
    public int maxWaves = 3; // Número máximo de oleadas
    
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Tipos de enemigos
    
    [Header("Pool Settings")]
    private List<GameObject> pooledObjects = new List<GameObject>(); // Pool de objetos para reutilizar
    private int amountPool = 25; // Tamaño inicial del pool
    private float tileSize = 5f; // Tamaño de cada casilla
    private float posY = 2.5f; // Altura de los enemigos
    
    [HideInInspector] public int enemysAlive; // Número de enemigos vivos actualmente
    [HideInInspector] public int enemyCount; // Número total de enemigos en la oleada actual
    [HideInInspector] public int waveCount = 0; // Contador de oleadas completadas
    [HideInInspector] public float waveTimer; // Temporizador para la siguiente oleada
    
    private List<Vector2Int> tileCells; // Camino que siguen los enemigos
    private bool waveCompleted = false; // Indica si la oleada actual está completada
    private bool waveStarted = false; // Indica si la oleada actual ha comenzado
    
    // Eventos para notificar a otros sistemas
    public static event Action OnWaveCompleted;
    public static event Action OnGameWin;

    // Suscripción a eventos de inicio de juego
    private void OnEnable() => UIManager.OnGameStart += InitializeGame;
    private void OnDisable() => UIManager.OnGameStart -= InitializeGame;

    // Inicializa el juego cuando se pulsa el botón de inicio
    private void InitializeGame()
    {
        UpdateEnemyCount();
        ResetTimer();
        InitializePool();
    }
    
    // Inicializa el pool de enemigos para reutilización
    private void InitializePool()
    {
        for (int i = 0; i < amountPool ; i++)
        {
            int enemyType = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            GameObject enemyObj = Instantiate(enemyPrefabs[enemyType]);
            enemyObj.SetActive(false);
            pooledObjects.Add(enemyObj);
        }
    }
    
    private void Update()
    {
        // Si se alcanzó el número máximo de oleadas, termina
        if (waveCount >= maxWaves)
            return;
            
        // Comprueba si la oleada actual ha terminado
        if (enemysAlive <= 0 && waveStarted && !waveCompleted)
        {
            waveCompleted = true;
            waveStarted = false;
            waveCount++;
            
            // Si quedan más oleadas, prepara la siguiente
            if (waveCount < maxWaves)
            {
                OnWaveCompleted?.Invoke();
                UpdateEnemyCount();
                ResetTimer();
            }
            else
            {
                // Victoria si se completan todas las oleadas
                OnGameWin?.Invoke();
            }
        }
        
        // Inicia la siguiente oleada cuando termina el temporizador
        if (!waveStarted && waveCount < maxWaves)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
            {
                StartCoroutine(SpawnEnemiesWithDelay());
                waveCompleted = false;
                waveStarted = true;
            }
        }
    }
    
    // Genera enemigos con un retraso entre cada uno
    private IEnumerator SpawnEnemiesWithDelay()
    {
        enemysAlive = enemyCount;
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();  
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
    // Genera un enemigo desde el pool
    private void SpawnEnemy()
    {   
        // Obtiene la posición inicial desde el camino
        Vector2Int startTile = tileCells[0];
        Vector3 startPos = new Vector3(startTile.x * tileSize + tileSize/2, posY, startTile.y * tileSize + tileSize/2);
        
        // Obtiene un enemigo del pool
        GameObject enemyObj = GetPooledObject();
        
        // Crea un nuevo enemigo si el pool está agotado
        if (enemyObj == null)
        {
            int enemyType = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            enemyObj = Instantiate(enemyPrefabs[enemyType]);
            pooledObjects.Add(enemyObj);
        }
        
        // Configura e inicia el enemigo
        enemyObj.transform.position = startPos;
        enemyObj.transform.rotation = Quaternion.identity;
    
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.InitializeMovement(tileCells);
        enemyObj.SetActive(true);
    }
    
    // Obtiene un objeto inactivo del pool
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
                return pooledObjects[i];
        }
        return null;
    }
    
    // Reduce el contador de enemigos vivos
    public void EnemyDefeated() => enemysAlive--;
    
    // Actualiza el número de enemigos según la oleada actual
    private void UpdateEnemyCount()
    {
        enemyCount = waveCount switch
        {
            0 => 10,  // Primera oleada: 10 enemigos
            1 => 15,  // Segunda oleada: 15 enemigos
            2 => 20,  // Tercera oleada: 20 enemigos
            _ => 10   // Por defecto: 10 enemigos
        };
    }
    
    // Reinicia el temporizador entre oleadas
    private void ResetTimer() => waveTimer = timeBetweenWaves;
    
    // Establece el camino que seguirán los enemigos
    public void SetTileCells(List<Vector2Int> path) => tileCells = path;
    
    // Obtiene el camino actual
    public List<Vector2Int> GetTileCells() => tileCells;

    // Obtiene todos los enemigos activos
    public List<GameObject> GetActiveEnemies()
    {
        List<GameObject> activeEnemies = new List<GameObject>();
        
        foreach (GameObject obj in pooledObjects)
        {
            if (obj != null && obj.activeInHierarchy)
                activeEnemies.Add(obj);
        }
        return activeEnemies;
    }
}