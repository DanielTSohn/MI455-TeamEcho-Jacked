using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGridInteraction : MonoBehaviour
{
    private GridSystem gs;
    // Start is called before the first frame update
    void Start()
    {
        gs = GameObject.FindGameObjectWithTag("GridSystem").GetComponent<GridSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*void DFSUtil(int v, bool[] visited)
    {
        // Mark the current node as visited
        // and print it
        visited[v] = true;

        // Recur for all the vertices
        // adjacent to this vertex
        List<int> vList = adj[v];
        foreach (var n in vList)
        {
            if (!visited[n])
                DFSUtil(n, visited);
        }
    }*/

    void OnCellHit(Vector3Int v)
    {
        // Get reference to the cell that was hit
        Cell currentCell = gs.cellsHash[v].GetComponent<Cell>();
        currentCell.destroyed = true;

        // Check if there are two or more adjacent, broken cells
        int numDestroyedNeighbors = 0;
        for (int i = 0; i < currentCell.neighbors.Count; i++)
        {
            if (currentCell.neighbors[i].GetComponent<Cell>().destroyed) { numDestroyedNeighbors++; }
        }

        if (numDestroyedNeighbors > 1)
        {
            // Find which side of the line to begin DFS on
            // DFS through all cells, disbale each of them
            // Once each cell has been disabled, spawn particle effect
        }

    }
}
