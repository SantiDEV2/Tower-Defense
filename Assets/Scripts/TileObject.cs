using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "Scriptable Objects/TileScriptableObject")]
public class TileObject : ScriptableObject
{
    public enum CellType { Path, Ground }

    public CellType cellType;
    public GameObject tilePrefab;
}
