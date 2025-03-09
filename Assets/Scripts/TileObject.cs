using UnityEngine;

// Define un ScriptableObject para representar tipos de casillas en el juego
[CreateAssetMenu(fileName = "Tile", menuName = "Scriptable Objects/TileScriptableObject")]
public class TileObject : ScriptableObject
{
    // Tipos de casillas: Camino (donde caminan los enemigos) y Terreno (donde se pueden construir torretas)
    public enum CellType { Path, Ground }

    public CellType cellType;      // Tipo de esta casilla
    public GameObject tilePrefab;  // Prefab visual de la casilla
}