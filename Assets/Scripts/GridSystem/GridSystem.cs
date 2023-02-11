using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Stuff")]
    [Tooltip("The actual grid")]
    [SerializeField] private Grid grid;

    [Header("Grid Settings")]
    [Tooltip("How many tiles wide the grid will be")]
    [SerializeField] private int gridWidth = 10;
    [Tooltip("How many tiles high the grid will be")]
    [SerializeField] private int gridHeight = 15;
    [Tooltip("Cell (hexagon) size")]
    [SerializeField] private float cellSize = 1.0f;
    [Tooltip("Cell class to spawn")]
    [SerializeField] private GameObject cell;

    [HideInInspector] public Dictionary<Vector3Int, GameObject> cellsHash = new Dictionary<Vector3Int, GameObject>();

    [ExecuteInEditMode]
    public void InitializeTiles()
    {
#if UNITY_EDITOR
        if (TryGetComponent(out grid))
        {
            if(cellsHash.Count > 0)
            {
                RemoveTiles();
            }
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    Vector3Int vIntLocation = new Vector3Int(i, j, 0);
                    Vector3 WorldLocation = grid.CellToWorld(vIntLocation);
                    var newCell = Instantiate(cell, WorldLocation, grid.transform.rotation, transform);
                    cellsHash.Add(vIntLocation, newCell);
                    AssignAsNeighbor(newCell, i, j);
                }
            }
        }
#endif
    }

    [ExecuteInEditMode]
    public void RemoveTiles()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        cellsHash.Clear();
    }

    // Takes a gameobject "Cell", and checks if it has neighbors. If so,
    // it assigns itself as a neighbor to each surrounding cell
    private void AssignAsNeighbor(GameObject g, int i, int j)
    {
        GameObject neighbor;
        // If there is a cell to the left of this one, make this current cell its right neighbor
        if (cellsHash.TryGetValue(new Vector3Int(i - 1, j, 0), out neighbor))
        {
            neighbor.GetComponent<Cell>().neighbors.Add(g);
        }
        // If there is a cell to the right of this one, make this current cell its left neighbor
        if (cellsHash.TryGetValue(new Vector3Int(i + 1, j, 0), out neighbor))
        {
            neighbor.GetComponent<Cell>().neighbors.Add(g);
        }
        // If there is a cell below this one, make this current cell its upper neighbor
        if (cellsHash.TryGetValue(new Vector3Int(i, j - 1, 0), out neighbor))
        {
            neighbor.GetComponent<Cell>().neighbors.Add(g);
        }
        // If there is a cell above this one, make this current cell its lower neighbor
        if (cellsHash.TryGetValue(new Vector3Int(i, j + 1, 0), out neighbor))
        {
            neighbor.GetComponent<Cell>().neighbors.Add(g);
        }
    }
}