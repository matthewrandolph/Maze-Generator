using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridScript : MonoBehaviour {

    [SerializeField] private Transform CellPrefab;
    [SerializeField] private Vector3 Size;
    [SerializeField] private Transform[,] Grid;

    [SerializeField] private GameObject playerCharacter;
 
    // Use this for initialization
    void Start () {
        CreateGrid();
        SetRandomNumbers();
        SetAdjacents();
        SetStart();

        // FindNext looks for the lowest weight adjacent to all the cells in the Set, and adds that cell to the set.
        //  A cell is disqualified if it has two open neighbors. This makes the maze full of deadends.
        // FindNext also will invoke itself as soon as it finishes, allowing it to loop indefinitely until the
        //  evoke is canceled when we detect our maze is done.
        FindNext();
	}
    
    void CreateGrid()
    {
        Grid = new Transform[(int)Size.x, (int)Size.z];
        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.z; z++)
            {
                Transform newCell;
                newCell = Instantiate(CellPrefab, new Vector3(x, 0, z), Quaternion.identity);
                newCell.name = string.Format("({0},0,{1})", x, z); // Works the same as:    newCell.name = "(" + x + ",0," + z + ")";
                newCell.parent = transform;
                newCell.GetComponent<CellScript>().Position = new Vector3(x, 0, z);
                Grid[x, z] = newCell;
            }
        }
        Camera.main.transform.position = Grid[(int)(Size.x / 2), (int)(Size.z / 2)].position + Vector3.up * 20f;
        Camera.main.orthographicSize = Mathf.Max(Size.x, Size.z)/2f + 5f;
    }

    void SetRandomNumbers()
    {
        foreach(Transform child in transform)
        {
            int weight = Random.Range(0, 10);
            child.GetComponentInChildren<TextMeshPro>().text = weight.ToString();
            child.GetComponent<CellScript>().Weight = weight;
        }
    }

    void SetAdjacents()
    {
        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.z; z++)
            {
                Transform cell;
                cell = Grid[x, z];
                CellScript cScript = cell.GetComponent<CellScript>();
                if(x - 1 >= 0)
                {
                    cScript.Adjacents.Add(Grid[x - 1, z]);
                }
                if (x + 1 < Size.x)
                {
                    cScript.Adjacents.Add(Grid[x + 1, z]);
                }
                if (z - 1 >= 0)
                {
                    cScript.Adjacents.Add(Grid[x, z - 1]);
                }
                if (z + 1 < Size.z)
                {
                    cScript.Adjacents.Add(Grid[x, z + 1]);
                }
                cScript.Adjacents.Sort(SortByLowestWeight);
            }
        }
    }

    int SortByLowestWeight(Transform inputA, Transform inputB)
    {
        int a = inputA.GetComponent<CellScript>().Weight; // a's weight
        int b = inputB.GetComponent<CellScript>().Weight;
        return a.CompareTo(b);
    }

    public List<Transform> Set;

    // A list of lists. Here is the structure:
    //  AdjSet {
    //      [ 0 ] is a list of all the cells that have a weight of 0, and are adjacent to the cells in Set.
    //      [ 1 ] is a list of all the cells that have a weight of 1, and are adjacent to the cells in Set.
    //      [ 2 ] is a list of all the cells that have a weight of 2, and are adjacent to the cells in Set.
    //      etc...
    //      [ 9 ] is a list of all the cells that have a weight of 9, and are adjacent to the cells in Set.
    //  }
    //
    // Note: Multiple entries of the same cell will not appear as duplicates.
    // (Some adjacent cells will be next to two or three or four other Set cells). They are only recorded in the AdjSet once.
    public List<List<Transform>> AdjSet;

    void SetStart()
    {
        // Create a new List<Transform> for Set.
        Set = new List<Transform>();
        // Also, we create a new List<List<Transform>> and in the For loop, List<Transform>'s.
        AdjSet = new List<List<Transform>>();
        for (int i = 0; i < 10; i++)
        {
            AdjSet.Add(new List<Transform>());
        }

        // The start of our Maze/Set will be color coded Green, so we apply that to the renderer's material's color here.
        Grid[0, 0].GetComponent<Renderer>().material.color = Color.green;
        // Now, we add the first cell to the set.
        AddToSet(Grid[0, 0]);
    }

    void AddToSet(Transform toAdd)
    {
        // Adds the toAdd object to the set. The toAdd transform is sent as a parameter.
        Set.Add(toAdd);
        // For every adjacent next to the toAdd object:
        foreach(Transform adj in toAdd.GetComponent<CellScript>().Adjacents)
        {
            // Add one to the adjacent's CellScript's AdjacentsOpened
            adj.GetComponent<CellScript>().AdjacentsOpened++;
            // If
            //  a) The Set does not contain the adjacent (cells in the set are not valid to be entered as adjacentCells as well).
            //   and
            //  b) The AdjSet does not already contain the adjacent cell.
            //   then...
            if (!Set.Contains(adj) && !(AdjSet[adj.GetComponent<CellScript>().Weight].Contains(adj)))
            {
                // ...add this new cell into the proper AdjSet sub-list.
                AdjSet[adj.GetComponent<CellScript>().Weight].Add(adj);
            }
        }
    }

    void FindNext()
    {
        // We create an empty Transform variable to store the next cell in.
        Transform next;
        // Perform this loop
        // While:
        //  The proposed next gameObject's AdjacentsOpened is less than or equal to 2.
        //   This is to ensure the maze-like structure.
        do
        {
            // We'll initially assume that each sub-list of AdjSet is empty and try to prove that assumption false in the for loop.
            // This boolean value will keep track.
            bool empty = true;
            // We'll also take a note of which list is the Lowest, and store it in this variable.
            int lowestList = 0;
            for (int i = 0; i < 10; i++)
            {
                // We loop through each sub-list in the AdjSet list of lists, until we find one with a count of more than 0.
                //  If there are more than 0 items in the sub-list, it is not empty.
                // We then stop the loop by using the break keyword; We've found the lowest sub-list, so there is no need 
                //  to continue searching.
                lowestList = i;
                if (AdjSet[i].Count > 0)
                {
                    empty = false;
                    break;
                }
            }
            // There is a chance that none of the sub-lists of AdjSet will have any items in them.
            // If this happens, then we have no more cells to open, and are done with the maze production.
            if (empty)
            {
                // If we finish, as stated and determined above, display a message to the DebugConsole 
                //  that includes how many seconds it took to finish.
                Debug.Log("We're Done, " + Time.timeSinceLevelLoad + " seconds taken");
                // Then cancel our recursive invokes of the FindNext function, as we're done with the maze.
                // If we allowed the invokes to keep going, we will receive an error.
                CancelInvoke("FindNext");
                // Set.Count-1 is the index of the last element in Set, or the last cell we opened.
                // This will be marked as the end of our maze, and so we mark it red.
                Set[Set.Count - 1].GetComponent<Renderer>().material.color = Color.red;
                // Here's an extra something. Every cell in the grid that is not in the set
                // will be moved one unit up and turned black. (The default color was changed from black to clear).
                // If you instantiate a FirstPersonController in the maze now, you can actually try walking through it.
                // It's really hard.
                foreach (Transform cell in Grid)
                {
                    if (!Set.Contains(cell))
                    {
                        cell.Translate(Vector3.up);
                        cell.GetComponent<Renderer>().material.color = Color.black;
                    }
                }

                Destroy(Camera.main.gameObject);
                Instantiate(playerCharacter, new Vector3(0,1,0), Quaternion.identity);
                
                return;
            }
            // If we did not finish, then:
            // 1. Use the smallest sub-list in AdjSet as found earlier with the lowestList variable.
            // 2. With that smallest sub-list, take the first element in tha tlist, and use it as the 'next'.
            next = AdjSet[lowestList][0];
            // Since we do not want the same cell in both AdjSet and Set, remove this 'next' variable from AdjSet.
            AdjSet[lowestList].Remove(next);
        } while (next.GetComponent<CellScript>().AdjacentsOpened >= 2); // 2 is the original number
        // The 'next' transform's material color becomes white.
        next.GetComponent<Renderer>().material.color = Color.white;
        // We add this 'next' tranform to the Set our function.
        AddToSet(next);
        // Recursively call this function as soon as this function finishes.
        Invoke("FindNext", 0);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene(0);
        }
    }
}
