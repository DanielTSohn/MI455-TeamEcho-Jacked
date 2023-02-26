using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using DestroyIt;

public class PlayerGridInteraction : MonoBehaviour
{
    [Header("Cutout Settings")]
    [Tooltip("Max breadth of BFS on tile hit (dictates maximum shape cutout size")]
    [SerializeField] private int maxCutoutBreadthSize = 25;
    [Tooltip("Delay between individual BFS steps (helps with optimization)")]
    [SerializeField] private float fillDelay = .2f;

    private HashSet<GridSystem> gridSystems = new HashSet<GridSystem>();
    private GridSystem gs;
    private Cell currentCell;

    [SerializeField] private GameObject debugSphere;
    private HashSet<Dictionary<Vector3Int, Cell>> activeCutoutInstances = new HashSet<Dictionary<Vector3Int, Cell>>();
    // This is the "cutout" cell which we need to treat as broken during and DFSs
    private Cell cutoutCellToIgnore;

    static Vector3Int
        LEFT = new Vector3Int(-1, 0, 0),
        RIGHT = new Vector3Int(1, 0, 0),
        DOWN = new Vector3Int(0, -1, 0),
        DOWNLEFT = new Vector3Int(-1, -1, 0),
        DOWNRIGHT = new Vector3Int(1, -1, 0),
        UP = new Vector3Int(0, 1, 0),
        UPLEFT = new Vector3Int(-1, 1, 0),
        UPRIGHT = new Vector3Int(1, 1, 0);

    static Vector3Int[] yIsEven =
      { LEFT, RIGHT, DOWN, DOWNLEFT, UP, UPLEFT };
    static Vector3Int[] yIsOdd =
          { LEFT, RIGHT, DOWN, DOWNRIGHT, UP, UPRIGHT };

    private void FindValidNeighbors(Vector3Int node, List<Cell> validCells)
    {
        Vector3Int[] directions = (node.y % 2) == 0 ? yIsEven : yIsOdd;
        foreach (var direction in directions)
        {
            GameObject cellCheck;
            Vector3Int neighborPos = node + direction;
            if (gs.cellsHash.TryGetValue(neighborPos, out cellCheck))
            {
                if (cellCheck) { validCells.Add(cellCheck.GetComponent<Cell>()); }
            }
        }
    }

    private void FloodValidNeighbors(Vector3Int node, Dictionary<Vector3Int, Cell> validCells)
    {
        Vector3Int[] directions = (node.y % 2) == 0 ? yIsEven : yIsOdd;
        foreach (var direction in directions)
        {
            GameObject cellCheck;
            Vector3Int neighborPos = node + direction;
            if (gs.cellsHash.TryGetValue(neighborPos, out cellCheck))
            {
                // If we've already visited this node, then don't recurse
                // Also make sure we don't flood over the tile the player is currently touching
                if (cellCheck && !validCells.ContainsKey(neighborPos) && cellCheck.GetComponent<Cell>() != cutoutCellToIgnore)
                {
                    validCells.Add(neighborPos, cellCheck.GetComponent<Cell>());
                    if (debugSphere)
                    {
                        var thingy = Instantiate(debugSphere,
                            gs.GetComponent<Grid>().CellToWorld(neighborPos), gs.transform.rotation);
                        Destroy(thingy, 0.5f);
                    }
                    FloodValidNeighbors(neighborPos, validCells);
                }
            }
        }
    }

    private int GetBrokenNeighborCount(Vector3Int node)
    {
        int brokenNeighborCount = 0;
        Vector3Int[] directions = (node.y % 2) == 0 ? yIsEven : yIsOdd;
        foreach (var direction in directions)
        {
            GameObject cellCheck;
            Vector3Int neighborPos = node + direction;
            if (gs.cellsHash.TryGetValue(neighborPos, out cellCheck))
            {
                if (!cellCheck) { brokenNeighborCount++; }
            }
        }

        print("Amount of broken neighbors to this tile: " + brokenNeighborCount);
        return brokenNeighborCount;
    }

    private void Awake()
    {
        //StartCoroutine(DelayedStart());
        SceneManager.sceneLoaded += GetGrids;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= GetGrids;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void GetGrids(Scene scene, LoadSceneMode lsMode)
    {
        if (scene.name == "ConstructionSite")
        {
            var thing = GameObject.FindGameObjectsWithTag("GridSystem");
            for (int i = 0; i < thing.Length; i++)
            {
                gridSystems.Add(thing[i].GetComponent<GridSystem>());
            }
        }

    }

    private IEnumerator DelayedStart()
    {
        if (!SpawnPlayers.Instance.SceneReady)
        {
            yield return new WaitUntil(() => SpawnPlayers.Instance.SceneReady);
        }

    }

    public void OnCellHit(Vector3Int v, GridSystem g)
    {
        if (gridSystems.Contains(g)) { gs = g; }
        else { print("Grid system not found for active cell"); return; }
        gs.DecrementTotalRemainingCells();
        // Get reference to the cell that was hit
        currentCell = gs.cellsHash[v].GetComponent<Cell>();
        cutoutCellToIgnore = currentCell;
        int brokenNeighborCount = GetBrokenNeighborCount(v);

        // If we have more than one broken cell bordering this one, there is a chance we've just cut out a shape
        if (brokenNeighborCount > 1)
        {
            List<Cell> validNeighbors = new List<Cell>();
            FindValidNeighbors(v, validNeighbors);
            //FindValidNeighbors(v, out validNeighbors);
            foreach (Cell c in validNeighbors)
            {
                Dictionary<Vector3Int, Cell> cutoutInstance = new Dictionary<Vector3Int, Cell>();
                activeCutoutInstances.Add(cutoutInstance);
                Flood(c.gridLocation.x, c.gridLocation.y, cutoutInstance);
                print("Flooding a tile");

                // After doing a DFS on each tile, check if the amount we just selected in
                // the search is less than the total number of remaining tiles. If so,
                // this means we found an island, and we can destroy said island, and exit the loop early.
                /*if (cutoutInstance.Count < gs.GetTotalRemainingCells()) 
                {
                    print("Valid island parent found, location of parent is: " + c.gridLocation);
                    foreach(Cell cell in cutoutInstance.Values)
                    {
                        if (cell.TryGetComponent(out Destructible tileDestroy))
                        {
                            tileDestroy.ApplyDamage(1);
                            gs.DecrementTotalRemainingCells();
                        }
                    }
                    break; 
                }*/
            }
            int smallestCutout = 99999999;
            int largestCutout = 0;
            Dictionary<Vector3Int, Cell> shapeToCutout = new Dictionary<Vector3Int, Cell>();
            foreach (var d in activeCutoutInstances)
            {
                if (d.Count < smallestCutout)
                {
                    smallestCutout = d.Count;
                    shapeToCutout = d;
                }
                if (d.Count > largestCutout) { largestCutout = d.Count; }
            }

            // If our smallest cutout is equal to our largest, it means that every DFS we did was over the
            // same subset of tiles, i.e. the entire map. In this case, we don't delete anything.
            if (smallestCutout != largestCutout)
            {
                //print("Valid island parent found, location of parent is: " + c.gridLocation);
                foreach (Cell cell in shapeToCutout.Values)
                {
                    if (cell.TryGetComponent(out Destructible tileDestroy))
                    {
                        tileDestroy.ApplyDamage(1);
                        gs.DecrementTotalRemainingCells();
                    }
                }
            }

            foreach (var d in activeCutoutInstances)
            {
                print("Size of active cutout instance: " + d.Count);
            }


            activeCutoutInstances.Clear();

        }

    }

    private void Flood(int x, int y, Dictionary<Vector3Int, Cell> cutoutInstance)
    {
        if (x >= 0 && x < gs.GetGridWidth() && y >= 0 && y < gs.GetGridHeight())
        {
            Vector3Int newVec = new Vector3Int(x, y, 0);

            // An optimization wherein if a tile we're about to check has already been used in
            // a separate DFS, we will skip this DFS, since we know it's going to cover the same
            // tiles as the previous one
            /*foreach(Dictionary<Vector3Int, Cell> d in activeCutoutInstances)
            {
                if (d !=cutoutInstance)
                {
                    Cell c;
                    if (d.TryGetValue(newVec, out c))
                    {
                        if (c)
                        {
                            print("Skipping DFS for this tile");
                            return;
                        }
                    }
                }
                
            }*/

            //print("Flooding to position" + newVec);
            GameObject cellCheck;
            Cell curCell = null;
            if (gs.cellsHash.TryGetValue(newVec, out cellCheck))
            {
                curCell = cellCheck.GetComponent<Cell>();
            }

            // If the new cell hasn't been visited yet
            if (curCell)
            {
                cutoutInstance.Add(newVec, curCell);
                FloodValidNeighbors(newVec, cutoutInstance);
            }
        }
    }
}
