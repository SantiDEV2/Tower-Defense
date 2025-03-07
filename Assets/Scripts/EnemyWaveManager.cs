using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float spawnDelay = 0.5f;
    public float timeBetweenWaves = 10f;
    public int maxWaves = 3;
    
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    
    [Header("Pool Settings")]
    private List<GameObject> pooledObjects = new List<GameObject>();
    private int amountPool = 40;
    
    [HideInInspector] public int enemysAlive;
    [HideInInspector] public int enemyCount;
    [HideInInspector] public int waveCount = 0;
    
    private List<Vector2Int> tileCells;
    private float waveTimer;
    private bool waveCompleted = false;
    private bool waveStarted = false;
    
    private void Start()
    {
        UpdateEnemyCount();
        ResetTimer();
        InitializePool();
    }
    
    private void InitializePool()
    {
        //Creamos una Pool de Enemigos para no tener que intanciarlos solo se retribuyen de esta misma 
        for (int i = 0; i < amountPool; i++)
        {
            int enemyType = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyObj = Instantiate(enemyPrefabs[enemyType]);
            enemyObj.SetActive(false);
            pooledObjects.Add(enemyObj);
        }
    }
    
    private void Update()
    {
        if (waveCount < maxWaves) //Sistema de Oleadas donde estamos comprobando en que oleada estamos
        {
            if (enemysAlive <= 0 && waveStarted && !waveCompleted) //Comprobamos que no hay ningun enemigo restante y que la oleada fue completada
            {
                waveCompleted = true;
                waveStarted = false;
                waveCount++;
                if (waveCount < maxWaves) //Aqui se obtienen los nuevos parametros para la siguiente oleada
                {
                    UpdateEnemyCount();
                    ResetTimer();
                }
            }
            
            if (!waveStarted && waveCount < maxWaves) // Aqui si no hay ninguna oleada iniciada iniciamos una si es menor de las oleadas maximas 
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
    }
    
    private IEnumerator SpawnEnemiesWithDelay()
    {
        enemysAlive = enemyCount; //Asignamos la cantidad de enemigos a la cantidad perteneciente por la ronda
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();  
            yield return new WaitForSeconds(spawnDelay);
        }   //Spawneamos los enemigos con un delay para que no salgan encimados 
    }
    
    private void SpawnEnemy()
    {   
        // Obtenemos la primera posicion de nuestros tiles 
        Vector2Int startTile = tileCells[0];
        Vector3 startPos = new Vector3(startTile.x * 5 + 2.5f, 2.5f, startTile.y * 5 + 2.5f);
        
        // Retribuimos un objeto de nuestra pool
        GameObject enemyObj = GetPooledObject();
        
        // If no pooled object is available, create a new one
        if (enemyObj == null)
        {
            int enemyType = Random.Range(0, enemyPrefabs.Length);
            enemyObj = Instantiate(enemyPrefabs[enemyType]);
            pooledObjects.Add(enemyObj);
        }
        
        // Position and activate the enemy
        enemyObj.transform.position = startPos;
        enemyObj.transform.rotation = Quaternion.identity;
        
        // Make sure it has an Enemy component
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyObj.AddComponent<Enemy>();
        }
        else
        {
            enemy.InitializeMovement(tileCells);
        }
        
        enemyObj.SetActive(true);
    }
    
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
    
    public void EnemyDefeated()
    {
        enemysAlive--;
    }
    
    private void UpdateEnemyCount()
    {
        switch (waveCount)
        {
            case 0:
                enemyCount = 10;
                break;
            case 1:
                enemyCount = 15;
                break;
            case 2:
                enemyCount = 20;
                break;
            default:
                enemyCount = 10;
                break;
        }
    }
    
    private void ResetTimer()
    {
        waveTimer = timeBetweenWaves;
    }
    
    public void SetTileCells(List<Vector2Int> path)
    {
        tileCells = path;
    }
    
    public List<Vector2Int> GetTileCells()
    {
        return tileCells;
    }
}