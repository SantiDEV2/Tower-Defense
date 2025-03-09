using System.Collections.Generic;
using UnityEngine;

// Clase que genera el camino que seguirán los enemigos
public class PathGenerator
{
    private readonly int width, height; // Dimensiones del grid
    private List<Vector2Int> tileCells; // Lista de casillas que forman el camino

    public PathGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    // Genera un camino aleatorio de izquierda a derecha
    public List<Vector2Int> GeneratePath()
    {
        tileCells = new List<Vector2Int>();
        
        int y = height / 2;
        int x = 0;

        // Genera el camino hasta llegar al borde derecho
        while (x < width)
        {
            tileCells.Add(new Vector2Int(x, y));

            bool valid = false;
            while (!valid)
            {
                // Elige un movimiento aleatorio: derecha, arriba o abajo
                int move = Random.Range(0, 3);

                // Movimiento hacia la derecha
                if (move == 0 || x % 2 == 0 || x > (width - 2))
                {
                    x++;
                    valid = true;
                }
                // Movimiento hacia arriba (si hay espacio y la casilla está libre)
                else if (move == 1 && y < (height - 2) && TileisFree(x, y + 1))
                {
                    y++;
                    valid = true;
                }
                // Movimiento hacia abajo (si hay espacio y la casilla está libre)
                else if (move == 2 && y > 2 && TileisFree(x, y - 1))
                {
                    y--;
                    valid = true;
                }
            }
        }
        return tileCells;
    }

    // Verifica si una casilla no está en el camino
    public bool TileisFree(int x, int y) => !tileCells.Contains(new Vector2Int(x, y));
    
    // Verifica si una casilla está en el camino
    public bool TileisTaken(int x, int y) => tileCells.Contains(new Vector2Int(x, y));

    // Verifica si una casilla tiene vecinos adyacentes en el camino
    public bool HasNeighbor(int x, int y) => 
        TileisTaken(x, y - 1) || TileisTaken(x, y + 1) || TileisTaken(x - 1, y) || TileisTaken(x + 1, y);
}