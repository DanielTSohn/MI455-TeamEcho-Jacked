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

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponent<Grid>();
        if (cell) { InitializeTiles(); }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitializeTiles()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                Vector3 WorldLocation = grid.CellToWorld(new Vector3Int(i, j, 0));
                Instantiate(cell, WorldLocation, grid.transform.rotation, transform);
            }
        }
    }

}
