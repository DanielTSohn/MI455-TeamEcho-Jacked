using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "ScriptableObjects/GridData")]
public class GridDataBase : ScriptableObject
{
    public Dictionary<Vector3Int, GameObject> Cells;      
}
