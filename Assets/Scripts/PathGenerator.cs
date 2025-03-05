using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathGenerator
{
    private int width, height;
    private List<Vector2Int> tileCells;

    public PathGenerator (int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public List<Vector2Int> GeneratePath()
    {
        tileCells = new List<Vector2Int>();

        int y = (int)(height / 2);
        int x = 0;

        while (x < width)
        {
            tileCells.Add(new Vector2Int(x, y));

            bool valid = false;

            while (!valid)
            {
                int move = Random.Range(0, 3);

                if (move == 0 || x % 2 == 0 || x > (width - 2 /* 8 */))
                {
                    x++;
                    valid = true;
                }
                else if (move == 1 && TileisFree(x, (y + 1)) && y < (height - 2 /* 4 */ ))
                {
                    y++;
                    valid = true;
                }
                else if (move == 2 && TileisFree(x, (y - 1)) && y > 2)
                {
                    y--;
                    valid = true;
                }
            }
        }
        return tileCells;
    }

    private bool TileisFree(int x, int y)
    {
        return !tileCells.Contains(new Vector2Int(x, y)); 
    }

    private bool TileisTaken(int x, int y)
    {
        return tileCells.Contains(new Vector2Int(x, y));
    }

    public bool HasNeighbor(int x, int y)
    {
        return (TileisTaken(x, y - 1) || TileisTaken(x, y + 1) || TileisTaken(x - 1, y) || TileisTaken(x + 1, y));
    }
}
