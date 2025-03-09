using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Clase encargada de generar el grid del juego
public class GridManager : MonoBehaviour
{
    [Header("Variables")]
    public int gridWidth = 16; // Ancho del grid en casillas
    public int gridHeight = 8; // Alto del grid en casillas
    public int minPathLength = 30; // Longitud mínima del camino de los enemigos

    [Header("References")]
    public TileObject dirtTile; // Tipo de casilla para el camino
    public TileObject grassTile; // Tipo de casilla para el terreno construible
    private PathGenerator pathGenerator; // Generador de caminos
    private EnemyWaveManager enemyWaveManager; // Gestor de oleadas
    private Transform tilesParent; // Objeto padre para organizar las casillas

    // Evento que notifica cuando el grid está completamente generado
    public static event Action OnTilesGenerated;
    private bool pathTilesCompleted = false; // Bool para el camino completado
    private bool grassTilesCompleted = false; // Bool para el terreno completado
    
    private float tileSize = 5f; // Tamaño de cada casilla

    // Suscripción a eventos de inicio de juego
    private void OnEnable() => UIManager.OnGameStart += InitializeGame;
    private void OnDisable() => UIManager.OnGameStart -= InitializeGame;

    void Update()
    {
        // Cuando ambos tipos de casillas están generados, notifica
        if(pathTilesCompleted && grassTilesCompleted)
        {
            OnTilesGenerated?.Invoke();
            
            // Reinicia los bools 
            pathTilesCompleted = false;
            grassTilesCompleted = false;
        }
    }

    // Inicializa el juego cuando se pulsa el botón de inicio
    private void InitializeGame()
    {
        // Busca o crea un objeto para contener las casillas
        tilesParent = GameObject.Find("Tiles")?.transform;
        if (tilesParent == null)
        {
            tilesParent = new GameObject("Tiles").transform;
        }
        
        pathGenerator = new PathGenerator(gridWidth, gridHeight);
        enemyWaveManager = GetComponent<EnemyWaveManager>();

        // Genera un camino que cumpla con la longitud mínima
        List<Vector2Int> tileCells;
        do {
            tileCells = pathGenerator.GeneratePath();
        } while (tileCells.Count < minPathLength);
        
        // Comparte el camino con el gestor de oleadas
        enemyWaveManager.SetTileCells(tileCells);

        // Inicia la generación visual de las casillas
        StartCoroutine(DrawPath(tileCells));
        StartCoroutine(DrawGrassTiles());
    }

    // Genera visualmente las casillas del camino
    private IEnumerator DrawPath(List<Vector2Int> tileCells)
    {
        foreach (Vector2Int cell in tileCells)
        {
            // Coloca pasto o tierra dependiendo de si tiene vecinos en el camino
            bool hasNeighbor = pathGenerator.HasNeighbor(cell.x, cell.y);
            GameObject tilePrefab = hasNeighbor ? dirtTile.tilePrefab : grassTile.tilePrefab;

            Instantiate(tilePrefab, new Vector3(cell.x * tileSize, 0f, cell.y * tileSize), Quaternion.identity, tilesParent);
            yield return new WaitForSeconds(0.1f); 
        }
        pathTilesCompleted = true;
    }

    // Genera visualmente las casillas de terreno 
    private IEnumerator DrawGrassTiles()
    {
        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                // Solo coloca casillas donde no hay camino
                if (pathGenerator.TileisFree(x, y))
                {
                    Instantiate(grassTile.tilePrefab, new Vector3(x * tileSize, 0f, y * tileSize), Quaternion.identity, tilesParent);
                    yield return new WaitForSeconds(0.025f); 
                }
            }
        }
        grassTilesCompleted = true;
    }
}