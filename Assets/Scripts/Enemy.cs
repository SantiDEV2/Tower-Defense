using System.Collections.Generic;
using UnityEngine;

// Clase que controla el comportamiento de los enemigos
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f; // Velocidad de movimiento del enemigo

    [Header("Variables")]
    private int health; // Vida actual
    public int maxHealth; // Vida máxima
    
    [Header("References")]
    private List<Vector2Int> path; // Camino a seguir
    private int currentPathIndex = 0; // Índice actual en el camino
    private EnemyWaveManager waveManager; // Referencia al gestor de oleadas
    private PlayerManager playerManager; // Referencia al gestor del jugador
    private float tileSize = 5f; // Tamaño de cada casilla
    private float posY = 2.5f; // Altura del enemigo sobre el suelo

    private void Awake()
    {
        // Inicializa la vida con un valor por defecto si no se especifica
        health = maxHealth > 0 ? maxHealth : 100;
    }

    private void Start()
    {
        // Obtiene referencias a componentes necesarios
        waveManager = FindAnyObjectByType<EnemyWaveManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
        path = waveManager.GetTileCells();
        currentPathIndex = 0;
    }

    private void Update()
    {
        // Solo se mueve si el juego no está pausado
        if (!PlayerManager.IsGamePaused)
            MoveAlongPath();
    }

    // Mueve al enemigo a lo largo del camino definido
    private void MoveAlongPath()
    {
        if (path == null || path.Count <= 1 || currentPathIndex >= path.Count)
            return;

        Vector3 currentPos = transform.position;
        Vector2Int nextTile = path[currentPathIndex];
        Vector3 nextPos = new Vector3(nextTile.x * tileSize + tileSize/2, posY, nextTile.y * tileSize + tileSize/2);

        // Mueve el enemigo hacia la siguiente posición
        transform.position = Vector3.MoveTowards(currentPos, nextPos, Time.deltaTime * speed);

        // Rota el enemigo hacia la dirección de movimiento
        Vector3 direction = (nextPos - currentPos).normalized;
        if (direction != Vector3.zero)
            transform.forward = direction;

        // Si llega a la posición, avanza al siguiente punto del camino
        if (Vector3.Distance(currentPos, nextPos) < 0.05f)
        {
            currentPathIndex++;

            // Si ha llegado al final del camino, llama a ReachedEnd
            if (currentPathIndex >= path.Count)
                ReachedEnd();
        }
    }

    // Inicializa el movimiento con un nuevo camino
    public void InitializeMovement(List<Vector2Int> newPath)
    {
        currentPathIndex = 0;
        path = new List<Vector2Int>(newPath);
        health = maxHealth;
    }

    // Se ejecuta cuando el enemigo llega al final del camino
    private void ReachedEnd()
    {
        // El jugador pierde vida si un enemigo llega al final
        playerManager?.TakeDamage();
        waveManager?.EnemyDefeated();
        gameObject.SetActive(false);
    }

    // Recibe daño y gestiona la muerte
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // Cuando muere, da monedas al jugador
            playerManager.AddCurrency(15);
            waveManager.EnemyDefeated();
            gameObject.SetActive(false);
        }
    }
}