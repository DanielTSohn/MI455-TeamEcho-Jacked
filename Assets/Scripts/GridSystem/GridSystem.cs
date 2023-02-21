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

    public Dictionary<Vector3Int, GameObject> cellsHash { get; private set; } = new();

    private int totalRemainingCells = 0;

    [ExecuteInEditMode]
    public void InitializeTiles()
    {
#if UNITY_EDITOR
        if (TryGetComponent(out grid))
        {
            if(cellsHash != null) { RemoveTiles(); }
            else { cellsHash = new(); }
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    /*
                    Vector3Int vIntLocation = new Vector3Int(i, j, 0);
                    Vector3 WorldLocation = grid.CellToWorld(vIntLocation);
                    var newCell = Instantiate(cell, WorldLocation, grid.transform.rotation, transform);
                    newCell.GetComponent<Cell>().gridLocation = vIntLocation;
                    //AssignAsNeighbor(newCell, i, j);
                    */
                    Vector3Int vIntLocation = new Vector3Int(i, j, 0);
                    Vector3 WorldLocation = grid.CellToWorld(vIntLocation);
                    Instantiate(cell, WorldLocation, grid.transform.rotation, transform);
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
    }

    private void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject currCell = transform.GetChild(i).gameObject;
            Vector3Int gridPosition = grid.WorldToCell(currCell.transform.position);
            currCell.GetComponent<Cell>().gridLocation = gridPosition;
            cellsHash.Add(gridPosition, currCell);
        }
        /*
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                Vector3Int vIntLocation = new Vector3Int(i, j, 0);
                Vector3 WorldLocation = grid.CellToWorld(vIntLocation);
                var newCell = Instantiate(cell, WorldLocation, grid.transform.rotation, transform);
                newCell.GetComponent<Cell>().gridLocation = vIntLocation;
                cellsHash.Add(vIntLocation, newCell);
                //AssignAsNeighbor(newCell, i, j);
            }
        }
        */
        Debug.Log(cellsHash.Count + " cells generated");

        totalRemainingCells = gridWidth * gridHeight;
    }

    public int GetGridWidth() { return gridWidth; }
    public int GetGridHeight() { return gridHeight; }

    public int GetTotalRemainingCells() { return totalRemainingCells; }
    public void DecrementTotalRemainingCells() { totalRemainingCells--; }
}